// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using FullSerializer;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup that is shown before starting a game.
    /// </summary>
    public class StartGamePopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Sprite enabledStarSprite;

        [SerializeField]
        private Image star1Image;

        [SerializeField]
        private Image star2Image;

        [SerializeField]
        private Image star3Image;

        [SerializeField]
        private GameObject goalPrefab;

        [SerializeField]
        private GameObject goalGroup;

        [SerializeField]
        private Text goalText;

        [SerializeField]
        private Text scoreGoalTitleText;

        [SerializeField]
        private Text scoreGoalAmountText;
#pragma warning restore 649

        private int numLevel;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(levelText);
            Assert.IsNotNull(enabledStarSprite);
            Assert.IsNotNull(star1Image);
            Assert.IsNotNull(star2Image);
            Assert.IsNotNull(star3Image);
            Assert.IsNotNull(goalPrefab);
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(goalText);
            Assert.IsNotNull(scoreGoalTitleText);
            Assert.IsNotNull(scoreGoalAmountText);
        }

        /// <summary>
        /// Loads the level data corresponding to the specified level number.
        /// </summary>
        /// <param name="levelNum">The number of the level to load.</param>
        public void LoadLevelData(int levelNum)
        {
            numLevel = levelNum;

            var serializer = new fsSerializer();
            var level = FileUtils.LoadJsonFile<Level>(serializer, "Levels/" + numLevel);
            levelText.text = "Level " + numLevel;
            var stars = PlayerPrefs.GetInt("level_stars_" + numLevel);
            if (stars == 1)
            {
                star1Image.sprite = enabledStarSprite;
            }
            else if (stars == 2)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
            }
            else if (stars == 3)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
                star3Image.sprite = enabledStarSprite;
            }
            foreach (var goal in level.goals)
            {
                if (goal is CollectBlockGoal || goal is CollectBlockerGoal)
                {
                    var goalObject = Instantiate(goalPrefab);
                    goalObject.transform.SetParent(goalGroup.transform, false);
                    goalObject.GetComponent<GoalUiElement>().Fill(goal);
                }
            }
            var reachScoreGoal = level.goals.Find(x => x is ReachScoreGoal);
            if (reachScoreGoal != null)
            {
                goalText.gameObject.SetActive(false);
                scoreGoalAmountText.text = ((ReachScoreGoal)reachScoreGoal).score.ToString();
            }
            else
            {
                scoreGoalTitleText.gameObject.SetActive(false);
                scoreGoalAmountText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when the play button is pressed.
        /// </summary>
        public void OnPlayButtonPressed()
        {
            PuzzleMatchManager.instance.lastSelectedLevel = numLevel;
            GetComponent<SceneTransition>().PerformTransition();
        }

        /// <summary>
        /// Called when the close button is pressed.
        /// </summary>
        public void OnCloseButtonPressed()
        {
            Close();
        }
    }
}
