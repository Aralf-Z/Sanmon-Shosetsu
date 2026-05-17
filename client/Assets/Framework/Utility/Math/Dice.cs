using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanmon.Utility.Math
{
    public struct Dices
    {
        public readonly int face;
        public readonly int amount;
        
        public Dices(int face, int amount = 1)
        {
            this.face = face;
            this.amount = amount;
        }
    }
    
    public class Dice
    {
        private readonly Random mRandom;

        public Dice()
        {
            mRandom = new Random();
        }

        public Dice(int seed)
        {
            mRandom = new Random(seed);
        }

        public IEnumerable<int> Roll(Dices dices)
        {
            for(var i = 0; i < dices.amount; i++)
                yield return mRandom.Next(1, dices.face + 1);
        }

        public int RollSum(Dices dices)
        {
            return Roll(dices).Sum();
        }
    }
}