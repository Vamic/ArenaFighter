using System;
using System.Collections.Generic;

namespace Util
{
    class Armor : Equipment
    {
        public int Defense { get; protected set; }

        protected Armor (EquipSlot type, int enchantments, int level, int bias)
        {
            slots[0] = type;
            base.bias = bias;
            if (enchantments != 0)
            {
                int roll = Game.RNG.Next(1000);
                //20% to get one enchantment
                int baseChance1 = 200;
                //+ 10% per level above 1
                int extraChance1 = baseChance1 + ((((level - 1) * baseChance1) / 2));
                //2.5% to get another enchantment
                int baseChance2 = 25;
                //+ ~1.25% per level above 1
                int extraChance2 = baseChance2 + (((level - 1) * baseChance2) / 2);

                //Check if it rolled good enough
                if (roll < baseChance1 + extraChance1 || enchantments > 0)
                {
                    suffixEnchantment = Enchantment.GetRandomEnchantment(false);
                    if (roll < baseChance2 + extraChance2 || enchantments > 1)
                    {
                        prefixEnchantment = Enchantment.GetRandomEnchantment(true);
                    }
                }
            }
        }

        internal static Armor GenerateRandomArmorpiece(int level, EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Head:
                case EquipSlot.Body:
                case EquipSlot.Hands:
                case EquipSlot.Feet:
                    //What gets passed to the constructor
                    object[] options = new object[] { slot, -1 , level};
                    Armor piece;
                    int roll;
                    do
                    {
                        //Get a random index
                        int i = Game.RNG.Next(Game.armorClasses.Length);
                        //Make armorpiece
                        piece = (Armor)Activator.CreateInstance(Game.armorClasses[i], options);
                        //Check if we should keep it
                        roll = Game.RNG.Next(100) - (level - 1) * 5;
                    } while (roll > piece.Bias);

                    return piece;
                default:
                    throw new Exception("Invalid EquipSlot passed.");
            }
        }

        internal static Armor GenerateRandomArmorpiece(int level)
        {
            EquipSlot[] slotChoices = {
                EquipSlot.Head,
                EquipSlot.Body,
                EquipSlot.Hands,
                EquipSlot.Feet
                };
            EquipSlot slot = slotChoices[Game.RNG.Next(slotChoices.Length)];
            return GenerateRandomArmorpiece(level, slot);
        }

        internal static Armor[] GenerateStartingGear(Character character)
        {
            int Level = character.Level;
            EquipSlot[] slots;
            switch (Level)
            {
                case 1:
                case 2:
                    slots = new EquipSlot[] { EquipSlot.Body };
                    break;
                case 3:
                case 4:
                    slots = new EquipSlot[] { EquipSlot.Head, EquipSlot.Body };
                    break;
                case 5:
                    slots = new EquipSlot[] { EquipSlot.Head, EquipSlot.Body, EquipSlot.Feet };
                    break;
                default:
                    slots = new EquipSlot[] { EquipSlot.Head, EquipSlot.Body, EquipSlot.Hands, EquipSlot.Feet };
                    break;
            }

            List<Armor> pieces = new List<Armor>();

            foreach (EquipSlot e in slots)
                pieces.Add(GenerateRandomArmorpiece(Level, e));

            return pieces.ToArray();
        }
    }

    class Leather : Armor
    {
        public Leather(EquipSlot type, int enchantments = -1, int level = 1) : base(type, enchantments, level, bias: 100)
        {
            stats = new Statistics(); //No stat penalties for light armor
            baseName = "Leather";
            switch (type)
            {
                case EquipSlot.Body:
                    baseName += " Armor";
                    Defense = 2;
                    break;
                case EquipSlot.Head:
                    baseName += " Cap";
                    Defense = 1;
                    break;
                case EquipSlot.Hands:
                    baseName += " Gloves";
                    Defense = 0;
                    break;
                case EquipSlot.Feet:
                    baseName += " Boots";
                    Defense = 0;
                    break;
            }
        }
    }

    class Chainmail : Armor
    {
        public Chainmail(EquipSlot type, int enchantments = -1, int level = 1) : base(type, enchantments, level, bias: 50)
        {
            stats = new Statistics(); //No stat penalties for most of them
            baseName = "Chainmail";
            switch (type)
            {
                case EquipSlot.Body:
                    baseName += " Armor";
                    Defense = 4;
                    stats = new Statistics(dexterity: -1); //Medium body armor penalty
                    break;
                case EquipSlot.Head:
                    baseName += " Helmet";
                    Defense = 2;
                    break;
                case EquipSlot.Hands:
                    baseName += " Gloves";
                    Defense = 1;
                    break;
                case EquipSlot.Feet:
                    baseName += " Boots";
                    Defense = 1;
                    break;
            }
        }
    }

    class Fullplate : Armor
    {
        public Fullplate(EquipSlot type, int enchantments = -1, int level = 1) : base(type, enchantments, level, bias: 20)
        {
            baseName = "Plate";
            switch (type)
            {
                case EquipSlot.Body:
                    baseName += " Armor";
                    Defense = 6;
                    stats = new Statistics(dexterity: -2); //Heavy body armor penalty
                    break;
                case EquipSlot.Head:
                    baseName += " Helmet";
                    Defense = 4;
                    stats = new Statistics(focus: -1); //Heavy helmet armor penalty
                    break;
                case EquipSlot.Hands:
                    baseName += " Gauntlets";
                    Defense = 2;
                    stats = new Statistics(dexterity: -1); //Heavy gauntlets armor penalty
                    break;
                case EquipSlot.Feet:
                    baseName += " Greaves";
                    Defense = 2;
                    stats = new Statistics(dexterity: -1); //Heavy boots armor penalty
                    break;
            }
        }
    }
}
