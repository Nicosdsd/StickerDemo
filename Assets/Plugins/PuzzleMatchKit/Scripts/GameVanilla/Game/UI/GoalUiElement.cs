// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages a single goal element within the in-game user interface for goals.
    /// </summary>
    public class GoalUiElement : MonoBehaviour
    {
        public Image image;
        public Text amountText;
        public Image tickImage;
        public Image crossImage;
        public ParticleSystem shineParticles;
        public ParticleSystem starParticles;

        public bool isCompleted { get; private set; }

        private Goal currentGoal;
        private int targetAmount;
        private int currentAmount;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            tickImage.gameObject.SetActive(false);
            crossImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fills this element with the information of the specified goal.
        /// </summary>
        /// <param name="goal">The associated goal.</param>
        public virtual void Fill(Goal goal)
        {
            currentGoal = goal;
            var blockGoal = goal as CollectBlockGoal;
            if (blockGoal != null)
            {
                var specificGoal = blockGoal;
                var texture = Resources.Load("Game/" + specificGoal.blockType) as Texture2D;
                if (texture != null)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100);
                }
                targetAmount = specificGoal.amount;
                amountText.text = targetAmount.ToString();
            }
            else
            {
                var blockerGoal = goal as CollectBlockerGoal;
                if (blockerGoal == null)
                {
                    return;
                }

                var specificGoal = blockerGoal;
                var texture = Resources.Load("Game/" + specificGoal.blockerType) as Texture2D;
                if (texture != null)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100);
                }
                targetAmount = specificGoal.amount;
                amountText.text = targetAmount.ToString();
            }
        }

        /// <summary>
        /// Updates this element based on the current state of the game.
        /// </summary>
        /// <param name="state">The current game state.</param>
        public virtual void UpdateGoal(GameState state)
        {
            if (currentAmount == targetAmount)
            {
                return;
            }

            var newAmount = 0;
            var blockGoal = currentGoal as CollectBlockGoal;
            if (blockGoal != null)
            {
                newAmount = state.collectedBlocks[blockGoal.blockType];
            }
            else
            {
                var blockerGoal = currentGoal as CollectBlockerGoal;
                if (blockerGoal != null)
                {
                    newAmount = state.collectedBlockers[blockerGoal.blockerType];
                }
            }

            if (newAmount == currentAmount)
            {
                return;
            }

            currentAmount = newAmount;
            if (currentAmount >= targetAmount)
            {
                currentAmount = targetAmount;
                SetCompletedTick(true);
                SoundManager.instance.PlaySound("ReachedGoal");
            }
            amountText.text = (targetAmount - currentAmount).ToString();
            if (amountText.gameObject.activeSelf)
            {
                amountText.GetComponent<Animator>().SetTrigger("GoalAchieved");
            }
        }

        /// <summary>
        /// Sets the goal tick as completed/not completed.
        /// </summary>
        /// <param name="completed">True if the completion tick should be shown; false otherwise.</param>
        public void SetCompletedTick(bool completed)
        {
            isCompleted = completed;
            amountText.gameObject.SetActive(false);
            if (completed)
            {
                tickImage.gameObject.SetActive(true);
                image.GetComponent<Animator>().SetTrigger("GoalAchieved");
                tickImage.GetComponent<Animator>().SetTrigger("GoalAchieved");
                shineParticles.Play();
                starParticles.Play();
            }
            else
            {
                crossImage.gameObject.SetActive(true);
            }
        }
    }
}
