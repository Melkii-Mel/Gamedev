using System;
using System.Collections.Generic;
using Gamedev;
using static Utils.Engine.MenuUtils;
using static Gamedev.EngineInstance;

namespace Games.Ring;

public class Game : IGame
{
    public void Start()
    {
        E.Scene.RootUi.PushScene(() => Menu([
            (MenuAction.Start, m => m.PushScene(() => GenericMenus.LevelSelectMenu(Levels()))),
            (MenuAction.Settings, m => m.PushScene(GenericMenus.DefaultSettings2D)),
            (MenuAction.Quit, m => m.Popup(GenericMenus.ExitConfirmation)),
        ]/*, style = "main-menu", layout = "main-menu"*/));
    }

    public List<GenericMenus.ILevel> Levels()
    {
        throw new NotImplementedException();
    }
}