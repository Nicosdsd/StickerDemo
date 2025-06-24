// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GameVanilla.Game.Common
{
    public abstract class TileScoreOverride
    {
        public int score;

#if UNITY_EDITOR
        public virtual void Draw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Score");
            score = EditorGUILayout.IntField(score, GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }
#endif
    }

    public class BlockScore : TileScoreOverride
    {
        public BlockType type;

#if UNITY_EDITOR
        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            type = (BlockType) EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            base.Draw();

            GUILayout.EndVertical();
        }
#endif

        public override string ToString()
        {
            return string.Format("{0}: {1}", type, score);
        }
    }

    public class BlockerScore : TileScoreOverride
    {
        public BlockerType type;

#if UNITY_EDITOR
        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            type = (BlockerType) EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            base.Draw();

            GUILayout.EndVertical();
        }
#endif

        public override string ToString()
        {
            return string.Format("{0}: {1}", type, score);
        }
    }

    public class BoosterScore : TileScoreOverride
    {
        public BoosterType type;

#if UNITY_EDITOR
        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            type = (BoosterType) EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            base.Draw();

            GUILayout.EndVertical();
        }
#endif

        public override string ToString()
        {
            return string.Format("{0}: {1}", type, score);
        }
    }

    /// <summary>
    /// This class stores the general game settings.
    /// </summary>
    [Serializable]
    public class GameConfiguration
    {
        public int maxLives;
        public int timeToNextLife;
        public int livesRefillCost;

        public int initialCoins;

        public int numExtraMoves;
        public int extraMovesCost;
        public int numExtraTime;
        public int extraTimeCost;

        public int defaultTileScore;
        public List<TileScoreOverride> tileScoreOverrides = new List<TileScoreOverride>();

        public Dictionary<BoosterType, int> boosterNeededMatches = new Dictionary<BoosterType, int>();

        public Dictionary<BoosterType, int> ingameBoosterAmount = new Dictionary<BoosterType, int>();
        public Dictionary<BoosterType, int> ingameBoosterCost = new Dictionary<BoosterType, int>();

        public float defaultZoomLevel;
        public float defaultCanvasScalingMatch;
        public List<ResolutionOverride> resolutionOverrides = new List<ResolutionOverride>();

        public string adsGameIdIos;
        public string adsGameIdAndroid;
        public bool adsTestMode;
        public int rewardedAdCoins;
        public List<IapItem> iapItems = new List<IapItem>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameConfiguration()
        {
            boosterNeededMatches.Add(BoosterType.HorizontalBomb, 5);
            boosterNeededMatches.Add(BoosterType.VerticalBomb, 5);
            boosterNeededMatches.Add(BoosterType.Dynamite, 6);
            boosterNeededMatches.Add(BoosterType.ColorBomb, 7);

            ingameBoosterAmount.Add(BoosterType.HorizontalBomb, 1);
            ingameBoosterAmount.Add(BoosterType.VerticalBomb, 1);
            ingameBoosterAmount.Add(BoosterType.Dynamite, 1);
            ingameBoosterAmount.Add(BoosterType.ColorBomb, 1);

            ingameBoosterCost.Add(BoosterType.HorizontalBomb, 100);
            ingameBoosterCost.Add(BoosterType.VerticalBomb, 100);
            ingameBoosterCost.Add(BoosterType.Dynamite, 100);
            ingameBoosterCost.Add(BoosterType.ColorBomb, 100);
        }

        /// <summary>
        /// Returns the score of the specified tile entity.
        /// </summary>
        /// <param name="tile">The tile entity.</param>
        /// <returns>The score of the specified tile entity.</returns>
        public int GetScore(TileEntity tile)
        {
            if (tile is Block)
            {
                var scores = tileScoreOverrides.FindAll(x => x is BlockScore);
                foreach (var score in scores)
                {
                    var blockScore = score as BlockScore;
                    if (blockScore != null && blockScore.type == ((Block) tile).type)
                    {
                        return blockScore.score;
                    }
                }
            }
            else if (tile is Booster)
            {
                var scores = tileScoreOverrides.FindAll(x => x is BoosterScore);
                foreach (var score in scores)
                {
                    var boosterScore = score as BoosterScore;
                    if (boosterScore != null && boosterScore.type == ((Booster) tile).type)
                    {
                        return boosterScore.score;
                    }
                }
            }
            return defaultTileScore;
        }

        /// <summary>
        /// Returns the score of the specified blocker type.
        /// </summary>
        /// <param name="type">The blocker type.</param>
        /// <returns>The score of the specified blocker type.</returns>
        public int GetBlockerScore(BlockerType type)
        {
            var scores = tileScoreOverrides.FindAll(x => x is BlockerScore);
            foreach (var score in scores)
            {
                var blockerScore = score as BlockerScore;
                if (blockerScore != null && blockerScore.type == type)
                {
                    return blockerScore.score;
                }
            }
            return defaultTileScore;
        }

        /// <summary>
        /// Returns the appropriate zoom level for the current device.
        /// </summary>
        /// <returns>The zoom level for this device.</returns>
        public float GetZoomLevel()
        {
            var zoomLevel = defaultZoomLevel;
            foreach (var resolution in resolutionOverrides)
            {
                if (resolution.width == Screen.width && resolution.height == Screen.height)
                {
                    zoomLevel = resolution.zoomLevel;
                    break;
                }
            }
            return zoomLevel;
        }
    }
}
