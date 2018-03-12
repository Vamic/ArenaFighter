using System;
using System.Collections.Generic;
using System.Linq;

namespace Util
{
    class EquipmentManager
    {
        List<Equipment> equipment;

        public Statistics Stats
        {
            get
            {
                Statistics result = new Statistics();
                foreach (Equipment e in equipment)
                {
                    result += e.Stats;
                    if (e.PrefixEnchantment != null)
                        result += e.PrefixEnchantment.GetStats();
                    if (e.SuffixEnchantment != null)
                        result += e.SuffixEnchantment.GetStats();
                }
                //Check if dualwielding a weapontype
                Weapon[] weapons = GetWeapons();
                if (weapons.Length > 1 && weapons[1].IsOffhand && weapons[0].BaseName == weapons[1].BaseName)
                    result += weapons[1].DualWieldBonus;

                return result;
            }
        }

        public EquipmentManager()
        {
            equipment = new List<Equipment>();
        }
        
        public Equipment Get(EquipSlot? slot)
        {
            if (slot == null) return null;
            //Checks if any item has claimed the slot
            Equipment result = equipment.Find(i => i.Slots[0] == slot);
            if (result == null)
                result = equipment.Find(i => i.Slots[1] == slot);

            return result;
        }

        public bool Contains(EquipSlot? slot)
        {
            return Get(slot) != null;
        }

        public void UnEquip(EquipSlot? slot)
        {
            if (slot == null) return;
            //Get all equipment that isn't in the slot and make that the new equipment
            equipment = equipment.Where(item => item.Slots[0] != slot && item.Slots[1] != slot).ToList();
        }
        
        public void Equip(Equipment e)
        {
            if(Contains(e.Slots[0]))
                UnEquip(e.Slots[0]);
            if(Contains(e.Slots[1]))
                UnEquip(e.Slots[1]);
            //There shouldn't be any conflicting items left so just add it.
            equipment.Add(e);
        }

        public void Equip(Equipment[] eArray)
        {
            foreach (Equipment e in eArray)
                Equip(e);
        }

        internal Weapon[] GetWeapons()
        {
            List<Weapon> result = new List<Weapon>();
            foreach (Equipment e in equipment)
            {
                if (e is Weapon)
                    result.Add((Weapon)e);
            }
            return result.ToArray();
        }

        internal int GetDefense()
        {
            int result = 0;
            foreach (Equipment e in equipment)
            {
                if (e is Armor)
                    result += (((Armor)e).Defense);
            }
            return result;
        }

        internal Equipment[] ToArray()
        {
            return equipment.ToArray();
        }

        internal void BreakRandomGear()
        {
            int roll = 0;
            do
            {
                roll = Game.RNG.Next(100);
                EquipSlot? slot = equipment[Game.RNG.Next(equipment.Count - 1)].Slots[0];
                UnEquip(slot);
            } while (roll < 50 && equipment.Count > 0);
        }
    }
}
