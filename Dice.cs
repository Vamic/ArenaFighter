using System;

namespace Util
{
    enum DiceSize
    {
        Four = 4,
        Six = 6,
        Eight = 8,
        Ten = 10
    }

    internal class Dice
    {
        DiceSize size;

        int amount;

        public Dice(DiceSize size = DiceSize.Six, int amount = 1)
        {
            this.size = size;
            this.amount = amount;
        }

        public int Roll()
        {
            int result = 0;
            for(int i = 0; i < amount; i++)
                result += Game.RNG.Next(1, (int)size + 1);
            return result;
        }

        public override string ToString()
        {
            return amount + "d" + (int)size;
        }

        public int Min()
        {
            return amount;
        }

        public int Max()
        {
            return amount * (int)size;
        }
    }
}