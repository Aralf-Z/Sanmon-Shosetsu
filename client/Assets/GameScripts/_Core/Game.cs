using System.Collections;
using System.Collections.Generic;
using Game.Config;
using Sanmon.Core;
using Sanmon.Module;
using Sanmon.Note;
using Sanmon.Syztem;

namespace GameScripts
{
    public static class Game
    {
        private class MyGame: IGetModule
        , IGetEntity
        , IGetSystem
        , IGetNote
        {
            
        }
        
        private static readonly MyGame game = new MyGame();

        public static AssetModule Asset => game.Module().Asset;
        
        public static UIModule UI => game.Module().UI;
        
        public static Tables Tables => game.Module().Config.Tables;
        
        public static GameSystem Systems => game.System();

        public static GameNote Notes => game.Note();
        
        public static T Sys<T>() where T: SystemBase => Systems.Get<T>();
        
        public static T Note<T>() where T: NoteBase => Notes.Get<T>();
        
    }
}