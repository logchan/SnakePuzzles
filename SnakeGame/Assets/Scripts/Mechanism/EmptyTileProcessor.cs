using GameModel;

namespace Mechanism {
    public class EmptyTileProcessor : TileProcessor {
        public EmptyTileProcessor(Tile tile, TileController controller): base(tile, controller) {

        }

        protected override bool CheckInternal(GameState state) {
            return state.occupancy[Tile.Y, Tile.X] == null || state.occupancy[Tile.Y, Tile.X].IsEmpty;
        }
    }
}
