#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gamedev;
using Gamedev.Entities;
using Games.Ring.Data;
using Utils.IO;
using Utils.IO.SerializerConverters;
using static Utils.Engine.MenuUtils;
using static Utils.Engine.Symbols;

namespace Games.Ring;

// TODO: Make JsonDeserializer be able to
// automatically detect json file references
// and replace them with the actual json content
// and then deserialize it into a single struct

public class Game : IGame
{
    public static Data.Data Data { get; private set; } = null!;
    public static Directory<Music, string[]> MusicDirectory { get; private set; } = null!;

    public void Start()
    {
        Data = LoadData<Data.Data>("data.json");
        MusicDirectory =
            Directory<Music, string[]>.LoadFromFileSystem(
                Path.Combine(AppContext.BaseDirectory, "Assets", Data.MusicDirectory));
        var saver = new Saver(Data.AppName);
        var save = saver.Load<Save>() ?? new Save();
        save.Index();
        var levels = Levels.LoadAll();
        LevelSession? currentSession;
        RootUi.PushScene(() => Menu([
            (MenuAction.Start, m => m.PushScene(() => GenericMenus.LevelSelectMenu(levels, StartLevel))),
            (MenuAction.Settings, m => m.PushScene(GenericMenus.DefaultSettings2D)),
            (MenuAction.Quit, m => m.Popup(GenericMenus.ExitConfirmation)),
        ] /*, style = "main-menu", layout = "main-menu"*/));
        return;

        void StartLevel(Level level)
        {
            currentSession = new LevelSession(level);
            RootUi.PushScene(() => new LevelUiScene(currentSession).Entity);
            currentSession.End += EndLevel;
        }

        void EndLevel(Record finalScore)
        {
            save.AddRecord(finalScore);
            saver.Save(save);
            currentSession?.Dispose();
            Root2D.PopScene();
            RootUi.ChangeScene(() => new ScoreUiScene(finalScore).Entity);
        }

        T LoadData<T>(string filename) where T : notnull =>
            GameDataLoader.LoadData<T>(filename, [ArrayToType.Deserialize]);
    }
}

public class ScoreUiScene
{
    public Entity Entity { get; }

    public ScoreUiScene(Record record)
    {
        var ui = Game.Data.Ui;
        var level = Levels.GetFresh(record.LevelId);
        var scene = Entities.Ui.Panel();
        var music = Game.MusicDirectory.IdItemBiMap.KeyToValue[level.Heading.MusicId];
        Entity = scene.Entity;
        
        var perfectsText = Ui.TextField().Configure(c => ConfigScoreText(c, record.Score.Perfect));
        var goodsText = Ui.TextField().Configure(c => ConfigScoreText(c, record.Score.Good));
        var missesText = Ui.TextField().Configure(c => ConfigScoreText(c, record.Score.Missed));
        var levelNameText = Ui.TextField().Configure(Title);
        var musicText = Ui.TextField().Configure(BodyText);
        var difficultyText = Ui.TextField().Configure(BodyText, DifficultyColor);
        
        levelNameText.Component.Text = level.Heading.Name;

        return;

        void DifficultyColor(ITextField textField)
        {
            textField.Color = ui.DifficultyColors.Length > level.Heading.Difficulty
                ? ui.DifficultyColors[(int)MathF.Round(level.Heading.Difficulty)]
                : ui.DifficultyColors.Last();
        }

        void BodyText(ITextField textField)
        {
            textField.FontSize = ui.FontSizeBody;
            textField.FontFamily = ui.FontFamilyBody;
        }

        void Title(ITextField textField)
        {
            textField.FontSize = ui.FontSizeHeading;
            textField.FontFamily = ui.FontFamilyHeading;
        }

        void ConfigScoreText(ITextField textField, int score)
        {
            textField.FontSize = ui.ScoreScene.PerfectFontSize;
            textField.FontFamily = ui.FontFamilyBody;
            textField.Text = score.ToString();
        }
    }
}

public class LevelUiScene
{
    public Entity Entity { get; }

    public LevelUiScene(LevelSession levelSession)
    {
        throw new NotImplementedException();
    }
}

public class LevelSession : IDisposable
{
    private static Rules Rules => Game.Data.Rules;

    private readonly Level _level;
    private readonly float _length;
    private readonly LevelBody _body;
    private readonly Pattern[] _patterns;
    private float _time;
    public Score Score = new();
    private List<Bullet> _bullets = [];
    private Ring _ring;

    public LevelSession(Level level)
    {
        _level = level;
        _ring = new Ring(level);
        _length = level.Heading.Length;
        _body = level.Body;
        _patterns = _body.Patterns;
        UpdateAdd(Update);
    }

    public event Action<Record>? End;

    public void Dispose()
    {
        UpdateRemove(Update);
    }

    private int _pointer;

    private void Update(float delta)
    {
        if (_time >= _length)
        {
            End?.Invoke(new Record(_level.Heading.Id, DateTime.Now, Score));
            Dispose();
            return;
        }

        while (_patterns.Length > _pointer && _patterns[_pointer].Time <= _time)
        {
            Spawn(_patterns[_pointer]);
            _pointer++;
        }

        for (var i = _bullets.Count - 1; i >= 0; i--)
        {
            var bullet = _bullets[i];
            if (!(bullet.Distance > Rules.RingDistance)) continue;
            _bullets.RemoveAt(i);
            UpdateRemove(bullet.Update);
            _ring.Score(ref Score, bullet.Angle);
        }

        _time += delta;
    }

    private void Spawn(Pattern pattern)
    {
        var formation = pattern.Formation;
        SpawnBullet(formation.AngleStart, formation.SpeedStart);
        if (formation.Delay == 0 || formation.Duration == 0) return;
        float time = 0;
        var count = 1;
        UpdateAdd(Spawning);

        return;

        void Spawning(float delta)
        {
            time += delta;
            while (time > formation.Delay * count)
            {
                count++;
                var x = time / formation.Duration;
                SpawnBullet(formation.AngleStart + (formation.AngleEnd - formation.AngleStart) * x,
                    formation.SpeedStart + (formation.SpeedEnd - formation.SpeedStart) * x);
            }
        }

        void SpawnBullet(float angle, float speed)
        {
            var bulletRoot = Entities.E2D.Node();
            var sprite = Entities.E2D.Sprite(Resources.TextureLoader.FromFileName(Game.Data.Sprites.Bullet));
            sprite.Component.Node.Transform.Rotation = angle;
            bulletRoot.AddChild(sprite);

            var bullet = new Bullet(bulletRoot, angle, speed);
            _bullets.Add(bullet);
            UpdateAdd(bullet.Update);
        }
    }
}

public class Ring
{
    private readonly Arcs _arcs;
    private float _rotation;

    private float Rotation
    {
        get => _rotation;
        set
        {
            const float twoPi = MathF.PI * 2f;
            value %= twoPi;
            if (value < 0)
                value += twoPi;
            _rotation = value;
        }
    }

    public Ring(Level level)
    {
        _arcs = new Arcs(level);
    }

    public void Score(ref Score score, float bulletAngle)
    {
        bulletAngle -= _rotation;
        var normalized = _arcs.Normalized;
        if (HasMatching(normalized.Normal)) score.AddPerfect();
        else if (HasMatching(normalized.Expanded)) score.AddGood();
        else score.AddMissed();

        return;

        bool HasMatching(IEnumerable<Arc> arcs)
        {
            var matching = arcs.Cast<Arc?>().FirstOrDefault(a => a!.Value.StartAngle >= bulletAngle);
            if (!matching.HasValue) return false;
            var a = matching.Value;
            return a.EndAngle > bulletAngle;
        }
    }
}
