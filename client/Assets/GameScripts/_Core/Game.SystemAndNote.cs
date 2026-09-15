using Sanmon.Battle;

namespace GameScripts
{
    public static partial class Game
    {
        public static BattleSystem BattleSystem => _battleSystem ??= Systems.Get<BattleSystem>();
        
        private static BattleSystem _battleSystem;

        public static BattleNote BattleNote => _battleNote ??= Notes.Get<BattleNote>();
        
        private static BattleNote _battleNote;
    }
}