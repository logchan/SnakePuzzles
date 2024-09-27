using GameModel;
using Mechanism;
using System.Collections.Generic;

public sealed class GameState {
    public Level level;
    public Snake snake;
    public TileProcessor[,] tiles;
    public SnakeBlock[,] occupancy;
    public List<Fruit> fruits;
}