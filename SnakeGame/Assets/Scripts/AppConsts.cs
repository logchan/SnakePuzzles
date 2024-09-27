using GameModel;
using System.Collections.Generic;

public static class AppConsts {
    public const float TileSize = 1f;
    public const float FieldSize = 3.6f;

    public static List<LevelGroup> BuiltInLevelGroups { get; } = new List<LevelGroup> {
        new LevelGroup { Path = "00", Name = "1-BA"},
        new LevelGroup { Path = "01", Name = "2-FR"},
        new LevelGroup { Path = "02", Name = "3-TR"},
        new LevelGroup { Path = "SP2", Name = "SP-WT"},
        new LevelGroup { Path = "SP1", Name = "SP-BR"},
    };

    public const string VersionText = "Version 1.0";
}
