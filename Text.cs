namespace BattleBoats
{
    static class Text // All of the text content of the game is stored here so that if the game were to be translated only this would have to change. I have no plan of doing so but I thought that this was a good way of organising the project.
    {
        private static string Selector(bool checkVal, string option1, string option2) =>
            $"({(checkVal ? "\x1b[1m" : "")}{option1}\x1b[m/{(checkVal ? "" : "\x1b[1m")}{option2}\x1b[m)"; // Display two options with the active one bold
        public const string MainMenu =
@"Select option:
1 - Settings
2 - New game
3 - Continue
4 - Quit
";
        public static string IntOption(int c) => $"Enter option (1-{c}): ";
        public const string IntOptionInvalid = "Invalid option.\n";
        public static string SettingsMenu() =>
$@"Select option:
1 - Toggle formatting {Selector(Game.Settings.displayMode == Game.Settings.DisplayMode.Horizontal, "Side by side", "Vertical")}
2 - Toggle gridlines {Selector(Game.Settings.showGrid, "Yes", "No")}
3 - Back
";
        public const string NewGameRequestName = "Enter name: ";
        public static string GameFile(string n) => $"games/{n}.bin";
        public const string NewGameExists = "Game already exists. (1:overwrite, 2:continue, 3:cancel)? ";
        public static char CellChar(CellState c, bool isAttack) // The char mapped to a given cell state, based on whether unhit ships should be revealed
        {
            switch (c)
            {
                case CellState.None: return '.';
                case CellState.Miss: return '#';
                case CellState.Hit: return '!';
                case CellState.HitSunk: return '~';
                case CellState.Unhit: return isAttack ? '.' : '@';
                default: return '?';
            }
        }
        public static string CellSep() => Game.Settings.showGrid ? "|" : "";
        public static string RowStart(int i) => Game.Settings.showGrid ? $"|{i}|" : $"{i}";
        public static string GridStart(bool forceSingle = false)
        {
            if (Game.Settings.displayMode == Game.Settings.DisplayMode.Vertical || forceSingle) return !Game.Settings.showGrid ? " ABCDEFGH\n" :
@"  +-+-+-+-+-+-+-+-+
  |A|B|C|D|E|F|G|H|
+-+-+-+-+-+-+-+-+-+
";
            else return !Game.Settings.showGrid ? " ABCDEFGH  ABCDEFGH\n" :
@"  +-+-+-+-+-+-+-+-+   +-+-+-+-+-+-+-+-+
  |A|B|C|D|E|F|G|H|   |A|B|C|D|E|F|G|H|
+-+-+-+-+-+-+-+-+-+ +-+-+-+-+-+-+-+-+-+
";
        }
        public static string RowSep(int forceType = 0)
        {
            if (Game.Settings.showGrid)
            {
                if (forceType != -1 && (Game.Settings.displayMode == Game.Settings.DisplayMode.Horizontal || forceType == 1)) return "+-+-+-+-+-+-+-+-+-+ +-+-+-+-+-+-+-+-+-+\n";
                else return "+-+-+-+-+-+-+-+-+-+\n";
            }
            return "";
        }
        public static string EvalMove(CellState c) // Explanation of what happened during a move
        {
            switch (c)
            {
                case CellState.Miss: return "It's a miss.";
                case CellState.Hit: return "It's a hit, but the ship isn't sunk.";
                case CellState.HitSunk: return "It's a hit, and the ship is sunk.";
                default: return "";
            }
        }
        public static string ComputerMove(int x, int y, CellState c) => $"The computer shot at ({x},{y}). {EvalMove(c)}"; // Explanation of a computer move
        public static string LineBlank = new string(' ', 50); // Cover up text when overwriting a line
        public const string NewGameChooseBoat = "Select boat location";
    }
}
