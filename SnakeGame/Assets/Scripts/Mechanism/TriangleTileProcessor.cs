using GameModel;
using UnityEngine;

namespace Mechanism {
    public class TriangleTileProcessor : TileProcessor {
        private readonly int _target;

        public TriangleTileProcessor(Tile tile, TileController controller) : base(tile, controller) {
            _target = int.Parse(tile.TileArgs[0]);
        }

        protected override bool CheckInternal(GameState state) {
            return (CheckNeighbor(1, 0, state) + CheckNeighbor(-1, 0, state) + CheckNeighbor(0, 1, state) + CheckNeighbor(0, -1, state)) == _target;
        }

        private int CheckNeighbor(int dx, int dy, GameState state) {
            var x = Tile.X + dx;
            var y = Tile.Y + dy;
            if (x < 0 || x >= state.level.Columns || y < 0 || y >= state.level.Rows) {
                return 0;
            }
            return !(state.occupancy[y, x]?.IsEmpty ?? true) ? 1 : 0;
        }
    }
}
