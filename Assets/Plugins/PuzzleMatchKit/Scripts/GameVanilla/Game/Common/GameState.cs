// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

namespace GameVanilla.Game.Common
{
    /// <summary>
    ///  This class stores the state of a game at a given point in time.
    /// </summary>
    public class GameState
    {
        public int score;
        public Dictionary<BlockType, int> collectedBlocks = new Dictionary<BlockType, int>();
        public Dictionary<BlockerType, int> collectedBlockers = new Dictionary<BlockerType, int>();

        /// <summary>
        /// Resets the game state to its initial state.
        /// </summary>
        public void Reset()
        {
            score = 0;
            collectedBlocks.Clear();
            collectedBlockers.Clear();
            foreach (var value in Enum.GetValues(typeof(BlockType)))
            {
                collectedBlocks.Add((BlockType) value, 0);
            }
            foreach (var value in Enum.GetValues(typeof(BlockerType)))
            {
                collectedBlockers.Add((BlockerType) value, 0);
            }
        }
    }
}
