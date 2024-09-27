using System.Collections.Generic;

namespace GameModel {
    public class Level {
        public string Name { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public Snake StartSnake { get; set; }
        public List<Tile> SpecialTiles { get; } = new List<Tile>();
        public List<Fruit> Fruits { get; } = new List<Fruit>();
        public bool IsWitness { get; set; }
        public string HelpText { get; set; } = "";
    }
}
