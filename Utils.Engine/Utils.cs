using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gamedev;
using Gamedev.Entities;
using Gamedev.Entities.SceneManagement;
using Gamedev.Localization;

namespace Utils.Engine;

public static class MenuUtils
{
    public enum MenuAction
    {
        Start,
        Settings,
        Quit,
    }

    public static readonly ReadOnlyDictionary<MenuAction, MenuData> MenuActionDefaults = new(
        new Dictionary<MenuAction, MenuData>
        {
            { MenuAction.Start, new MenuData("Start") },
            { MenuAction.Settings, new MenuData("Settings") },
            { MenuAction.Quit, new MenuData("Quit") },
        });

    public static SceneRoot Menu(IEnumerable<(MenuAction, Action<SceneRoot>)> menuActions)
    {
        return Menu(menuActions.Select(a => (MenuActionDefaults[a.Item1], a.Item2)));
    }

    public static SceneRoot<IControl> Menu(IEnumerable<(MenuData, Action<SceneRoot>)> menuActions)
    {
        var sceneRoot = new SceneRoot<IControl>(EngineInstance.E.Entities.Ui.Control());
        sceneRoot.PushScene(() =>
        {
            var rootControl = EngineInstance.E.Entities.Ui.Control();
            foreach (var menuAction in menuActions)
            {
                var button = EngineInstance.E.Entities.Ui.Button();
                button.Component.Text = new Text(menuAction.Item1.Name);
                button.Component.OnClick += () => menuAction.Item2(sceneRoot);
                rootControl.Entity.AddChild(button.Entity);
            }

            return rootControl.Entity;
        });
        return sceneRoot;
    }

    public record MenuData(string Name);

    public static class GenericMenus
    {
        public static SceneRoot LevelSelectMenu(IEnumerable<ILevel> levels)
        {
            throw new NotImplementedException();
        }

        public static SceneRoot LevelSelectMenu<TLevel>(IEnumerable<TLevel> levels,
            Func<TLevel, IControl> levelDisplayOverride) where TLevel : ILevel
        {
            throw new NotImplementedException();
        }

        public static SceneRoot DefaultSettings2D()
        {
            throw new NotImplementedException();
        }

        public static SceneRoot ExitConfirmation()
        {
            throw new NotImplementedException();
        }

        public interface ILevel;
    }
}
