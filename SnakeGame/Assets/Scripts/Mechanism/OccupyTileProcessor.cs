using GameModel;

namespace Mechanism {
    public class OccupyTileProcessor : TileProcessor {
        public OccupyTileProcessor(Tile tile, TileController controller): base(tile, controller) {

        }

        protected override bool CheckInternal(GameState state) {
            return !(state.occupancy[Tile.Y, Tile.X]?.IsEmpty ?? true);
        }
    }
}
