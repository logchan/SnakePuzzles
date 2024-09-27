﻿using Newtonsoft.Json;
using UnityEngine;

namespace GameModel {
    public class Fruit {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsPersistent { get; set; }

        [JsonIgnore]
        public GameObject Object { get; set; }
        [JsonIgnore]
        public bool IsConsumed { get; set; }
    }
}