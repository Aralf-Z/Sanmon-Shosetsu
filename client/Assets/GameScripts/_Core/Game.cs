using Game.Config;
using Sanmon.Core;
using Sanmon.Module;
using Sanmon.Note;
using Sanmon.Syztem;

namespace GameScripts
{
    public static partial class Game
    {
        private class MyGame : IGetModule
            , IGetEntity
            , IGetSystem
            , IGetNote { }
        
        private static readonly MyGame game = new MyGame();

        public static GameModule Module => game.Module();
        
        public static AssetModule Asset => Module.Asset;
        
        public static UIModule UI => Module.UI;
        
        public static Tables Tables => Module.Config.Tables;
        
        public static GameSystem Systems => game.System();

        public static GameNote Notes => game.Note();
        
        public static GameEntity Entity => game.Entity();
        
        public static T Sys<T>() where T: SystemBase => Systems.Get<T>();
        
        public static T Note<T>() where T: NoteBase => Notes.Get<T>();
    }
}