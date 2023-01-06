#pragma warning disable 8604
namespace BattleBoats
{
    class Program
    {
        enum State { Null, Menu, Settings, NewGame, Continue, Game }; // To avoid unbounded recursion, states are used to control the program flow. Null exits the program
        static int Main()
        {
            Dictionary<State, Func<State>> states = new Dictionary<State, Func<State>>();
            states.Add(State.Menu, () =>
            {
                Console.Write(Text.MainMenu);
                int option = Input.MenuOption(4); // Read an integer from 1 to 4 from the user
                switch (option) // Choose the next state based on the user input
                {
                    case 1: return State.Settings;
                    case 2: return State.NewGame;
                    case 3: return State.Continue;
                    case 4: return State.Null;
                }
                return State.Null;
            });
            states.Add(State.Settings, () =>
            {
                Console.Write(Text.SettingsMenu());
                int option = Input.MenuOption(3);
                switch (option)
                {
                    case 1: Game.Settings.displayMode = (Game.Settings.DisplayMode)(1 - (int)Game.Settings.displayMode); break; // Flip the display mode
                    case 2: Game.Settings.showGrid = !Game.Settings.showGrid; break; // Flip the grid mode
                    case 3: return State.Menu;
                }
                return State.Settings;
            });
            states.Add(State.NewGame, () =>
            {
                Console.Write(Text.NewGameRequestName);
                string path = Text.GameFile(Console.ReadLine()); // Get the file name
                if (File.Exists(path)) // Check if there is an existing file and ask the user about it
                {
                    Console.Write(Text.NewGameExists);
                    int option = Input.MenuOption(3);
                    switch (option)
                    {
                        case 1: File.Delete(path); break; // Overwrite the file
                        case 2: Game.file = path; return State.Game; // Resume the file
                        case 3: return State.Menu; // Cancel
                    }
                }
                Game.file = path; // Set the file path
                Game.player = Board.CreateInteractive(true); // Get the player to input the ship layout
                Random r = new Random(); // Create a random number generator to setup the computer board
                Game.computer = Game.player.ComputerLike(r); // Create a new board with the same ship sizes as the player board
                Game.Save(); // Save the state of the game
                return State.Game;
            });
            states.Add(State.Continue, () =>
            {
                Console.Write(Text.NewGameRequestName);
                string path = Text.GameFile(Console.ReadLine()); // Get the file name
                if (File.Exists(path)) // Check if the saved game exists
                {
                    Game.file = path; // Set the game path
                    return State.Game; // Go to playing the game
                }
                return State.Menu; // If it doesn't exist, go back to the menu
            });
            states.Add(State.Game, () =>
            {
                Game.Load();
                Random r = new Random(); // Create a random number generator to use in the game
                Input.GridInput gridInput = new Input.GridInput(Game.player, Game.computer, true); // Initialise the grid with the player and computer boards and have the cursor placed on the secondary grid
                gridInput.SetPressAction((ref int x, ref int y) => // Register an action for when the space key is pressed
                {
                    if (Game.computer.hitMask[x, y]) return true; // If the target location has already been shot at, continue
                    Game.computer.hitMask[x,y] = true; // Mark the target location as shot at
                    if (Game.computer.IsWon()) return false; // Exit if the player has won
                    gridInput.statusLine = Game.player.ComputerMove(r); // Make a computer move, and display it as a status line
                    if (Game.player.IsWon()) return false; // Exit if the computer won
                    Game.Save(); // Save the game
                    return true; // Continue with the play cycle
                });
                gridInput.Run(); // Begin the game cycle
                Game.Save();
                return State.Menu; // Return to the menu
            });
            State s = State.Menu; // Start in the main menu
            while (s != State.Null) s = states[s](); // Move to the next state when the current one returns
            return 0;
        }
    }
}
