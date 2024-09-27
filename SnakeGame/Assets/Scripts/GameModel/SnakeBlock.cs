using Newtonsoft.Json;
using UnityEngine;

namespace GameModel {
    public class SnakeBlock {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsEmpty { get; set; }

        [JsonIgnore]
        public GameObject Object { get; set; }
        [JsonIgnore]
        public bool IsFruit { get; set; }
    }
}
