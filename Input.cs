namespace BattleBoats
{
    static class Input
    {
        public static int MenuOption(int optionCount) // Ask for an option from a list in the form of an integer
        {
            int r;
            while (true) // Loop until the user enters a valid integer
            {
                Console.Write(Text.IntOption(optionCount)); // Display the prompt
                try
                {
                    r = Convert.ToInt32(Console.ReadLine()); // Try to convert the user input into an integer
                }
                catch
                {
                    Console.Write(Text.IntOptionInvalid); // If the conversion fails, complain and try again
                    continue;
                }
                if (r > 0 && r <= optionCount) break; // If the input is in the correct range, return it
                Console.Write(Text.IntOptionInvalid); // Otherwise, complain
            }
            return r;
        }
        public class GridInput
        {
            public delegate bool KeyAction(ref int x, ref int y); // Type for input callbacks. For movement keys, return whether movement should be accepted. For others, return whether to continue the cycle or exit
            private KeyAction pressAction = (ref int _, ref int _) => false; // For when space is pressed. By default, exit immediately
            private KeyAction escAction = (ref int _, ref int _) => false; // For when escape is pressed. Exit by default
            private KeyAction moveAction = (ref int x, ref int y) => (x >= 0 && y >= 0 && x < 8 && y < 8); // Allow the movement if it is in bounds
            private char[,] gridState; // What chaaracters are in each cell
            private Board mainGrid; // If rendering board(s), the main board
            private Board secondaryGrid; // If rendering boards, the secondary board
            private bool isBoards = false; // Use boards to render? 
            private bool selectSecond = false; // If rendering two boards, should the cursor appear on the second board?
            private bool forceExit = false; // Force the grid to exit early
            public ConsoleKey lastKey { get; private set; } // Which key was last pressed
            public string statusLine = ""; // Displayed under the grid
            public void ForceExit() => forceExit = true; // Force the grid to exit early
            public GridInput(char[,] grid) // Create a new grid from a character array
            {
                gridState = grid;
            }
            public GridInput(Board main, Board secondary = null, bool useSecond = false) // Create a new grid from one or two boards, optionally specifying to place the cursor over the second one
            {
                mainGrid = main;
                secondaryGrid = secondary;
                isBoards = true;
                selectSecond = useSecond;
            }
            public void SetPressAction(KeyAction action) => pressAction = action; //
            public void SetMoveAction(KeyAction action) => moveAction = action;   // Assign input callbacks
            public void SetEscapeAction(KeyAction action) => escAction = action;  //
            private int GetLineCount() => ((Game.Settings.showGrid ? 19 : 9) * (((Game.Settings.displayMode == Game.Settings.DisplayMode.Horizontal) || !isBoards || (secondaryGrid == null)) ? 1 : 2)); // Determine the number of lines the grid(s) occupy based on the settings
            private void UpdateCursor(int x, int y, int pos) // Move the cursor to the correct grid cell
            {
                int xpos = x;
                int ypos = y;
                if (Game.Settings.displayMode == Game.Settings.DisplayMode.Horizontal && selectSecond) xpos = Game.Settings.showGrid ? (x * 2) + 23 : x + 11;
                else xpos = Game.Settings.showGrid ? (x * 2) + 3 : x + 1;
                ypos = pos - GetLineCount() + (Game.Settings.showGrid ? (y * 2) + 3 : y + 1);
                if (Game.Settings.displayMode == Game.Settings.DisplayMode.Vertical && selectSecond) ypos += Game.Settings.showGrid ? 19 : 9;
                Console.SetCursorPosition(xpos, ypos);
            }
            private void Draw() // Draw out the current grid state
            {
                if (isBoards) // If using Boards, have them draw themself
                {
                    if (secondaryGrid == null) mainGrid.Show();
                    else mainGrid.ShowWith(secondaryGrid);
                    Console.Write(statusLine);
                    Console.Write(Text.LineBlank); // Cover up any extra characters from the last status
                    return;
                }
                Console.Write(Text.GridStart(true)); // Draw the start of the grid
                for (int y = 0; y < 8; y++) // For each row:
                {
                    Console.Write(Text.RowStart(y + 1)); // Draw the start of the row
                    for (int x = 0; x < 8; x++) // For each cell:
                    {
                        Console.Write(gridState[y, x]); // Draw the cell contents
                        Console.Write(Text.CellSep()); // Add a separator
                    }
                    Console.Write('\n');
                    Console.Write(Text.RowSep(-1)); // Add a separator
                }
                Console.Write(statusLine);
                Console.Write(Text.LineBlank);
            }
            private void Redraw(int pos) // Overwrite the grid
            {
                Console.SetCursorPosition(0, pos - GetLineCount()); // Move back to the start position
                Draw(); // Draw the grid
            }
            public (int, int) Run() // Execute the input cycle
            {
                Draw(); // Draw the grid to start with
                int x = 0; //
                int y = 0; // Start in the top left
                forceExit = false; // Don't exit yet
                int currentPos = Console.CursorTop; // Log where we start
                while (!forceExit) // Loop until we need to stop
                {
                    Redraw(currentPos); // Redraw the screen
                    UpdateCursor(x, y, currentPos); // Move the cursor th the current cell
                    ConsoleKeyInfo key = Console.ReadKey(true); // Read a key from the user
                    int nx = x; //
                    int ny = y; // Store the target location
                    bool isMove = false; // Is the press a key move
                    lastKey = key.Key; // Store the last key so it is accessible from the callback
                    switch (key.Key)
                    {
                        case ConsoleKey.LeftArrow: // If the key is an arrow, move the target and record that it is a move
                            nx--;
                            isMove = true;
                            break;
                        case ConsoleKey.RightArrow:
                            nx++;
                            isMove = true;
                            break;
                        case ConsoleKey.UpArrow:
                            ny--;
                            isMove = true;
                            break;
                        case ConsoleKey.DownArrow:
                            ny++;
                            isMove = true;
                            break;
                        case ConsoleKey.Spacebar: // If it is another relevant key, run its callback
                            if (!pressAction(ref x, ref y)) forceExit = true;
                            break;
                        case ConsoleKey.Escape:
                            if (!escAction(ref x, ref y)) forceExit = true;
                            break;
                    }
                    if (isMove) // If we need to move
                    {
                        if (moveAction(ref nx, ref ny)) // Check if the move is valid
                        {
                            x = nx; //
                            y = ny; // Update the position
                        }
                    }
                }
                Redraw(currentPos); // Redraw the screen before continuing with normal output
                Console.WriteLine(); // Move onto the next line
                return (x, y); // Return the final position
            }
        }
    }
}
