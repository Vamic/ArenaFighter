using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    class Statistics
    {
        private int strength;
        private int dexterity;
        private int constitution;
        private int focus;

        public int Strength { get => strength; set => strength = value; }
        public int Dexterity { get => dexterity; set => dexterity = value; }
        public int Constitution { get => constitution; set => constitution = value; }
        public int Focus { get => focus; set => focus = value; }

        public Statistics(int strength = 0, int dexterity = 0, int constitution = 0, int focus = 0)
        {
            this.strength = strength;
            this.dexterity = dexterity;
            this.constitution = constitution;
            this.focus = focus;
        }

        public Statistics(StatType type, int value)
        {
            switch (type)
            {
                case StatType.Strength:
                    strength = value;
                    break;
                case StatType.Dexterity:
                    dexterity = value;
                    break;
                case StatType.Constitution:
                    constitution = value;
                    break;
                case StatType.Focus:
                    focus = value;
                    break;
                default:
                    throw new Exception("Invalid stat type.");
            }
        }

        public int GetStat(StatType type)
        {
            switch(type)
            {
                case StatType.Strength:
                    return Strength;
                case StatType.Dexterity:
                    return Dexterity;
                case StatType.Constitution:
                    return Constitution;
                case StatType.Focus:
                    return Focus;
                default:
                    throw new Exception("Invalid stat type.");
            }
        }

        public static Statistics operator +(Statistics s1, Statistics s2)
        {
            int str = s1.Strength + s2.Strength;
            int dex = s1.Dexterity + s2.Dexterity;
            int con = s1.Constitution + s2.Constitution;
            int foc = s1.Focus + s2.Focus;
            return new Statistics(str, dex, con, foc);
        }

        public static Statistics Combine(Statistics[] inputStats)
        {
            Statistics result = new Statistics();
            for (int i = 0; i < inputStats.Length; i++)
            {
                result += inputStats[i];
            }
            return result;
        }

        internal static Statistics GenerateStatistics(Character character)
        {
            Dice d = new Dice(DiceSize.Six);
            int LevelBonus = (character.Level / 2);
            //3 to 8
            int str = 2 + d.Roll();
            int dex = 2 + d.Roll();
            int con = 2 + d.Roll();
            int foc = 2 + d.Roll();
            //Distribute Level bonus
            while(LevelBonus > 0)
            {
                d = new Dice(DiceSize.Four);
                switch(d.Roll())
                {
                    case 1:
                        str++;
                        break;
                    case 2:
                        dex++;
                        break;
                    case 3:
                        con++;
                        break;
                    case 4:
                        foc++;
                        break;
                }
                LevelBonus--;
            }
            return new Statistics(str, dex, con, foc);
        }
    }
}
