using System.Collections.Generic;
using UnityEngine;

namespace GameModel {
    public class Snake {
        public List<SnakeBlock> Blocks { get; set; }

        public void Destroy() {
            foreach (var block in Blocks) {
                GameObject.Destroy(block.Object);
                block.Object = null;
            }
        }
    }
}
