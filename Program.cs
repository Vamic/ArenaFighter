using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Lexicon.CSharp.InfoGenerator;

namespace Util
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.RNG = new Random();

            NameGenerator.Initialize();

            Menu.InitiateMenues();
            
            Display.Initialize();

            //Get all the items in the game for randomly picking through them later

            var equipmentClasses = Assembly
               .GetAssembly(typeof(Equipment)) //Get all Equipment classes
               .GetTypes();

            Game.weaponClasses = equipmentClasses
                .Where(t => t.IsSubclassOf(typeof(Weapon))) //Get classes inheriting Weapon
               .ToArray();

            Game.armorClasses = equipmentClasses
                .Where(t => t.IsSubclassOf(typeof(Armor))) //Get classes inheriting Armor
               .ToArray();

            Game.Start();
        }
    }

    enum EquipSlot
    {
        Mainhand,
        Offhand,
        Head,
        Body,
        Feet,
        Hands,
        Amulet,
        LeftRing,
        RightRing
    }

    enum StatType
    {
        Strength,
        Dexterity,
        Constitution,
        Focus
    }

    static class Game
    {
        public static Random RNG;

        public static Type[] weaponClasses;

        public static Type[] armorClasses;

        public static Character Player { get; private set; }

        public static Character Opponent { get; private set; }

        public static List<Match> Matches { get; private set; }

        private static bool exiting;

        static internal void Start()
        {
            Reset();

            GameLoop();
        }

        //New character
        static internal void Reset()
        {
            Display.Clear();

            exiting = false;

            //Name & reroll player character
            CharacterCreation();
            
            Matches = new List<Match>();
        }

        static private void GameLoop()
        {
            while (!exiting)
            {
                while (Player.IsAlive && !exiting)
                {
                    if (Player.IsRetired)
                        Retired();
                    else
                    {
                        Menu.MainGameSelection mChoice = Menu.MainGameMenu();
                        Display.Clear();
                        switch (mChoice)
                        {
                            case Menu.MainGameSelection.Search:
                                Search();
                                break;
                            case Menu.MainGameSelection.Battlelog:
                                Battlelog();
                                break;
                            case Menu.MainGameSelection.Retire:
                                Player.Retire();
                                break;
                            case Menu.MainGameSelection.FightTilDeath:
                                FightTilDeath();
                                break;
                            case Menu.MainGameSelection.Exit:
                                exiting = true;
                                break;
                        }
                    }
                }
                if(!exiting)
                {
                    Menu.GameOverSelection gChoice = Menu.GameOver();

                    switch (gChoice)
                    {
                        case Menu.GameOverSelection.Battlelog:
                            Battlelog();
                            break;
                        case Menu.GameOverSelection.ContinueWithOpponent:
                            Display.Clear(typeof(Display.SideBorders));
                            SwitchCharacter();
                            break;
                        case Menu.GameOverSelection.NewFighter:
                            //Reset sidebar & go to character creation
                            Display.Clear(typeof(Display.SideBorders));
                            Reset();
                            break;
                        case Menu.GameOverSelection.Exit:
                            exiting = true;
                            break;
                    }
                }
            }
        }

        private static void SwitchCharacter()
        {
            Display.Clear();

            //Set the current character to opponent
            Player.IsPlayer = false;
            
            //Make the opponent the new player
            Character temp = Player;
            Player = Opponent;
            Opponent = temp;
            Player.IsPlayer = true;
            
            //Reset the battlelog
            Match lastMatch = Matches.Last();
            Matches = new List<Match>() { lastMatch };

            //Restore the sidebar
            Display.UpdateSidebar(true, true, true, true, true);

            //Loot the old gear
            LootPickup(Opponent.DropGear());

            //Restore hp and possibly Level up
            Player.Rest();
        }

        private static void Search(bool fightTilDeath = false)
        {
            int roll = RNG.Next(100);
            if(roll < 10 && !fightTilDeath)
            {
                List<Equipment> loot = new List<Equipment>();
                //Happen upon some loot
                if (roll < 3) //30%
                {
                    //A weapon
                    loot.Add(Weapon.GenerateRandomWeapon(Player));
                } else //70%
                {
                    //A piece of equipment
                    loot.Add(Armor.GenerateRandomArmorpiece(Player.Level + 2));
                }

                LootPickup(loot);
            } else
            {
                FightEncounter(fightTilDeath);
            }
        }

        private static void LootPickup(List<Equipment> loot)
        {
            if (loot.Count == 0) return;

            //In case a weapon was set to offhand, reset it
            foreach(Equipment item in loot)
            {
                if(item is Weapon && ((Weapon)item).CanBeOffhand && ((Weapon)item).IsOffhand)
                {
                    ((Weapon)item).IsOffhand = false;
                }
            }

            Equipment lootChoice;
            do
            {
                lootChoice = Menu.LootSelection(loot.ToArray());
                if (lootChoice != null)
                {
                    Equipment playerEquipment1 = Player.Gear.Get(lootChoice.Slots[0]);
                    Equipment playerEquipment2 = Player.Gear.Get(lootChoice.Slots[1]);
                    string message = "";

                    //Two handed weapon and we have something in the offhand
                    if (lootChoice.Slots[1] != null && playerEquipment2 != null)
                        message = "Do you want to replace your " + playerEquipment1 + " and " + playerEquipment2 + " with " + lootChoice + "?";
                    //We have something in the slot
                    else if (playerEquipment1 != null)
                        message = "Do you want to replace your " + playerEquipment1 + " with " + lootChoice + "?";
                    //The slot is empty
                    else
                        message = "Do you want to equip the " + lootChoice + "?";

                    bool confirmChoice = Menu.Confirm(message);
                    //Equip armor/mainhand
                    if (confirmChoice)
                    {
                        if (lootChoice is Weapon)
                            ((Weapon)lootChoice).IsOffhand = false; //Make sure its in the mainhand

                        //Equip
                        Player.Equip(lootChoice);
                        //Remove item from gound
                        loot.Remove(lootChoice);

                        //Place items on ground if there were any
                        if (playerEquipment1 != null)
                            loot.Add(playerEquipment1);
                        if (playerEquipment2 != null)
                            loot.Add(playerEquipment2);
                        //User declined, ask to equip weapon in offhand
                    }
                    else if (lootChoice is Weapon && ((Weapon)lootChoice).CanBeOffhand)
                    {
                        if (playerEquipment2 == null)
                            playerEquipment2 = Player.Gear.Get(EquipSlot.Offhand);

                        Equipment toReplace = playerEquipment2;
                        //Got something in offhand
                        if (playerEquipment2 != null)
                        {
                            toReplace = playerEquipment2;
                            message = "Do you want to replace your " + playerEquipment2 + " with " + lootChoice + "?";
                            //Two handed weapon
                        }
                        else if (playerEquipment1.Slots[1] == EquipSlot.Offhand)
                        {
                            toReplace = playerEquipment1;
                            message = "Do you want to unequip your " + playerEquipment2 + " and put the " + lootChoice + " in your offhand?";
                        }
                        //Offhand is empty
                        else
                        {
                            toReplace = null;
                            message = "Do you want to equip " + lootChoice + " in your offhand?";
                        }

                        bool offhandChoice = Menu.Confirm(message);
                        //Equip in offhand
                        if (offhandChoice)
                        {
                            //Set item to offhand
                            ((Weapon)lootChoice).IsOffhand = true;
                            //Equip
                            Player.Equip(lootChoice);
                            //Remove from ground
                            loot.Remove(lootChoice);
                            //Add replaced weapon to ground
                            if (toReplace != null)
                                loot.Add(toReplace);
                        }
                    }
                }
            } while (lootChoice != null && loot.Count > 0);
        }

        private static void FightEncounter(bool fightTilDeath)
        {
            int speed = fightTilDeath ? 0 : 2;
            GenerateOpponent();
            NewMatch(speed);

            if (Player.IsAlive)
            {
                if (!fightTilDeath)
                {
                    //Write victory message
                    Console.ForegroundColor = ConsoleColor.Green;
                    Display.WriteLine("Your fighter " + Player + " remains. Press any key to continue.");
                    Console.ResetColor();
                    Console.ReadKey();
                    
                    LootPickup(new List<Equipment>(Opponent.DropGear()));
                }
                //Get some exp
                Player.GainKill(Opponent);
                //Restore health & Levelup
                Player.Rest();
            }
            else
            {
                Opponent.GainKill(Player); //Give opponent some exp in case player decides to use them

                Console.ForegroundColor = ConsoleColor.Red;
                Display.WriteLine("Your fighter " + Player + " has died. Press any key to continue.");
                Console.ReadKey();
            }
        }

        static private void FightTilDeath()
        {
            while (Player.IsAlive)
            {
                Display.WriteLine("--- A New Challenger Approaches ---", 0);
                Search(true);
                Thread.Sleep(100);
                Display.Clear();
            }
        }

        private static void GenerateOpponent()
        {
            //Create opponent
            string name = NameGenerator.GetCharacterName();
            Opponent = new Character(name: name, level: Player.Level);
            //Update the sidebar
            Display.UpdateSidebar(opponentHP: true, opponentLevel: true, opponentName: true, opponentStats: true);
        }

        private static void Retired()
        {
            Menu.RetiredSelection rChoice = Menu.Retired();

            switch (rChoice)
            {
                case Menu.RetiredSelection.Battlelog:
                    Battlelog();
                    break;
                case Menu.RetiredSelection.NewFighter:
                    //Reset sidebar & go to character creation
                    Display.Clear(typeof(Display.SideBorders));
                    Reset();
                    break;
                case Menu.RetiredSelection.Exit:
                    exiting = true;
                    break;
            }
        }

        static private void Battlelog()
        {
            Display.Clear();
            foreach (Match m in Matches)
            {
                Display.WriteLine(m.ToString(), 8);
            }
            Console.ReadKey();

            /**
            Display.WriteLine("Enter the number of the match you'd like to replay.\n Entering nothing sends you back to the menu.");
            Display.Write("Match-");
            int selection;
            do
            {
                selection = Math.Abs(ConsoleEx.ReadInt(true));
                if (selection != 0 && Matches.Count() > selection)
                {
                    DisplayMatch(Matches[selection]);
                }
            } while (selection != 0);
            /**/
        }

        private static void NewMatch(int speed = 2)
        {
            //New match
            Match match = new Match(Player, Opponent);
            //Fight to the death
            while (Player.IsAlive && Opponent.IsAlive)
            {
                //Makes a new round and executes it
                Round round = match.NextRound();

                //Print the summary
                Display.WriteLine(round.ToString(), speed);

                //Update health
                Display.UpdateSidebar(playerHP: true, opponentHP: true);
            }
            //Wrap up, set variables like remaining health and whatever
            match.Finish();
            //Add match to battlelog
            Matches.Add(match);
        }
        
        static private void CharacterCreation()
        {
            Display.Write("Welcome to the arena, choose your fighter's name: ", 0);
            string name = ConsoleEx.ReadName(21, true);
            Display.WriteLine();

            Menu.CreationSelection cChoice;

            do
            {
                Player = new Character(name: name, isPlayer: true);

                Display.UpdateSidebar(true, true, true, true, true);

                cChoice = Menu.CreateCharacter();

                if (cChoice == Menu.CreationSelection.Exit)
                {
                    exiting = true;
                }
            } while (cChoice == Menu.CreationSelection.Reroll && !exiting);
        }

        static public void ListClasses()
        {

            Console.WriteLine("\nWeapon:\n");

            foreach (Type p in weaponClasses)
                Console.WriteLine(
                    Activator.CreateInstance(p)
            );

            Console.WriteLine("\nArmor:\n");

            List<Armor> list = new List<Armor>();

            foreach (Type p in armorClasses)
                foreach (EquipSlot e in Enum.GetValues(typeof(EquipSlot)))
                    switch (e)
                    {
                        case EquipSlot.Head:
                        case EquipSlot.Body:
                        case EquipSlot.Hands:
                        case EquipSlot.Feet:
                            list.Add((Armor)Activator.CreateInstance(p, new object[] { e, -1 }));
                            list.Add((Armor)Activator.CreateInstance(p, new object[] { e, -1 }));
                            list.Add((Armor)Activator.CreateInstance(p, new object[] { e, 2 }));
                            break;
                        default:
                            break;
                    }

            for(int i = 0; i < list.Count(); i++)
            {
                Console.WriteLine(i+1 + ": " + list[i]);
            }
            list.Sort();
            for (int i = 0; i < list.Count(); i++)
            {
                Console.WriteLine(i+1 + ": " + list[i]);
            }
            
            Console.ReadKey();
        }
    }
}
