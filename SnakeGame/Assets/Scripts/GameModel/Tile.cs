using System.Collections.Generic;

namespace GameModel {
    public class Tile {
        public int X { get; set; }
        public int Y { get; set; }
        public TileType TileType { get; set; }
        public List<string> TileArgs { get; } = new List<string>();
    }
}
