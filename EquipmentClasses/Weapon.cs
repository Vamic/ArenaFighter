using System;

namespace Util
{
    class Weapon : Equipment
    {
        protected Dice baseDamage;
        protected int damageBonus;
        protected int offhandPenalty;
        protected StatType statModifier;
        protected bool canBeOffhand;

        public bool IsOffhand {
            get => slots[0] == EquipSlot.Offhand;
            set => slots[0] = value && CanBeOffhand ? EquipSlot.Offhand : EquipSlot.Mainhand;
        }
        public bool CanBeOffhand { get => canBeOffhand; }

        internal static Weapon GenerateRandomWeapon(Character character, bool offhand = false)
        {            
            Weapon weapon;
            do
            {
                //Get a random index
                int i = Game.RNG.Next(Game.weaponClasses.Length);

                //Make the weapon
                weapon = (Weapon)Activator.CreateInstance(Game.weaponClasses[i]);
                //If we want offhand, make it offhand if it can be, otherwise try again
                if (weapon.CanBeOffhand && offhand)
                    weapon.IsOffhand = true;
            } while (weapon.IsOffhand != offhand);

            return weapon;
        }

        protected virtual int GetDamageModifier(Character character)
        {

            //Calculate modifier from stats
            int statMod = character.Stats.GetStat(statModifier);
            statMod += character.Gear.Stats.GetStat(statModifier);
            statMod /= 2;
            //Calculate demerits
            int damagePenalty = IsOffhand ? offhandPenalty : 0;

            //Actual damage modifier
            return statMod + damageBonus - damagePenalty;
        }

        public int GetMinDamage(Character character)
        {
            int minDamage = baseDamage.Min() + GetDamageModifier(character);
            if (minDamage < 0) minDamage = 0;
            return minDamage;
        }

        public int GetMaxDamage(Character character)
        {
            int maxDamage = baseDamage.Max() + GetDamageModifier(character);
            return maxDamage;
        }

        public int GetDamage(Character character)
        {
            //Roll damage and apply the modifier
            int damage = baseDamage.Roll() + GetDamageModifier(character);

            //Can't do negative damage
            if (damage < 0)
                damage = 0;

            return damage;
        }

        public int GetAccuracy(Character character)
        {
            return character.Stats.GetStat(statModifier) / 2;
        }
    }

    class Longsword : Weapon
    {
        public Longsword()
        {
            baseName = "Longsword";
            stats = new Statistics(dexterity: -2);
            statModifier = StatType.Strength;
            baseDamage = new Dice(DiceSize.Eight);
            slots[0] = EquipSlot.Mainhand;
            slots[1] = EquipSlot.Offhand;
        }

        //Longswords get double the strength bonus as it takes up both hands and only gets one shot at hitting per round
        protected override int GetDamageModifier(Character character)
        {
            //Calculate modifier from stats
            int statMod = character.Stats.GetStat(statModifier);
            statMod += character.Gear.Stats.GetStat(statModifier);
            //Calculate demerits
            int damagePenalty = IsOffhand ? offhandPenalty : 0;

            //Actual damage modifier
            return statMod + damageBonus - damagePenalty;
        }
    }

    class Maul : Weapon
    {
        public Maul()
        {
            baseName = "Maul";
            stats = new Statistics(dexterity: -2, focus: -2);
            statModifier = StatType.Strength;
            baseDamage = new Dice(DiceSize.Ten);
            slots[0] = EquipSlot.Mainhand;
            slots[1] = EquipSlot.Offhand;
        }

        //Mauls get double the strength bonus as it takes up both hands and only gets one shot at hitting per round
        protected override int GetDamageModifier(Character character)
        {
            //Calculate modifier from stats
            int statMod = character.Stats.GetStat(statModifier);
            statMod += character.Gear.Stats.GetStat(statModifier);
            //Calculate demerits
            int damagePenalty = IsOffhand ? offhandPenalty : 0;

            //Actual damage modifier
            return statMod + damageBonus - damagePenalty;
        }
    }

    class Shortsword : Weapon
    {
        public Shortsword()
        {
            baseName = "Shortsword";
            stats = new Statistics();
            statModifier = StatType.Strength;
            baseDamage = new Dice();
            slots[0] = EquipSlot.Mainhand;
        }
    }

    class Dagger : Weapon
    {
        public Dagger()
        {
            canBeOffhand = true;
            offhandPenalty = 1;
            baseName = "Dagger";
            stats = new Statistics();
            statModifier = StatType.Dexterity;
            baseDamage = new Dice(DiceSize.Four);
            slots[0] = EquipSlot.Mainhand;
        }
        public Dagger(bool offhand) : this()
        {
            if (offhand)
                slots[0] = EquipSlot.Offhand;
        }
    }

    class Rapier : Weapon
    {
        public Rapier()
        {
            baseName = "Rapier";
            stats = new Statistics(focus:1);
            statModifier = StatType.Dexterity;
            baseDamage = new Dice(DiceSize.Four);
            damageBonus = 1;
            slots[0] = EquipSlot.Mainhand;
        }
    }

    class Flail : Weapon
    {
        public Flail()
        {
            baseName = "Flail";
            stats = new Statistics(focus:-1);
            statModifier = StatType.Dexterity;
            baseDamage = new Dice();
            slots[0] = EquipSlot.Mainhand;
        }
    }
}
