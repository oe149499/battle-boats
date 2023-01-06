namespace BattleBoats
{
    struct Mask // Represents a board
    {
        private long mask; // Internally used to store the grid, with each bit mapped to one cell
        public Mask(long i) // Create a mask from the internal value
        {
            mask = i;
        }
        public Mask(int x, int y) // Create a mask with a given cell filled
        {
            mask = 0L;
            this[x, y] = true;
        }
        public Mask(Random r) // Create a maske with a random cell filled
        {
            mask = 0L;
            this[r.Next(7), r.Next(7)] = true;
        }
        static void ApplyDir(ref int x, ref int y, Dir d) // Move a coordinate in a given direction
        {
            switch (d)
            {
                case Dir.Up:
                    y--;
                    break;
                case Dir.Down:
                    y++;
                    break;
                case Dir.Left:
                    x--;
                    break;
                case Dir.Right:
                    x++;
                    break;
            }
        }
        public Mask(int x, int y, Dir d, int l) // Create a line starting from a given point and extending a certain distance in a given direction (for generating ships)
        {
            mask = 0L;
            this[x, y] = true;
            for (int i = 0; i < l; i++)
            {
                ApplyDir(ref x, ref y, d);
                if (x >= 0 & y >= 0 & x < 8 & y < 8) this[x, y] = true;
                else break;
            }
        }
        public (int, int) Randomize(Random r) // Set exactly one random cell and return its coordinates (for choosing a computer guess)
        {
            int x = r.Next(8); 
            int y = r.Next(8);
            mask = 0L;
            this[x, y] = true;
            return (x, y);
        }
        public int Count() // Count how many cells are filled
        {
            int c = 0;
            for (int x = 0; x < 64; x++) if (this[x]) c++;
            return c;
        }
        public bool this[int index] // Get based on a single index, with values higher than 8 wrapping to the next row
        {
            get => ((1L << index) & mask) != 0; // Shift 1 by the index to align it with the corresponding cell and bitwise AND them to check the bit in the mask
            set
            {
                if (value) mask |= (1L << index); // To set the value, OR the corresponding bit with 1
                else mask &= ~(1L << index); // To unset the value, AND the bit with 0
            }
        }
        public bool this[int x, int y] // Compute the single-dimensional index and use it to apply the operation
        {
            get => this[x + (y << 3)];
            set => this[x + (y << 3)] = value;
        }
        public static implicit operator Mask(long l) => new Mask(l); // Masks and longs can be used interchangeably
        public static explicit operator long(Mask m) => m.mask;
        public static Mask operator &(Mask a, Mask b) => new Mask(a.mask & b.mask); // Bitwise operations are forwarded to the underlying long
        public static Mask operator |(Mask a, Mask b) => new Mask(a.mask | b.mask);
        public static implicit operator bool(Mask m) => m.mask != 0L; // A mask is truthy if it has any cells filled
        public static Mask operator ~(Mask m) => new Mask(~m.mask);
    }
    enum CellState { None, Unhit, HitSunk, Hit, Miss }; // What states a cell can appear to be in
    enum Dir { Up, Down, Left, Right }; // Directions a ship can face

    class Board
    {
        public Mask hitMask; // Which cells have been shot at
        public Mask[] shipMasks; // Which cells each ship occupies
        private Mask allShipsMask; // Which cells are occupied by any ship (cached so it is only computed when ships are updated)
        private bool isComplex; // Are there any ships with more than 2 cells?
        public Board(Mask hits) // Initialise with default values
        {
            hitMask = hits;
            shipMasks = new Mask[0];
            allShipsMask = 0L;
            isComplex = false;
        }
        public Board()
        {
            hitMask = 0L;
            shipMasks = new Mask[0];
            allShipsMask = 0L;
            isComplex = false;
        }
        public CellState GetCell(int x, int y) // Get the state of a single cell
        {
            if (hitMask[x, y]) // Check if it has been fired at
            {
                foreach (Mask m in shipMasks) // Iterate over each ship
                {
                    if (m[x, y] && !(m & ~hitMask)) // If the cell is in the ship and there are no cells in the ship which have not been fired at
                    {
                        return CellState.HitSunk; // The cell is a hit, and the ship is fully sunk
                    }
                }
                if (allShipsMask[x, y]) return CellState.Hit; // The cell is a hit, but another cell in the ship is not hit
                return CellState.Miss; // The cell is a miss
            }
            else if (allShipsMask[x, y]) return CellState.Unhit; // There is a ship at the cell's location, but it has not been hit
            return CellState.None; // The cell has no ship and has not been fired at
        }
        public char[,] GetGrid(bool isAttack, char[,]? g = null) // Get all of the cells on the board so they can be displayed
        {
            char[,] grid = g ?? new char[8, 8]; // If an array was provided, use it, otherwise create a new one
            for (int y = 0; y < 8; y++)
            {          // Iterate over each cell
                for (int x = 0; x < 8; x++)
                {
                    grid[y, x] = Text.CellChar(GetCell(x, y), isAttack); // Look up the corresponding cell and find the relevant character
                }
            }
            return grid;
        }
        public IEnumerable<CellState> GetRow(int r) // Iterate over each cell in a row and yield it
        {
            for (int i = 0; i < 8; i++) yield return GetCell(i, r);
        }
        public void RecomputeMasks() // Update the cached mask values
        {
            allShipsMask = 0L; //
            isComplex = false; // Reset the values to their defaults 
            foreach (Mask m in shipMasks) // Iterate over each ship
            {
                allShipsMask |= m; // Add the ship to the combined mask
                if (m.Count() > 1) isComplex = true; // If the ship has more than one cell, then the board is complex
            }
        }
        public static Board LoadFromFile(BinaryReader file) // Create a new board from a file stream
        {
            Board b = new Board(); // Initialise the board
            int shipMasks = file.ReadInt32(); // Read how many ships there are
            b.hitMask = file.ReadInt64(); // Read the mask of which cells have been fired at
            Array.Resize(ref b.shipMasks, shipMasks); // Resize the ship mask array to the number of ships
            for (int i = 0; i < shipMasks; i++) // Repeat for the number of ships
            {
                b.shipMasks[i] = file.ReadInt64(); // Store the next ship in the board
            }
            b.RecomputeMasks(); // Compute the cached values
            return b; // Give back the board
        }
        private static Dir KeyDir(ConsoleKey key) // Convert between an arrow key and the corresponding direction
        {
            switch (key)
            {
                case ConsoleKey.UpArrow: return Dir.Up;
                case ConsoleKey.DownArrow: return Dir.Down;
                case ConsoleKey.LeftArrow: return Dir.Left;
                case ConsoleKey.RightArrow: return Dir.Right;
                default: return Dir.Up;
            }
        }
        public static Board CreateInteractive(bool doComplex = false) // Create a new board interactively
        {
            Board board = new Board(); // Initialise the board
            char[,] grid = new char[8, 8]; // Create a display grid
            for (int i = 0; i < 8; i++) for (int j = 0; j < 8; j++) grid[i, j] = '.'; // Initialise the grid to empty
            Input.GridInput gridInput = new Input.GridInput(grid); // Create a grid input instance
            Mask currentShips = 0L; // Keep track of where ships are currently
            Mask[] ships = new Mask[5]; // The ships that have currently been placed
            board.shipMasks = ships; // Store the ships in the board
            int cX = -1; //
            int cY = -1; // The start location of the ships. -1 means the ship has not yet been started
            int count = 0; // How many ships have been placed so far
            gridInput.statusLine = Text.NewGameChooseBoat;
            gridInput.SetMoveAction((ref int x, ref int y) => // Check if an arrow key move is valid
            {
                if (x < 0 || x >= 8 || y < 0 || y >= 8) return false; // If outside of bounds, don't continue
                if (cX == -1) return true; // If the ship is not started, impose no further restrictions
                return (!currentShips[x, y]) && (x == cX || y == cY); // Only allow the move if there is no ship on the target location and the target is in either the same row or column as the start position
            });
            gridInput.SetEscapeAction((ref int x, ref int y) => // What to do when the escape key is pressed - cancel current ship placement
            {
                grid[cY, cX] = '.'; // Reset ship start marker
                cX = -1; //
                cY = -1; // Reset start position
                return true; // Continue running
            });
            gridInput.SetPressAction((ref int x, ref int y) => // Space pressed
            {
                if (cX == -1) // If the ship has not been started
                {
                    if (currentShips[x, y]) return true; // If there is already a ship, cancel and continue
                    cX = x; // Update the start 
                    cY = y; //
                    grid[y, x] = '#'; // Place a marker
                    return true; // Continue recieving input
                }
                Dir d = Dir.Up; //
                int l = 0;      // Initialise the length and direction
                if (x < cX) { l = cX - x; d = Dir.Left; }  //
                if (x > cX) { l = x - cX; d = Dir.Right; } //
                if (y < cY) { l = cY - y; d = Dir.Up; }    // Determine the direction and calculate the length
                if (y > cY) { l = y - cY; d = Dir.Down; }  //
                Mask m = new Mask(cX, cY, d, l); // Create a ship mask with the target properties
                if (m & currentShips) return true; // If it overlaps a ship, don't allow the ship to be placed
                currentShips |= m; // Add the mask to the current ships in order to block future ships from overlapping with it
                ships[count] = m; // Store the mask in the board
                board.RecomputeMasks();     //
                board.GetGrid(false, grid); // Draw the new layout to the grid so it can be rendered
                count++; // Move on to the next ship
                cX = -1; //
                cY = -1; //
                return count < 5; // Stop if all five ships are placed
            });
            gridInput.Run(); // Start the input loop
            board.RecomputeMasks(); // Refresh the cache
            return board;
        }

        public void SaveToFile(BinaryWriter file) // Save the current state to a stream
        {
            file.Write(shipMasks.Length); // Write the number of ships
            file.Write((long)hitMask); // Write which spaces have been fired at
            foreach (Mask m in shipMasks) file.Write((long)m); // Write each of the ships
        }
        public string RenderRow(int r, bool isAttack = false) // Get a string representation of a row
        {
            return $"{Text.RowStart(r + 1)}{String.Join(Text.CellSep(), GetRow(r).Select(x => Text.CellChar(x, isAttack)))}{Text.CellSep()}";
        } //                                Join the cells              get the cells and map each to the corresponding char       add another separator
        public void Show(bool isAttack = false) // Display the whole grid from the perspective of either the attacker or the defender
        {
            Console.Write(Text.GridStart(true)); // Display the start of the grid, forcing it to be the single-grid version
            for (int i = 0; i < 8; i++) // Iterate over the rows
            {
                Console.WriteLine(RenderRow(i, isAttack)); // Render the row
                Console.Write(Text.RowSep(-1)); // Display the separator, forcing the single-grid version
            }
        }
        public void ShowWith(Board b) // Show along with another board, taking into account which one should show unhit ships
        {
            if (Game.Settings.displayMode == Game.Settings.DisplayMode.Horizontal) // Check if the boards should render horizontally
            {
                Console.Write(Text.GridStart()); // Display the beginning of the grid
                for (int i = 0; i < 8; i++) // For each row:
                {
                    Console.Write($"{RenderRow(i)} {b.RenderRow(i, true)}\n"); // Render the rows of each grid, showing the attacker perspective for the second one
                    Console.Write(Text.RowSep()); // Show a row separator
                }
            }
            else
            {
                Show();       //
                b.Show(true); // Show this board, then show the other
            }
        }
        private static readonly (int, int)[] MoveAdj = { (-1, 0), (0, -1), (1, 0), (0, 1) }; // Directions a ship's cells can be adjacent to each other
        public string ComputerMove(Random r) // Execute a computer move and return the evaluation
        {
            if (isComplex) for (int x = 0; x < 8; x++)
                {            // If there are larger ships, iterate over each cell
                    for (int y = 0; y < 8; y++)
                    {
                        CellState cs = GetCell(x, y); // If a cell is found of a partially damaged ship
                        if (cs == CellState.Hit)
                        {
                            foreach ((int x, int y) dir in MoveAdj) // Check each adjacent cell
                            {
                                int nx = x + dir.x;
                                int ny = y + dir.y;
                                if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && !hitMask[nx, ny]) // If it is in bounds and has not already been fired at, fire at it
                                {
                                    hitMask[nx, ny] = true; // Fire at the cell
                                    return Text.ComputerMove(nx, ny, GetCell(nx, ny)); // Evaluate the move and return it
                                }
                            }
                        }
                    }
                }
            Mask m = 0L;
            (int x, int y) c = m.Randomize(r); // Generate a random cell
            while (m & hitMask) c = m.Randomize(r); // Keep randomizing it until it has not been fired at already
            hitMask |= m; // Mark the cell as hit
            return Text.ComputerMove(c.x, c.y, GetCell(c.x, c.y)); // Return the evaluation
        }
        public Board ComputerLike(Random r) // Generate a board with the same ship sizes as the player board
        {
            Board b = new Board();
            b.shipMasks = new Mask[5];
            bool RecurseFill(Mask currentShips, int pos) // Recursively look for a place to fill in the next ship
            {
                if (pos >= 5) return true; // If all ships are filled, stop
                int size = shipMasks[pos].Count(); // Find out how big the next ship needs to be
                for (int i = 0; i < 32; i++) // Try 32 times to find a valid space, if none are found then backtrack and try again
                {
                    int x = r.Next(8); // Find a row to put the ship in
                    Dir facing = r.Next(2) == 0 ? Dir.Down : Dir.Right; // Choose whether the ship is horizontal or vertical
                    int y = r.Next(9 - size); // Choose the ships's position within the row
                    if (facing == Dir.Right) // If horizontal, swap the axes
                    {
                        int t = y;
                        y = x;
                        x = t;
                    }
                    Mask m = new Mask(x, y, facing, size - 1); // Create a new mask with the parameters decided on
                    if (m & currentShips) continue; // If the mask overlaps with an existing ship, try again
                    if (RecurseFill(currentShips | m, pos + 1)) // Try to place the remaining ships. If they cannot be placed within the 32 tries, re-place the current ship
                    {
                        b.shipMasks[pos] = m; // Add the ship to the board
                        return true; // Return success
                    }
                }
                return false;
            }
            RecurseFill(0L, 0); // Place the ships
            b.RecomputeMasks(); // Update the cache
            return b;
        }
        public bool IsWon() => !(~hitMask & allShipsMask); // Check if there are no unhit ships
    }
}
