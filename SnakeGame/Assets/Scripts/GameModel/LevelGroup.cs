using System.Collections.Generic;

namespace GameModel {
    public sealed class LevelGroup {
        public string Path { get; set; }
        public string Name { get; set; }
        public List<Level> Levels { get; } = new List<Level>();
        public List<string> LevelJsons { get; } = new List<string>();
        public List<string> LevelFileNames { get; } = new List<string>();
    }
}
