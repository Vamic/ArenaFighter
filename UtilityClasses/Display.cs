using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Util
{
    static class Display
    {
        //Contains the main text of the game
        public enum TextBorders
        {
            Top = 0,
            Right = 75,
            Bottom = 29,
            Left = 0
        }

        //Contains player stats n stuff
        public enum SideBorders
        {
            Top = 0,
            Right = 120,
            Bottom = 29,
            Left = 77
        }

        //Inside SideBorders, contains opponent stats
        public enum OpponentSideBorders
        {
            Top = SideBorders.Top,
            Right = SideBorders.Right,
            Bottom = 11,
            Left = SideBorders.Left + 21
        }

        //Amount of text to clear
        private static List<int> textToClear = new List<int>();

        //Characters that change the color of the text, wont be shown
        private const string colorCharacers = "~^#¤";

        //Colors to swap to, index correlates to the character
        private static readonly ConsoleColor[] colors = { ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.DarkGray };

        private static List<ConsoleColor> previousColors;

        //Keeps track of how much space equipment takes up in the sidebar for when we update it
        private static int sidebarGearRows;

        internal static void Initialize()
        {
            //Previous colors that get restored after color swaps end
            previousColors = new List<ConsoleColor> { ConsoleColor.Gray };
            //Set the cursor where we want it
            Console.CursorLeft = (int)TextBorders.Left;
            Console.CursorTop = (int)TextBorders.Top;
            Display.Clear();
        }
        
        //Does all the writing
        static private void DoWrite(string input, Type borders, bool isWriteLine, int speed)
        {
            //Count columns so we can clear the screen more efficiently
            int columnCount = 0;
            //How many times the colors have been switched so we can properly switch them back
            int resetColors = 0;
            
            //Limits of the text area
            int top = Convert.ToInt32(Enum.Parse(borders, "Top"));
            int left = Convert.ToInt32(Enum.Parse(borders, "Left"));
            int bot = Convert.ToInt32(Enum.Parse(borders, "Bottom"));
            int right = Convert.ToInt32(Enum.Parse(borders, "Right"));

            //Do the write
            for (int i = 0; i < input.Length; i++)
            {
                //Go back one color 
                if (resetColors > 0)
                {
                    //Restore color
                    Console.ForegroundColor = previousColors.Last();
                    //Remove from the colorchanges
                    previousColors.RemoveAt(previousColors.LastIndexOf(Console.ForegroundColor));
                    resetColors--;
                }

                //If we're too far left (happens if console.write hits the edge of the console)
                if(Console.CursorLeft < left)
                {
                    Console.CursorLeft = left;
                }
                //If we're too far right, newline
                if (Console.CursorLeft > right || input[i] == "\n"[0])
                {
                    //Store the columns for clearing
                    if (typeof(TextBorders) == borders)
                    {
                        textToClear.Add(columnCount);
                        columnCount = 0;
                    }
                    NewLine(borders);
                }
                //Wait for user to let the text continue
                if (Console.CursorTop >= bot)
                {
                    if (typeof(TextBorders) == borders)
                    {
                        //Change to default color and add to color reset counter
                        previousColors.Add(Console.ForegroundColor);
                        Console.ResetColor();
                        Console.Write("Press any key to clear the text and continue.");
                        resetColors++;
                        //Add columns to clear so it clears it.
                        textToClear.Add("Press any key to clear the text and continue.".Length);
                        //Wait for any key
                        Console.ReadKey(true);
                        Display.Clear(borders);
                        //Reset columns since we just cleared
                        columnCount = 0;
                    }
                    Console.CursorLeft = left;
                    Console.CursorTop = top;
                }
                //Do colors, doesn't print the color character
                if (colorCharacers.IndexOf(input[i]) > -1)
                {
                    //Get color index
                    int index = colorCharacers.IndexOf(input[i]);
                    //Check if not that color, if so switch to it and add the current to the reset list
                    if (Console.ForegroundColor != colors[index])
                    {
                        previousColors.Add(Console.ForegroundColor);
                        Console.ForegroundColor = colors[index];
                    }
                    else
                    {
                        //Restore previous color
                        resetColors++;
                    }
                    //Ignore this character
                    continue;
                }

                if (input[i] != "\n"[0])
                    Console.Write(input[i]);

                //Amount of characters before waiting, higher number is faster, unless 0 which is instant
                if (speed > 0 && i % speed == 0)
                    Thread.Sleep(20);
                
                //Add to the columns to clear
                if (typeof(TextBorders) == borders)
                    columnCount++;
            }
            Console.ForegroundColor = previousColors.Last();

            if (typeof(TextBorders) == borders)
            {
                if(Console.CursorTop > textToClear.Count-1)
                {
                    textToClear.Add(columnCount);
                } else
                {
                    int stored = textToClear[Console.CursorTop];
                    textToClear[Console.CursorTop] = columnCount > stored ? columnCount : stored;
                }
            }

            //Writeline gets a newline at the end.
            if(isWriteLine)
            {
                NewLine(borders);
            }
            
        }

        //Bunch of overloads that redirect to DoWrite
        static public void Write(string input, int speed = 2)
        {
            DoWrite(input, typeof(TextBorders), false, speed);
        }

        static public void Write(string input, Type borders, int speed = 2)
        {
            DoWrite(input, borders, false, speed);
        }

        //Except this one, this one makes a new line
        static public void WriteLine()
        {
            NewLine(typeof(TextBorders));
        }

        static public void WriteLine(string input, int speed = 2)
        {
            DoWrite(input, typeof(TextBorders), true, speed);
        }

        static public void WriteLine(string input, Type borders, int speed = 2)
        {
            DoWrite(input, borders, true, speed);
        }

        //Updates relevant info on the sidebar
        static public void UpdateSidebar(
            bool playerName = false, 
            bool playerLevel = false,
            bool playerHP = false,
            bool playerStats = false,
            bool playerGear = false,
            bool opponentName = false,
            bool opponentLevel = false,
            bool opponentHP = false,
            bool opponentStats = false)
        {
            Console.CursorVisible = false;

            //Save position
            int cursorTop = Console.CursorTop;
            int cursorLeft = Console.CursorLeft;
            
            Console.CursorLeft = (int)SideBorders.Left;
            Console.CursorTop = (int)SideBorders.Top;
            Character character = Game.Player;

            Type borders = typeof(SideBorders);

            //Write name
            if (playerName)
            {
                Display.Write(character.ToString(), borders, 0);
                ClearLine(borders);
            }
            NewLine(borders);

            //Level
            if (playerLevel)
                Display.WriteLine("Level " + character.Level + " ", borders, 0);
            else NewLine(borders);

            Display.WriteLine("Score " + character.Score + "   ", borders, 0);

            NewLine(borders);

            if(playerStats || playerGear)
                Display.WriteLine("DMG: " + character.GetDamageRange() + "   ", borders, 0);
            else NewLine(borders);
            if (playerHP)
                Display.WriteLine("HP: " + HPStringBuilder(character.CurrentHealth, character.MaxHealth) + "   ", borders, 0);
            else NewLine(borders);

            //Get stats
            EquipmentManager gear = character.Gear;
            Statistics total = character.Stats + gear.Stats;

            //Write stats


            if (playerGear)
                Display.WriteLine("Defense: " + gear.GetDefense() + " ", borders, 0);
            else NewLine(borders);

            if (playerStats)
            {
                Display.WriteLine("Str: " + StatStringBuilder(character.Stats.Strength, gear.Stats.Strength) + "  ", borders, 0);
                Display.WriteLine("Dex: " + StatStringBuilder(character.Stats.Dexterity, gear.Stats.Dexterity) + "  ", borders, 0);
                Display.WriteLine("Con: " + StatStringBuilder(character.Stats.Constitution, gear.Stats.Constitution) + "  ", borders, 0);
                Display.WriteLine("Foc: " + StatStringBuilder(character.Stats.Focus, gear.Stats.Focus) + "  ", borders, 0);
            }
            else
            {
                NewLine(borders);
                NewLine(borders);
                NewLine(borders);
                NewLine(borders);
            }

            NewLine(borders);

            //Gear
            if(playerGear)
            {
                int beforeGearTop = Console.CursorTop;
                foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
                {
                    //Write the gear, write 2 handed weapons in dark gray as offhand
                    Equipment item = gear.Get(slot);
                    if(item != null && slot == EquipSlot.Offhand && item.Slots[0] != EquipSlot.Offhand)
                        Display.Write(slot + ": ¤" + item + "¤", borders, 0);
                    else
                        Display.Write(slot + ": " + item, borders, 0);
                    //Clear the rest of the line
                    ClearLine(borders);
                    //Go to next line
                    NewLine(borders);
                }
                //Clear any text below our gear (caused by long names of previously equipped gear)
                while(Console.CursorTop - beforeGearTop < sidebarGearRows)
                {
                    ClearLine(borders);
                    NewLine(borders);
                }
                //Save the height of our gearlist
                sidebarGearRows = Console.CursorTop - beforeGearTop;
            } else
            {
                //If we're not writing gear, newline to the end
                for (int i = 0; i < sidebarGearRows; i++)
                    NewLine(borders);
            }

            //Opponent
            if(opponentHP || opponentName || opponentLevel || opponentStats)
            {
                Console.CursorLeft = (int)OpponentSideBorders.Left;
                Console.CursorTop = (int)OpponentSideBorders.Top;
                character = Game.Opponent;

                borders = typeof(OpponentSideBorders);

                //Write name
                if (opponentName)
                    Display.WriteLine(character.ToString(), borders, 0);
                else
                    NewLine(borders);

                //Level

                if (opponentLevel)
                    Display.WriteLine("Level " + character.Level, borders, 0);
                else
                    NewLine(borders);
                NewLine(borders);//Replaces score line
                NewLine(borders);

                NewLine(borders);//Replaces damage line
                if (opponentHP) Display.WriteLine("HP: " + HPStringBuilder(character.CurrentHealth, character.MaxHealth) + "   ", borders, 0);
                
                //Get stats
                gear = character.Gear;
                total = character.Stats + gear.Stats;

                //Write stats
                if (opponentStats)
                {
                    Display.WriteLine("Defense: " + gear.GetDefense() + " ", borders, 0);
                    Display.WriteLine("Str: " + StatStringBuilder(character.Stats.Strength, gear.Stats.Strength) + "  ", borders, 0);
                    Display.WriteLine("Dex: " + StatStringBuilder(character.Stats.Dexterity, gear.Stats.Dexterity) + "  ", borders, 0);
                    Display.WriteLine("Con: " + StatStringBuilder(character.Stats.Constitution, gear.Stats.Constitution) + "  ", borders, 0);
                    Display.WriteLine("Foc: " + StatStringBuilder(character.Stats.Focus, gear.Stats.Focus) + "  ", borders, 0);
                }
                else {
                    NewLine(borders);
                    NewLine(borders);
                    NewLine(borders);
                    NewLine(borders);
                    NewLine(borders);
                }
            }

            //Restore position
            Console.CursorTop = cursorTop;
            Console.CursorLeft = cursorLeft;

            Console.CursorVisible = true;
        }

        internal static void ResetCursor()
        {
            Console.CursorLeft = (int)TextBorders.Left;
            Console.CursorTop = (int)TextBorders.Top;
        }

        private static void ClearLine(Type borders)
        {
            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;
            int right = Convert.ToInt32(Enum.Parse(borders, "Right"));

            //CursorTop changes automatically if we hit the edge of the console
            while (Console.CursorLeft < right && Console.CursorTop == cursorTop)
                Console.Write(" ");

            //Restore our previous position
            Console.CursorLeft = cursorLeft;
            Console.CursorTop = cursorTop;
        }

        private static string StatStringBuilder(int statBase, int statBonus)
        {
            //Operator to display
            string op = "+";
            //Dark gray if 0
            string color = "¤";
            
            if (statBonus < 0)
            {
                op = "-";
                //Red if negative
                color = "^";
            } else if (statBonus > 0)
            {
                //Green if positive
                color = "~";
            }

            return statBase + statBonus + " (" + statBase + " " + color + op + " " + Math.Abs(statBonus) + color + ")";
        }

        private static string HPStringBuilder(int current, int max)
        {
            string result;
            float percentage = (float)current / (float)max;

            //Dark gray if dead
            if (percentage <= 0)
                result = "¤";
            //Red if critical health
            else if (percentage <= 0.1)
                result = "^";
            //Yellow of low
            else if (percentage <= 0.3)
                result = "#";
            //Otherwise green
            else
                result = "~";

            return result + current + " / " + max + result;
        }

        public static void Clear()
        {
            for (int i = 0; i < textToClear.Count(); i++)
            {
                Console.CursorTop = i;
                Console.CursorLeft = (int)TextBorders.Left;

                int lineLength = textToClear[i];
                for (int j = 0; j < lineLength; j++)
                {
                    Console.Write(" ");
                }
            }

            textToClear = new List<int>();

            Console.CursorTop = (int)TextBorders.Top;
            Console.CursorLeft = (int)TextBorders.Left;
        }

        public static void Clear(Type borders)
        {
            if (borders == typeof(TextBorders)) Clear();
            else
            {
                int top = Convert.ToInt32(Enum.Parse(borders, "Top"));
                int left = Convert.ToInt32(Enum.Parse(borders, "Left"));
                int bot = Convert.ToInt32(Enum.Parse(borders, "Bottom"));
                int right = Convert.ToInt32(Enum.Parse(borders, "Right"));
                for (int i = top; i <= bot; i++)
                {
                    Console.CursorLeft = left;
                    Console.CursorTop = i;
                    for (int j = left; j < right; j++)
                    {
                        Console.Write(" ");
                    }
                }
                
                Console.CursorTop = top;
                Console.CursorLeft = left;
            }
        }

        private static void NewLine(Type borders)
        {
            if (!borders.IsEnum) throw new Exception("Passed borders is not an enum.");
            int left = Convert.ToInt32(Enum.Parse(borders, "Left"));
            Console.CursorLeft = left;
            Console.CursorTop++;
        }
    }
}
