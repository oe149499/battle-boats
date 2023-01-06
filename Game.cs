namespace BattleBoats
{
    static class Game
    { // Store global data
#pragma warning disable 0649 // I know I have uninitialised variables, I don't care
#pragma warning disable 8618
        public static class Settings
        {
            public enum DisplayMode { Vertical, Horizontal };
            public static DisplayMode displayMode;
            public static bool showGrid;
        }
        public static Board player;
        public static Board computer;
        public static string file;
        public static void Save() // Save to the current file
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(file, FileMode.Create)))
            {
                player.SaveToFile(bw);
                computer.SaveToFile(bw);
            }
        }
        public static void Load() // Load the current file
        {
            using (BinaryReader br = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                player = Board.LoadFromFile(br);
                computer = Board.LoadFromFile(br);
            }
        }
    }
#pragma warning restore 8618
#pragma warning restore 0649
}
