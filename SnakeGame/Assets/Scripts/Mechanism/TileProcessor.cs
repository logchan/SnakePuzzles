using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mechanism {
    public class TileProcessor {
        public TileController Controller { get; }
        public Tile Tile { get; }
        public bool HasFailure { get; set; }

        public TileProcessor(Tile tile, TileController controller) {
            Tile = tile;
            Controller = controller;
        }

        public virtual bool Check(GameState state) {
            var result = CheckInternal(state);
            HasFailure = !result;
            return result;
        }

        protected virtual bool CheckInternal(GameState state) {
            return true;
        }
    }
}
