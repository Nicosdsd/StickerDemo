// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages the in-game user interface.
    /// </summary>
    public class GameUi : MonoBehaviour
    {
        public Text limitTitleText;
        public Text limitText;

        public Text scoreText;

        public ProgressBar progressBar;

        public GoalUi goalUi;

#pragma warning disable 649
        [SerializeField]
        private GameObject goalHeadline;

        [SerializeField]
        private GameObject scoreGoalHeadline;

        [SerializeField]
        private Text scoreGoalAmountText;
#pragma warning restore 649

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(goalHeadline);
            Assert.IsNotNull(scoreGoalHeadline);
            Assert.IsNotNull(scoreGoalAmountText);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            goalHeadline.SetActive(false);
            scoreGoalHeadline.SetActive(false);
        }

        /// <summary>
        /// Sets the goals in the goal UI.
        /// </summary>
        /// <param name="goals">The list of goals of the current level.</param>
        /// <param name="itemGoalPrefab">The goal prefab.</param>
        public void SetGoals(List<Goal> goals, GameObject itemGoalPrefab)
        {
            var childrenToRemove = goalUi.group.GetComponentsInChildren<GoalUiElement>().ToList();
            foreach (var child in childrenToRemove)
            {
                Destroy(child.gameObject);
            }

            foreach (var goal in goals)
            {
                if (!(goal is CollectBlockGoal) && !(goal is CollectBlockerGoal))
                {
                    continue;
                }
                var goalObject = Instantiate(itemGoalPrefab);
                goalObject.transform.SetParent(goalUi.group.transform, false);
                goalObject.GetComponent<GoalUiElement>().Fill(goal);
            }
            var reachScoreGoal = goals.Find(x => x is ReachScoreGoal);
            if (reachScoreGoal != null)
            {
                goalHeadline.SetActive(false);
                scoreGoalHeadline.SetActive(true);
                scoreGoalAmountText.text = ((ReachScoreGoal)reachScoreGoal).score.ToString();
            }
            else
            {
                scoreGoalHeadline.SetActive(false);
                goalHeadline.SetActive(true);
            }
        }
    }
}
