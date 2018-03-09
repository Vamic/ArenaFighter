using System;

namespace Util
{
    class Enchantment
    {
        int statBonus;
        StatType statType;

        bool isPrefix;
        
        public bool IsPrefix { get => isPrefix; }
        public bool IsSuffix { get => !isPrefix; }

        public Enchantment(int statBonus, StatType statType, bool isPrefix)
        {
            this.statBonus = statBonus;
            this.isPrefix = isPrefix;
            this.statType = statType;
        }

        public Statistics GetStats()
        {
            Statistics stats;
            switch(statType)
            {
                case StatType.Strength:
                    stats = new Statistics(strength: statBonus);
                    break;
                case StatType.Dexterity:
                    stats = new Statistics(dexterity: statBonus);
                    break;
                case StatType.Constitution:
                    stats = new Statistics(constitution: statBonus);
                    break;
                case StatType.Focus:
                    stats = new Statistics(focus: statBonus);
                    break;
                default:
                    stats = new Statistics();
                    break;
            }
            return stats;
        }

        public override string ToString()
        {
            return isPrefix ? NameGenerator.GetPrefix(statBonus, statType) : NameGenerator.GetSuffix(statBonus, statType);
        }

        internal static Enchantment GetRandomEnchantment(bool isPrefix)
        {
            //Get the stat to buff
            Array values = Enum.GetValues(typeof(StatType));
            StatType stat = (StatType)values.GetValue(Game.RNG.Next(values.Length));

            //Decide the strength of the enchantment
            int value = 1;
            int roll = Game.RNG.Next(1000);
            //2.5% for +3
            if(roll < 25)
            {
                value = 3;
            } else if (roll < 100) // 10% for +2
            {
                value = 2;
            }

            return new Enchantment(value, stat, isPrefix);
        }
    }
}
