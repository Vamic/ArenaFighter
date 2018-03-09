using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    abstract class Equipment : IComparable<Equipment>
    {
        protected string baseName;

        protected Enchantment prefixEnchantment;

        protected Enchantment suffixEnchantment;

        protected EquipSlot?[] slots = new EquipSlot?[2];

        protected Statistics stats;

        protected int bias;

        public int Bias { get => bias; }
        public string BaseName { get => baseName; }
        public Enchantment PrefixEnchantment { get => prefixEnchantment; }
        public Enchantment SuffixEnchantment { get => suffixEnchantment; }
        public EquipSlot?[] Slots { get => slots; }
        public Statistics Stats { get => stats; }

        public override string ToString()
        {
            string name = "";
            if (prefixEnchantment == null)
                name += baseName;
            else
                name += "~" + prefixEnchantment + "~ " + baseName;

            if (suffixEnchantment == null)
                return name;
            else
                return name + " ~" + suffixEnchantment + "~";
        }
        
        public int CompareTo(Equipment other)
        {
            //Sort by bias(rarity), name, slot, enchantment count
            int result = other.bias - this.bias; //Rarest lowest
            if (result == 0)
                result += this.baseName.CompareTo(other.baseName);
            if (result == 0)
                result += (int)(this.slots[0] - other.slots[0]); //Equipment should never have null in index 0
            if (result == 0)
            {
                //+1 if this has a suffix enchantment
                result += Convert.ToInt32((this.suffixEnchantment != null));
                //-1 if the other has a suffix enchantment
                result -= Convert.ToInt32((other.suffixEnchantment != null));

                //+1 if this has a prefix enchantment
                result += Convert.ToInt32((this.prefixEnchantment != null));
                //-1 if the other has a prefix enchantment
                result -= Convert.ToInt32((other.prefixEnchantment != null));

                //If its still 0 at this point it doesn't matter what order they're in
            }
            
            return result;
        }
    }
}
