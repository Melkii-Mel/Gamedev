// TODO: Maybe Some Sfx, not sure

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gamedev;
using Gamedev.Entities;
using Gamedev.InputSystem.ActionSystem;
using Gamedev.Resources;
using Primitives;
using Primitives.Shapes;
using Silk.NET.Maths;
using Utils.Engine.MovementControls;
using Utils.Extensions;
using Utils.IO;
using Utils.Noises;
using static Gamedev.EngineInstance;

namespace HistoryMuseum;

// ReSharper disable once UnusedType.Global
public class Game : IGame
{
    private Data _data = LoadData();
    private EntityComponent<INode2D> _root2D = InitRoot2D();
    private EntityComponent<INode2D>? _rootEntries;
    private UiController? _uiController;
    private Action<float>? _update;
    private float _time;

    public void Start()
    {
        E.Display.WindowedFullscreen();
        E.Update += d => _update?.Invoke((float)d);
        RegisterActions();
        Init();
        E.Music.Play("music.mp3");
        E.Music.Repeat = true;
    }

    public void Init()
    {
        Console.WriteLine("Reset");
        _time = 0;
        _rootEntries = E.Entities.E2D.Node();
        _root2D.AddChild(_rootEntries);
        E.EssentialSettings.SkipLocalization = true;
        E.Resources.TextureLoader.Defaults.Filter = false;
        SpawnUi();
        SpawnCharacter();
        SpawnRoom(Vector2D<int>.Zero);
        _update += d => _time += d;
    }

    public void Restart()
    {
        _actions = null;
        _discoveredEntries.Clear();
        _currentAmount = 0;
        E.Scene.RootUi.PopScene();
        E.Scene.Root2D.PopScene();
        _data = LoadData();
        _update = null;
        _root2D.Entity.Free();
        _root2D = InitRoot2D();
        Init();
    }

    private void RegisterActions()
    {
        var inputConfig = _data.Config.InputConfig;
        E.Input.RegisterActions([
            inputConfig.Up,
            inputConfig.Down,
            inputConfig.Left,
            inputConfig.Right,
            inputConfig.Close,
            inputConfig.ToggleHelp,
            inputConfig.HideWinPopup,
            inputConfig.Restart,
        ]);
    }

    private static EntityComponent<INode2D> InitRoot2D()
    {
        var root = E.Entities.E2D.Node();
        E.Scene.Root2D.PushScene(() => root.Entity);
        return root;
    }

    private static Data LoadData()
    {
        return Serializer.Deserialize<Data>(FileLoader.LoadTextFile("data.json")
                                            ?? throw LoadException()) ??
               throw LoadException();

        static Exception LoadException()
        {
            return new Exception("Failed to load game data file" + AppContext.BaseDirectory + " ...");
        }
    }

    public void SpawnCharacter()
    {
        var inputConfig = _data.Config.InputConfig;
        var (size, imageName, movementConfig, animation) = _data.Config.CharacterConfig;

        var characterRoot = E.Entities.E2D.Node();
        _root2D.AddChild(characterRoot);
        var sprite = E.Entities.E2D.Sprite(imageName != null
            ? E.Resources.TextureLoader.FromFileName(imageName)
            : null);
        sprite.Component.Node.Transform.Scale = size / sprite.Component.Texture?.Size.As<float>() ??
                                                _data.Config.EntryConfig.FallbackImageSize;
        sprite.Component.Pivot = Defaults.TopLeftPivot;
        var shape = new Box(size);
        var trigger = E.Entities.E2D.Trigger(shape);
        // trigger.Component.Node.Transform.Position = shape.Size / 2;

        characterRoot.AddChild(trigger);
        characterRoot.AddChild(sprite);

        var topDownSystem = new TopDownSystem(
            movementConfig,
            new TopDownState(_data.Config.SceneConfig.PlayerRelativePosition * E.Display.ViewportSize),
            update => _update += update,
            new TopDownDiscreteDirectionActionNames(inputConfig.Up.Name, inputConfig.Down.Name, inputConfig.Left.Name,
                inputConfig.Right.Name));
        topDownSystem.Init();
        var roomSize = E.Display.ViewportSize;
        var ignoreNextUpdate = false;
        topDownSystem.StateUpdated += state =>
        {
            if (ignoreNextUpdate)
            {
                ignoreNextUpdate = false;
                return;
            }

            var threshold = size.As<int>();
            var delta = Vector2D<int>.Zero;
            if (state.Position.X > roomSize.X + threshold.X)
            {
                state.Position.X = 0;
                delta.X = 1;
            }

            if (state.Position.X < 0 - threshold.X)
            {
                state.Position.X = roomSize.X;
                delta.X = -1;
            }

            if (state.Position.Y < 0 - threshold.Y)
            {
                state.Position.Y = roomSize.Y;
                delta.Y = -1;
            }

            if (state.Position.Y > roomSize.Y + threshold.Y)
            {
                state.Position.Y = 0;
                delta.Y = 1;
            }

            if (topDownSystem.State != state)
            {
                ignoreNextUpdate = true;
                topDownSystem.State = state;
            }

            if (delta != Vector2D<int>.Zero)
            {
                MoveToRoom(delta);
            }
        };
        var time = 0.0f;
        var baseScale = sprite.Component.Node.Transform.Scale;
        _update += d =>
        {
            var transform = sprite.Component.Node.Transform;
            transform.Scale = transform.Scale with
            {
                Y = baseScale.Y + MathF.Sin(time * MathF.PI * 2) * animation.Amplitude,
                X = baseScale.X * (topDownSystem.State.Velocity.X > -0.01 ? -1 : 1),
            };
            transform.Position = transform.Position with
            {
                Y = (1 - transform.Scale.Y / baseScale.Y) * size.Y,
            };
            time += d * (animation.Idle +
                         topDownSystem.State.Velocity.Length / movementConfig.MaxVelocity *
                         (animation.MaxSpeed - animation.Idle));
        };
        topDownSystem.StateUpdated += state => characterRoot.Component.Transform.Position = state.Position;
    }

    private Vector2D<int> _index;
    private IEntity? _tiles;
    private IEntity? _entries;

    public void SpawnRoom(Vector2D<int> index)
    {
        _index = index;
        _tiles?.Free();
        _entries?.Free();
        _tiles = SpawnTiles(index);
        _entries = SpawnEntries(index);
        _uiController?.UpdateIndexUi(index);
    }

    public void MoveToRoom(Vector2D<int> delta)
    {
        SpawnRoom(_index + delta);
    }

    public IEntity? SpawnTiles(Vector2D<int> index)
    {
        var tileConfig = _data.Config.TileConfig;
        var rng = new Random(0);
        var tilesParent = E.Entities.E2D.Node();
        tilesParent.Component.ZIndex = (int)ZIndex.Tiles;

        var tilesTexture = E.Resources.TextureLoader.FromFileName(_data.Config.TileConfig.ImageNames[
            (int)(rng.NextDouble() * _data.Config.TileConfig.ImageNames.Length)
        ]);
        if (tilesTexture is null)
        {
            E.DebugOutput.Error("Tile texture not found");
            return null;
        }

        var tileWorldSize = new Vector2D<float>(tileConfig.DefaultSize);
        tilesParent.Component.Transform.Position = tilesParent.Component.Transform.Position with
        {
            Y = tilesParent.Component.Transform.Position.Y + tileWorldSize.Y / 2,
        };
        var roomSize = E.Display.ViewportSize;
        var tilesNeeded = (roomSize / tileConfig.DefaultSize).Ceil();
        var noise = NoiseDefaults.DefaultTiled();
        noise.Scale = 0.5f;
        var loadedTiles = _data.Config.TileConfig.ImageNames.Select(n => E.Resources.TextureLoader.FromFileName(n))
            .ToArray();
        for (var x = 0; x < tilesNeeded.X; x++)
        for (var y = 0; y < tilesNeeded.Y; y++)
        {
            var tileTexture = loadedTiles[
                (int)MathF.Min(
                    MathF.Pow(MathF.Max(noise.Sample(tilesNeeded.X * index.X + x, tilesNeeded.Y * index.Y + y), 0),
                        1.3f) *
                    _data.Config.TileConfig.ImageNames.Length, _data.Config.TileConfig.ImageNames.Length - 1)
            ];
            var tileScale = tileWorldSize / tileTexture?.Size.As<float>() ?? Vector2D<float>.One;
            var sprite = E.Entities.E2D.Sprite(tileTexture);
            sprite.Component.Node.Transform.Scale = tileScale;
            sprite.Component.Node.Transform.Position =
                new Vector2D<float>((x + 0.5f) * tileWorldSize.X, y * tileWorldSize.Y);
            sprite.Component.Pivot = Defaults.TopLeftPivot;
            tilesParent.AddChild(sprite);
        }

        const bool testNoise = false;
        if (testNoise)
        {
            noise.Scale = 1 / 100000f;
            var max = -1f;
            var min = 2f;
            for (var i = 0; i < 1000000 * (testNoise ? 1 : 0); i++)
            {
                if (i % 100 == 0)
                {
                    E.DebugOutput.Info(noise.Sample(i, i).ToString(CultureInfo.InvariantCulture));
                }

                max = MathF.Max(max, noise.Sample(i, i));
                min = MathF.Min(min, noise.Sample(i, i));
            }

            E.DebugOutput.Info("Max " + max.ToString(CultureInfo.InvariantCulture));
            E.DebugOutput.Info("Min " + min.ToString(CultureInfo.InvariantCulture));
        }

        _root2D.AddChild(tilesParent);
        return tilesParent.Entity;
    }

    public IEntity SpawnEntries(Vector2D<int> index)
    {
        var entryConfig = _data.Config.EntryConfig;

        var countPerRoom = _data.Config.SceneConfig.EntriesPerRoom;
        // var availableSpaceRect =
        // new Rectangle<float>(sceneConfig.RoomSize * sceneConfig.EntriesPaddingPercentage.Origin,
        // sceneConfig.RoomSize - sceneConfig.RoomSize * sceneConfig.EntriesPaddingPercentage.Size);
        // var placeRect = availableSpaceRect with { Size = availableSpaceRect.Size - entryConfig.Size };

        // var availableSpaceRectAspectRatio = availableSpaceRect.Size.X / availableSpaceRect.Size.Y;
        var entryAspect = entryConfig.Size.X / entryConfig.Size.Y;

        var screenSize = E.Display.ViewportSize;
        var screenAspectRatio = screenSize.X / screenSize.Y;
        var columns = (int)MathF.Max(1,
            (int)MathF.Round(MathF.Sqrt(countPerRoom * screenAspectRatio / entryAspect)));
        var rows = (int)MathF.Ceiling((float)countPerRoom / columns);
        var spacePerEntry = screenSize / new Vector2D<float>(columns + 1, rows + 1);
        var placeRect = new Rectangle<float>(spacePerEntry / 2, screenSize);

        var root = E.Entities.E2D.Node();
        var entries = _data.Entries.ToArray();
        var rng = new Random(index.GetHashCode());
        E.DebugOutput.Info((rng.Next() + rng.Next() + rng.Next()).ToString());
        for (var i = entries.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (entries[i], entries[j]) = (entries[j], entries[i]);
        }

        for (var i = 0; i < countPerRoom; i++)
        {
            if (_data.Config.EntryConfig.MissingChance > rng.NextDouble()) continue;

            var entry = entries[i];
            var x = i / columns;
            var y = i % columns;

            SpawnEntry(root.Entity, entry, new Vector2D<float>(
                placeRect.Origin.X + placeRect.Size.X * y / columns - entryConfig.Size.X * entry.Scale / 2,
                placeRect.Origin.Y + placeRect.Size.Y * x / rows - entryConfig.Size.Y * entry.Scale / 2), rng);
        }

        _rootEntries?.AddChild(root);
        return root.Entity;
    }

    private readonly List<(Entry, ITexture?)> _activeEntries = [];

    private void SpawnEntry(IEntity root, Entry entry, Vector2D<float> origin, Random random)
    {
        var entryConfig = _data.Config.EntryConfig;
        var entryTexture = E.Resources.TextureLoader.FromFileName(entry.ImageName);
        var entryRoot = E.Entities.E2D.Node();
        entryRoot.Component.Transform.Position = origin + entryConfig.Size * entry.Scale +
                                                 (float)random.NextDouble() *
                                                 entryConfig.RandomDisplacement;
        entryRoot.Component.Transform.Scale = new Vector2D<float>(entry.Scale);
        UpdateModulation();
        root.AddChild(entryRoot);

        SpawnStand();
        SpawnTrigger();
        SpawnImage();
        return;

        void SpawnStand()
        {
            var standTexture = E.Resources.TextureLoader.FromFileName(entryConfig.StandImageName);
            if (standTexture == null) return;
            var stand = E.Entities.E2D.Sprite(standTexture);
            var standComponent = stand.Component;
            standComponent.Pivot = Defaults.TopLeftPivot;
            var standSize = _data.Config.EntryConfig.Size;
            standComponent.Node.Transform.Scale = standSize / standTexture.Size.As<float>();
            entryRoot.AddChild(stand);
        }

        void Enter()
        {
            _activeEntries.Add((entry, entryTexture));
            UpdateEntry();
            UpdateEntriesCounter(entry);
            UpdateModulation();
        }

        void Exit()
        {
            _activeEntries.Remove((entry, entryTexture));
            UpdateEntry();
        }

        void UpdateEntry()
        {
            if (_activeEntries.Count > 0)
            {
                ShowInfo();
            }
            else
            {
                HideInfo();
            }
        }

        void UpdateModulation()
        {
            entryRoot.Component.Modulation = entryRoot.Component.Modulation with
            {
                A = _discoveredEntries.Contains(entry) ? 1 : entryConfig.NewEntryOpacity,
            };
        }

        void SpawnTrigger()
        {
            var trigger =
                E.Entities.E2D.Trigger(
                    new Box(new Vector2D<float>(entryConfig.Size.X * entryConfig.TriggerWidthCoefficient,
                        entryConfig.TriggerHeightUp + entryConfig.TriggerHeightDown)),
                    Defaults.CenterPivot);
            var triggerComponent = trigger.Component;
            var plate = E.Entities.E2D.Sprite(E.Resources.TextureLoader.FromFileName(entryConfig.PlateImageName));
            plate.Component.Node.Transform.Scale = new Vector2D<float>(entryConfig.PlateScale);
            plate.Component.Node.Transform.Position = new Vector2D<float>(0, entryConfig.PlateDisplacement);
            plate.Entity.Free();

            triggerComponent.Node.Transform.Position =
                new Vector2D<float>(0, entryConfig.Size.Y - entryConfig.TriggerHeightUp);
            // plate.Component.Node.Transform.Position = triggerComponent.Node.Transform.Position;
            trigger.AddChild(plate);
            triggerComponent.OnEnter += _ => Enter();
            triggerComponent.OnExit += _ => Exit();

            entryRoot.AddChild(trigger);
        }

        void SpawnImage()
        {
            var image = E.Entities.E2D.Sprite(
                E.Resources.TextureLoader.FromFileName(entry.ImageName)
            );
            var imageComponent = image.Component;
            var transform = imageComponent.Node.Transform;
            imageComponent.Texture ??= E.Resources.TextureLoader.FromFileName(entryConfig.FallbackImage);
            imageComponent.Pivot = Defaults.TopLeftPivot;
            transform.Position = new Vector2D<float>(0, -entryConfig.Elevation);
            var basePosition = transform.Position;
            imageComponent.Node.Transform.Scale =
                (_data.Config.EntryConfig.Size / imageComponent.Texture?.Size.As<float>() ??
                 _data.Config.EntryConfig.FallbackImageSize) * entryConfig.EntryImageScale;
            entryRoot.AddChild(image);
            var time = (float)new Random().NextDouble() * MathF.PI * entryConfig.LevitationFrequency;

            _update += OnUpdate;

            return;

            void OnUpdate(float f)
            {
                if (!image.Entity.IsValid)
                {
                    _update -= OnUpdate;
                    return;
                }

                time += f;
                transform.Position = basePosition with
                {
                    Y = basePosition.Y +
                        MathF.Sin(time * MathF.PI * entryConfig.LevitationFrequency *
                                  (_discoveredEntries.Contains(entry) ? 1 : 0.3f)) * entryConfig.LevitationAmplitude
                };
            }
        }

        void ShowInfo()
        {
            var e = _activeEntries.Last();
            _uiController?.SetTitle(e.Item1.Title);
            _uiController?.SetTexture(e.Item2);
            _uiController?.SetDescription(e.Item1.Description);
            _uiController?.Show();
        }

        void HideInfo()
        {
            _uiController?.Hide();
        }
    }

    private void UpdateEntriesCounter(Entry entry)
    {
        var added = _discoveredEntries.Add(entry);
        if (added)
        {
            _uiController?.RaiseCounter();
        }
    }

    private Action<InputActionEventArgs<bool>>? _actions;
    private readonly HashSet<Entry> _discoveredEntries = [];
    private int _currentAmount;

    private void SpawnUi(bool visible = false)
    {
        var inputConfig = _data.Config.InputConfig;
        var uiConfig = _data.Config.UiConfig;
        var rootUi = E.Entities.Ui.Control();
        E.Scene.RootUi.PushScene(() => rootUi.Entity);
        var roomIndex = E.Entities.Ui.TextField();
        ConfigText(roomIndex.Component, uiConfig.DescriptionTextConfig);
        rootUi.AddChild(roomIndex);
        var helpPanel = E.Entities.Ui.Panel();
        var helpText = E.Entities.Ui.TextField();
        var counter = E.Entities.Ui.TextField();
        rootUi.AddChild(counter);
        counter.Component.Control.Bounds = counter.Component.Control.Bounds with
        {
            Origin = new Vector2D<float>(0, uiConfig.DescriptionTextConfig.FontSize + 10),
        };
        ConfigText(counter.Component, uiConfig.TitleTextConfig);
        var timer = E.Entities.Ui.TextField();
        rootUi.AddChild(timer);
        timer.Component.Control.Bounds = timer.Component.Control.Bounds with
        {
            Origin = new Vector2D<float>(0,
                uiConfig.DescriptionTextConfig.FontSize + 16 + counter.Component.Control.Bounds.Origin.Y),
        };
        ConfigText(timer.Component, uiConfig.DescriptionTextConfig);
        var winPopup = E.Entities.Ui.Panel();
        winPopup.Component.Control.Visible = false;
        var winText = E.Entities.Ui.TextField();
        rootUi.AddChild(winPopup);
        winText.Component.Text = "Вы нашли все отголоски!\n" +
                                 "Продолжайте исследовать бесконечные коридоры эпохи Возрождения\n" +
                                 "или попробуйте собрать все отголоски за время!\n" +
                                 "Нажмите E, чтобы спрятать это окно и продолжить исследование,\n" +
                                 "R, чтобы начать сначала,\n" +
                                 "или Q, чтобы закрыть игру...";
        ConfigText(winText.Component, uiConfig.TitleTextConfig);
        _actions = args =>
        {
            if (args.Name == inputConfig.ToggleHelp.Name && args.Value)
            {
                helpPanel.Component.Control.Visible = !helpPanel.Component.Control.Visible;
            }

            if (args.Name == inputConfig.HideWinPopup.Name && args.Value)
            {
                _uiController?.HideWinPopup();
            }

            if (args.Name == inputConfig.Close.Name && !args.Value)
            {
                E.Application.Quit();
            }

            if (args.Name == inputConfig.Restart.Name && !args.Value)
            {
                Restart();
            }
        };
        helpPanel.AddChild(helpText);
        E.Input.Dispatcher.AddListener(_actions);
        helpText.Component.Text = _data.Help + "\n\n\n" + string.Join("\n", _data.Sources);
        helpText.Component.AutoWrap = true;
        var panel = E.Entities.Ui.Panel();
        rootUi.AddChild(panel);
        rootUi.AddChild(helpPanel);
        SetVisible(visible);
        var image = E.Entities.Ui.Image();
        panel.AddChild(image);
        ConfigPanel(panel.Component);
        ConfigPanel(helpPanel.Component);
        ConfigPanel(winPopup.Component);
        var title = E.Entities.Ui.TextField();
        var description = E.Entities.Ui.TextField();
        title.Component.AutoWrap = true;
        description.Component.AutoWrap = true;
        ConfigText(title.Component, uiConfig.TitleTextConfig);
        ConfigText(description.Component, uiConfig.DescriptionTextConfig);
        ConfigText(helpText.Component, uiConfig.DescriptionTextConfig);
        panel.AddChild(title);
        panel.AddChild(description);
        _update += _ =>
        {
            var screenSize = E.Display.ViewportSize;
            var panelBoundsSize = screenSize * (1 - uiConfig.PanelRelativePadding * 2);
            panel.Component.Control.Bounds =
                new Rectangle<float>(screenSize * uiConfig.PanelRelativePadding, panelBoundsSize);
            helpPanel.Component.Control.Bounds = panel.Component.Control.Bounds;
            helpText.Component.Control.Bounds = panel.Component.Control.Bounds with { Origin = Vector2D<float>.Zero };
            winPopup.Component.Control.Bounds = panel.Component.Control.Bounds;
            winText.Component.Control.Bounds = panel.Component.Control.Bounds with { Origin = Vector2D<float>.Zero };
            winText.Component.HAlignment = HAlignment.Center;
            winText.Component.VAlignment = VAlignment.Center;
            title.Component.Control.Bounds = panel.Component.Control.Bounds with { Origin = Vector2D<float>.Zero };
            description.Component.Control.Bounds = title.Component.Control.Bounds;
            var contentBounds = new Rectangle<float>(panelBoundsSize * uiConfig.PanelRelativeMargin,
                panelBoundsSize - panelBoundsSize * uiConfig.PanelRelativeMargin * 2);
            helpText.Component.Control.Bounds = contentBounds;
            title.Component.Control.Bounds = contentBounds;
            description.Component.Control.Bounds = contentBounds;
            var imageSize = (image.Component.Texture?.Size.As<float>() ??
                             _data.Config.EntryConfig.FallbackImageSize) * uiConfig.ImageSizeCoefficient;
            image.Component.Control.Bounds = contentBounds with
            {
                Size = imageSize,
                Origin = contentBounds.Origin with { X = contentBounds.Origin.X + contentBounds.Size.X - imageSize.X },
            };
            timer.Component.Text = $"Длительность исследования: {(int)(_time / 60):00}:{(int)(_time % 60):00}";
        };

        winPopup.AddChild(winText);
        _uiController = new UiController(
            () => SetVisible(true),
            () => SetVisible(false),
            t => image.Component.Texture = t,
            s => title.Component.Text = s,
            s => description.Component.Text = s,
            v => roomIndex.Component.Text = $"[{v.X} {v.Y}]",
            () =>
            {
                _currentAmount++;
                counter.Component.Text = $"Найдено Отголосков: {_currentAmount}";
                if (_currentAmount >= _data.Entries.Length)
                {
                    _uiController?.ShowWinPopup();
                }
            },
            () => { winPopup.Component.Control.Visible = true; },
            () => { winPopup.Component.Control.Visible = false; }
        );

        return;

        void SetVisible(bool b)
        {
            var time = b ? 0 : 0.3f;
            // panel.Component.Control.Visible = b;
            _update += ChangeVisibility;
            // TODO: Extract value
            return;

            void ChangeVisibility(float d)
            {
                var c = panel.Component.Control;
                time += b ? d : -d;
                if (time > 0.3)
                {
                    _update -= ChangeVisibility;
                    c.Modulation = c.Modulation with { A = 1 };
                }

                if (time <= 0)
                {
                    _update -= ChangeVisibility;
                    c.Modulation = c.Modulation with { A = 0 };
                }

                c.Modulation = c.Modulation with { A = time / 0.3f };
            }
        }

        void ConfigText(ITextField component, TextConfig config)
        {
            component.FontSize = config.FontSize;
            component.FontFamily = config.FontFamily;
            component.HAlignment = config.HAlignment;
            component.VAlignment = config.VAlignment;
        }

        void ConfigPanel(IPanel p)
        {
            p.BackgroundColor = Color.ParseSmart("BB9457CC");
            p.BorderThickness = 10;
            p.BorderColor = Color.ParseSmart("BB945777");
        }
    }

    private record UiController(
        Action Show,
        Action Hide,
        Action<ITexture?> SetTexture,
        Action<string> SetTitle,
        Action<string> SetDescription,
        Action<Vector2D<int>> UpdateIndexUi,
        Action RaiseCounter,
        Action ShowWinPopup,
        Action HideWinPopup
    );
}

#region Data

public record Data(
    Entry[] Entries,
    string[] Sources,
    Config Config,
    string Help
);

public record Entry(
    string Title,
    string Description,
    string ImageName,
    float Scale = 1
);

public record Config(
    SceneConfig SceneConfig,
    EntryConfig EntryConfig,
    TileConfig TileConfig,
    UiConfig UiConfig,
    CharacterConfig CharacterConfig,
    InputConfig InputConfig
);

public record InputConfig(
    InputAction Up,
    InputAction Down,
    InputAction Left,
    InputAction Right,
    InputAction ToggleHelp,
    InputAction Close,
    InputAction HideWinPopup,
    InputAction Restart
);

public record CharacterConfig(
    Vector2D<float> Size,
    string? ImageName,
    TopDownConfig MovementConfig,
    Animation Animation
);

public record Animation(
    float Idle,
    float MaxSpeed,
    float Amplitude
);

public record SceneConfig(
    Vector2D<float> RoomSize,
    Rectangle<float> EntriesPaddingPercentage,
    Vector2D<float> PlayerRelativePosition,
    int EntriesPerRoom
);

public record EntryConfig(
    Vector2D<float> Size,
    string StandImageName,
    string PlateImageName,
    float PlateDisplacement,
    float PlateScale,
    float Elevation,
    float TriggerHeightUp,
    float TriggerHeightDown,
    float TriggerWidthCoefficient,
    Vector2D<float> FallbackImageSize,
    float EntryImageScale,
    string FallbackImage,
    Vector2D<float> RandomDisplacement,
    float NewEntryOpacity,
    float MissingChance,
    float LevitationFrequency,
    float LevitationAmplitude
);

public record TileConfig(
    string[] ImageNames,
    float DefaultSize
);

public record UiConfig(
    float PanelRelativePadding,
    float PanelRelativeMargin,
    TextConfig TitleTextConfig,
    TextConfig DescriptionTextConfig,
    float ImageSizeCoefficient
);

public record TextConfig(
    float FontSize,
    string FontFamily,
    HAlignment HAlignment,
    VAlignment VAlignment
);

public enum ZIndex
{
    Bg = -2,
    Tiles = -1,
}

#endregion
