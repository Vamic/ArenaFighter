using Lexicon.CSharp.InfoGenerator;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    static class NameGenerator
    {
        static private string[,] strengthAffixes;
        static private string[,] dexterityAffixes;
        static private string[,] constitutionAffixes;
        static private string[,] focusAffixes;
        
        private static InfoGenerator infoGenerator;

        static public void Initialize()
        {
            infoGenerator = new InfoGenerator(Game.RNG.Next(30000));

            strengthAffixes = new string[,] {
                { "Fighter's", "Soldier's", "Champion's" },
                { "of the Ox", "of the Bear", "of the Giant" }
            };
            dexterityAffixes = new string[,] {
                { "Acrobat's", "Assassin's", "Ninja's" },
                { "of the Wolf", "of the Viper", "of the Cheetah" }
            };
            constitutionAffixes = new string[,] {
                { "Defender's", "Guardian's", "Sentinel's" },
                { "of the Tortoise", "of the Hippo", "of the Troll" }
            };
            focusAffixes = new string[,] {
                { "Calming", "Clear", "Serene" },
                { "of the Owl", "of the Hawk", "of the Eagle" }
            };
        }

        static private string GetAffix(int indexType, int indexName, StatType type)
        {
            switch(type)
            {
                case StatType.Strength:
                    return strengthAffixes[indexType, indexName];
                case StatType.Dexterity:
                    return dexterityAffixes[indexType, indexName];
                case StatType.Constitution:
                    return constitutionAffixes[indexType, indexName];
                case StatType.Focus:
                    return focusAffixes[indexType, indexName];
                default:
                    return indexType == 0 ? "Enigmatic" : "of Mystery";
            }
        }

        static public string GetPrefix(int value, StatType type)
        {
            int power = 0;
            if (value > 3)
            {
                power = value - 3;
                value = 3;
            }
            string result = GetAffix(0, value - 1, type);
            if (power > 0) result += " +" + power;
            return result;
        }

        static public string GetSuffix(int value, StatType type)
        {
            int power = 0;
            if (value > 3)
            {
                power = value - 3;
                value = 3;
            }
            string result = GetAffix(1, value - 1, type);
            if (power > 0) result += " +" + power;
            return result;
        }

        static public string GetCharacterName()
        {
            string name = infoGenerator.NextFirstName();
            name = char.ToUpper(name[0]) + name.Substring(1);
            return name;
        }

        static public string GetCharacterTitle(Statistics stats)
        {
            return "the Fighter";
        }
    }
}
