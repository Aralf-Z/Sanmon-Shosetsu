using System;

namespace Sanmon.Battle
{
    [Flags]
    public enum Group
    {
        Neutral = 0b1,
        Player = 0b10,
        Enemy = 0b100,
    }
}