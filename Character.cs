using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    class Character
    {
        private int rolledHealth;
        private string name;
        private int fatigue;
        private int currentHealth;
        
        public int CurrentHealth
        {
            //Check if health is above max before doing anything to it
            get
            {
                if (currentHealth > MaxHealth) currentHealth = MaxHealth;
                return currentHealth;
            }
            private set
            {
                if (currentHealth > MaxHealth) currentHealth = MaxHealth;
                currentHealth = value;
            }
        }
        public int Level { get; private set; }
        public int Kills { get; private set; }
        public int Exp { get; private set; }
        public int StatPoints { get; private set; }
        
        public bool IsAlive { get => CurrentHealth > 0; }
        public bool IsRetired { get; private set; }
        public bool IsPlayer { get; set; }

        public EquipmentManager Gear { get; private set; }
        public Statistics Stats { get; private set; }
        
        //Variables that are just calculations
        public int MaxHealth { get => rolledHealth + Level * ((Stats.Constitution+Gear.Stats.Constitution) / 3); }
        public int Score { get => Level * 10 + Kills * 2 + (IsAlive ? 2 * Level : 0); }
        public int NextLevelExp {
            get => (int)(Math.Pow((Level + 1), 2) * 100);
        }
        private int StrengthDmgBonus { get => (int)Math.Floor(((double)(Stats.Strength + Gear.Stats.Strength) / 3)) - 2; }

        public Character(int level = 1, string name = "Unassigned", bool isPlayer = false)
        {
            this.name = name;
            this.Level = level;
            IsPlayer = isPlayer;
            fatigue = 0;

            //Gear up
            Gear = new EquipmentManager();
            Weapon mainWeapon = Weapon.GenerateRandomWeapon(this);
            Weapon offhandWeapon = null;
            //Roll for dual wielding
            if(mainWeapon.Slots[1] == null)
            {
                int roll = Game.RNG.Next(100);
                if(roll < 50)
                {
                    offhandWeapon = Weapon.GenerateRandomWeapon(this, true);
                }
            }
            Armor[] armorPieces = Armor.GenerateStartingGear(this);

            Gear.Equip(mainWeapon);
            if(offhandWeapon != null)
                Gear.Equip(offhandWeapon);
            Gear.Equip(armorPieces);

            Stats = Statistics.GenerateStatistics(this);

            if(level > 0) InitialCalculations();
        }
        
        public void InitialCalculations()
        {
            //Health
            //1d4 + con/2 HP per Level
            rolledHealth = new Dice(DiceSize.Four, Level + 2).Roll();
            int con = Stats.Constitution + Gear.Stats.Constitution;

            CurrentHealth = MaxHealth;

            //Exp
            Exp = new Character(Level - 1).NextLevelExp;
        }

        public void RecalculateVariables()
        {
            //Health
            int con = Stats.Constitution + Gear.Stats.Constitution;
            int previousMax = MaxHealth; 

            //Have currenthealth follow the stat change
            CurrentHealth += MaxHealth - previousMax;
            //Make sure you don't die from it when it changes
            if (MaxHealth != previousMax && CurrentHealth < 1) CurrentHealth = 1;
        }

        internal void AddStat(StatType stat)
        {
            StatPoints--;
            Stats += new Statistics(stat, 1);
            Display.UpdateSidebar(playerStats: true);
        }

        public void Rest()
        {
            while(Exp > NextLevelExp)
            {
                LevelUp();
            }

            fatigue = 0;
            CurrentHealth = MaxHealth;
            Display.UpdateSidebar(playerHP: true);
        }

        public void Retire()
        {
            IsRetired = true;
        }

        private void LevelUp()
        {
            Level++;
            Display.WriteLine("Level up! Press any key to continue.");
            Console.ReadKey(true);

            //Add stat points every other Level
            if (Level % 2 == 0) StatPoints++;
            //Levelup menu
            StatType statChoice = 0;
            //Keep going while we have stat point and choice isnt out of bounds (Exit choice is out of bounds)
            while (StatPoints > 0 && Enum.IsDefined(typeof(StatType), statChoice))
            {
                statChoice = Menu.Levelup();
                if (Enum.IsDefined(typeof(StatType), statChoice))
                    AddStat(statChoice);
            }

            //Roll some more health
            rolledHealth += new Dice(DiceSize.Four).Roll();

            //Update HP and EXP
            RecalculateVariables();
            //Display the updates
            Display.UpdateSidebar(playerHP: true, playerLevel: true);
        }
        
        public void GainKill(Character defeated)
        {
            Kills++;

            //Get 50% of what the opponent needs to Level up
            int newExp = (defeated.NextLevelExp - defeated.Exp)/2;
            //Check Level difference
            int LevelDifference = defeated.Level - Level;
            //If the opponent was 4 Levels or more below, get almost nothing
            if (LevelDifference < -3)
                newExp /= 5;
            //If opponent was within 2 Levels or more below, get a quarter
            else if (LevelDifference < -1)
                newExp /= 2;
            //If opponent is 2 Levels or more above, get a quarter more
            //Higher Leveled enemies give more experience regardless
            else if (LevelDifference > 1)
                newExp += newExp/4;

            Exp += newExp;
        }

        internal List<Equipment> DropGear()
        {
            Gear.BreakRandomGear();
            return new List<Equipment>(Gear.ToArray());
        }

        public bool DodgeAttack(int toHit)
        {
            int fatigueGain = 0;
            //Check if dodged, can't dodge if too tired
            int dodgeToBeat = Stats.Dexterity + Gear.Stats.Dexterity + 4;
            if (toHit > dodgeToBeat || fatigue > 100)
            {
                fatigueGain = 10 - Stats.Constitution;
                if (fatigueGain < 0) fatigueGain = 0;
                fatigue += fatigueGain;
                return false;
            }
            else
            {
                fatigueGain = 20 - Stats.Constitution;
                if (fatigueGain < 10) fatigueGain = 10;
                fatigue += fatigueGain;
                return true;
            }
        }

        public int RecieveDamage(int damage)
        {
            //Block damage with armor
            int trueDamage = damage - (int)Math.Ceiling((Gear.GetDefense() * 0.9)); //Defense effectiveness is 90%

            //Minimum damage dealt is 1
            if (trueDamage < 1)
                trueDamage = 1;

            //Take damage
            CurrentHealth -= trueDamage;
            return trueDamage;
        }

        public string Attack(Character opponent)
        {
            //Variables for keeping track of everything
            int damage = 0;
            int attacks = 0;
            int crits = 0;

            //Attack with each weapon
            Weapon[] weapons = Gear.GetWeapons();
            foreach(Weapon w in weapons)
            {
                //Calculate ToHit, (1-10) + half of focus + half of weapon's stattype
                //Example: ToHit with dex weapon with 6 focus and 8 dex: 8 to 17
                int toHit = Game.RNG.Next(10) + (Stats.Focus / 2) + w.GetAccuracy(this);

                //Check if that hits (Has to beat opponents dex + 4
                bool dodged = opponent.DodgeAttack(toHit);
                if(!dodged)
                {
                    //Roll damage of weapon
                    int newDamage = w.GetDamage(this) + StrengthDmgBonus;

                    //Check if crit, focus is critchance
                    int critRoll = Game.RNG.Next(100);
                    if(critRoll < Stats.Focus)
                    {
                        //damage * 1.5 on crit
                        newDamage += newDamage/2;
                        crits++;
                    }
                    attacks++;
                    damage += newDamage;
                }
            }

            //Calculate real damage, if damage is over 0 the minimum damage dealt is 1.
            int trueDamage = 0;
            if (damage > 0)
                trueDamage = opponent.RecieveDamage(damage);
           
            string result = "";
            
            //Build response
            if (attacks == 0)
                return "missed";
            else
            {
                result = "hit";
                if (attacks > 1)
                    result += " " + attacks + " times";

                if (trueDamage > 0)
                    result += " for " + trueDamage + " damage";
                else
                    result += " but dealt no damage";

                if (crits > 0)
                    result += " ^*CRITICAL*^";

                return result;
            }
        }

        public string GetDamageRange()
        {
            Weapon[] weapons = Gear.GetWeapons();
            //Calculate min
            int min = 0;
            for (int i = 0; i < weapons.Length; i++)
            {
                if (i == 0)
                    min = weapons[i].GetMinDamage(this);
                else
                    min = weapons[i].GetMinDamage(this) < min ? weapons[i].GetMinDamage(this) : min;
            }
            min += StrengthDmgBonus;
            //Calculate max
            int max = StrengthDmgBonus;
            foreach (Weapon w in weapons)
            {
                max += w.GetMaxDamage(this);
            }
            //Return range
            return min + " - " + max;
        }

        public override string ToString()
        {
            string color = "^";
            if (IsPlayer)
                color = "~";
            return color + name + color;
        }

        public void Equip(Equipment item)
        {
            Gear.Equip(item);
            RecalculateVariables();
            Display.UpdateSidebar(playerGear: true, playerStats: true, playerHP: true);
        }

        public void PrintSummary ()
        {
            string s = name;
            Statistics total = Stats + Gear.Stats;
            s += "\n-----Equipment-----";
            s += "\nHead: " + Gear.Get(EquipSlot.Head);
            s += "\nBody: " + Gear.Get(EquipSlot.Body);
            s += "\nHands: " + Gear.Get(EquipSlot.Hands);
            s += "\nFeet: " + Gear.Get(EquipSlot.Feet);
            s += "\n";
            s += "\nAmulet: " + Gear.Get(EquipSlot.Amulet);
            s += "\nRing: " + Gear.Get(EquipSlot.LeftRing);
            s += "\nRing: " + Gear.Get(EquipSlot.RightRing);
            s += "\n";
            s += "\n------Weapons------ ";
            s += "\nMainhand: " + Gear.Get(EquipSlot.Mainhand);
            s += "\nOffhand: " + Gear.Get(EquipSlot.Offhand);
            s += "\n";
            s += "\n-------Stats------- ";
            s += "\nStr: " + total.Strength + " (" + Stats.Strength + " + " + Gear.Stats.Strength + ")";
            s += "\nDex: " + total.Dexterity + " (" + Stats.Dexterity + " + " + Gear.Stats.Dexterity + ")";
            s += "\nCon: " + total.Constitution + " (" + Stats.Constitution + " + " + Gear.Stats.Constitution + ")";
            s += "\nFoc: " + total.Focus + " (" + Stats.Focus + " + " + Gear.Stats.Focus + ")";
            s += "\n";
            s += "\nDefense: " + Gear.GetDefense();
            Display.WriteLine(s);
        }
    }
}
