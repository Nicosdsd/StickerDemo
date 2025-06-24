// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The base class used for the tiles in the visual editor.
    /// </summary>
    public class LevelTile
    {
        public BlockerType blockerType;
    }

    /// <summary>
    /// The class used for block tiles.
    /// </summary>
    public class BlockTile : LevelTile
    {
        public BlockType type;
    }

    /// <summary>
    /// The class used for booster tiles.
    /// </summary>
    public class BoosterTile : LevelTile
    {
        public BoosterType type;
    }
}
