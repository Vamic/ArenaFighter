using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    
    static class Menu
    {
        private static int selection;

        private static string[][] menues;

        private static object[][] menuVariables;

        private enum MenuDisplays
        {
            Levelup,
            GameOver,
            MainGame,
            Retired,
            LootSelection,
            Confirm,
            Creation
        }

        public enum MainGameSelection
        {
            Search = 1,
            Battlelog = 2,
            Retire = 3,
            FightTilDeath = 4,
            Exit = 5
        }

        public enum GameOverSelection
        {
            Battlelog = 1,
            ContinueWithOpponent = 2,
            NewFighter = 3,
            Exit = 4
        }

        public enum RetiredSelection
        {
            Battlelog = 1,
            NewFighter = 2,
            Exit = 3
        }

        public enum CreationSelection
        {
            Reroll = 0,
            Continue = 1,
            Exit = 2
        }

        public static void InitiateMenues()
        {
            menues = new string[Enum.GetValues(typeof(MenuDisplays)).Length][];
            menuVariables = new object[Enum.GetValues(typeof(MenuDisplays)).Length][];

            menuVariables[(int)MenuDisplays.GameOver] = new object[] { 0 };
            menues[(int)MenuDisplays.GameOver] = new string[] {
                "Your fighter is ^dead^, now what? Score: {0}",
                "Look at the battlelog",
                "Continue with the opponent as your fighter",
                "Choose a new fighter",
                "Exit Game"
            };

            menuVariables[(int)MenuDisplays.Retired] = new object[] { 0 };
            menues[(int)MenuDisplays.Retired] = new string[] {
                "Your fighter is ~retired~, now what? Score: {0}",
                "Look at the battlelog",
                "Choose a new fighter",
                "Exit Game"
            };

            menues[(int)MenuDisplays.MainGame] = new string[] {
                "Your fighter is rested. What now?",
                "Search for an opponent",
                "Reflect over previous battles",
                "Retire",
                "Fight forever",
                "Exit Game"
            };

            menues[(int)MenuDisplays.Creation] = new string[] {
                "Reroll fighter",
                "Continue with this fighter",
                "Exit Game"
            };
        }

        private static int MenuSelection(int startSelection, int endSelection, MenuDisplays menu)
        {
            Console.CursorVisible = false;
            Display.Clear();
            selection = startSelection;

            ConsoleKey key;
            while (true) //Keep going until a selection is made 
            {
                do
                {
                    DisplayMenu(menu);
                    key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.D:
                            selection++;
                            if (selection > endSelection)
                                selection = startSelection;
                            break;
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.W:
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.A:
                            selection--;
                            if (selection < startSelection)
                                selection = endSelection;
                            break;
                    }
                }
                while (key != ConsoleKey.Enter);

                if (selection >= startSelection && selection <= endSelection)
                {
                    Console.CursorVisible = true;
                    return selection;
                }
            }
        }

        private static void DisplayMenu(MenuDisplays index)
        {
            Display.ResetCursor();
            string[] menu = menues[(int)index];
            object[] menuVars = menuVariables[(int)index];
            for (int i = 0; i < menu.Length; i++)
            {
                string line = menu[i];
                if (menuVars != null)
                {
                    line = String.Format(line, menuVars);
                }
                if (i == selection)
                    line = "#" + line + "#";

                Display.WriteLine(line, 0);
            }
        }

        public static bool Confirm(string message)
        {
            menues[(int)MenuDisplays.Confirm] = new string[] {
                message,
                "Yes",
                "No"
            };
            int startSelection = 1;
            int endSelection = 2;
            return startSelection == MenuSelection(startSelection, endSelection, MenuDisplays.Confirm);
        }

        public static CreationSelection CreateCharacter()
        {
            int endSelection = (int)CreationSelection.Exit;
            int startSelection = endSelection - (Enum.GetNames(typeof(CreationSelection)).Length - 1);
            return (CreationSelection)MenuSelection(startSelection, endSelection, MenuDisplays.Creation);
        }

        public static MainGameSelection MainGameMenu()
        {
            int endSelection = (int)MainGameSelection.Exit;
            int startSelection = endSelection - (Enum.GetNames(typeof(MainGameSelection)).Length - 1);
            return (MainGameSelection)MenuSelection(startSelection, endSelection, MenuDisplays.MainGame);
        }

        public static GameOverSelection GameOver()
        {
            //Sets the score variable to be displayed
            menuVariables[(int)MenuDisplays.GameOver] = new object[] { Game.Player.Score };

            int endSelection = (int)GameOverSelection.Exit;
            int startSelection = endSelection - (Enum.GetNames(typeof(GameOverSelection)).Length - 1);
            return (GameOverSelection)MenuSelection(startSelection, endSelection, MenuDisplays.GameOver);
        }
        
        public static RetiredSelection Retired()
        {
            //Sets the score variable to be displayed
            menuVariables[(int)MenuDisplays.Retired] = new object[] { Game.Player.Score };
            
            int endSelection = (int)RetiredSelection.Exit;
            int startSelection = endSelection - (Enum.GetNames(typeof(RetiredSelection)).Length - 1);
            return (RetiredSelection)MenuSelection(startSelection, endSelection, MenuDisplays.Retired);
        }

        public static StatType Levelup()
        {
            string[] menu = Enum.GetNames(typeof(StatType));

            string[][] additions = new string[][]
            {
                new string[] { "Choose which stat to increase:" },
                new string[] { "Save it for later" }
            };

            menu = additions[0].Concat(menu).ToArray().Concat(additions[1]).ToArray();

            menues[(int)MenuDisplays.Levelup] = menu;

            int startSelection = 1;
            int endSelection = menues[(int)MenuDisplays.Levelup].Length - 1;
            return (StatType)(MenuSelection(startSelection, endSelection, MenuDisplays.Levelup) - 1);
        }

        public static Equipment LootSelection(Equipment[] loot)
        {
            string[] menu = loot.Select(e => e.ToString()).ToArray();

            string[][] additions = new string[][]
            {
                new string[] { "You found some ~loot~!" },
                new string[] { "Leave" }
            };

            menu = additions[0].Concat(menu).ToArray().Concat(additions[1]).ToArray();
            
            menues[(int)MenuDisplays.LootSelection] = menu;

            int startSelection = 1;
            int endSelection = menues[(int)MenuDisplays.LootSelection].Length - 1;
            int index = MenuSelection(startSelection, endSelection, MenuDisplays.LootSelection) - 1;
            if (index > loot.Length - 1)
                return null;
            else
                return loot[index];
        }
    }
}
