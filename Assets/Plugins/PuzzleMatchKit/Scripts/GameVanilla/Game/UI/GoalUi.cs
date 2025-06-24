// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages the in-game user interface for the level goals.
    /// </summary>
    public class GoalUi : MonoBehaviour
    {
        [SerializeField]
        public HorizontalLayoutGroup group;

        /// <summary>
        /// Updates the state of the goals of the current level.
        /// </summary>
        /// <param name="state">The current game state.</param>
        public void UpdateGoals(GameState state)
        {
            foreach (var element in group.GetComponentsInChildren<GoalUiElement>())
            {
                element.UpdateGoal(state);
            }
        }
    }
}
