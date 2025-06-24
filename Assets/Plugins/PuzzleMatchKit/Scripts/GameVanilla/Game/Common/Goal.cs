// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace GameVanilla.Game.Common
{
    public abstract class Goal
    {
        public abstract bool IsComplete(GameState state);

#if UNITY_EDITOR

        public abstract void Draw();

#endif
    }

    public class ReachScoreGoal : Goal
    {
        public int score;

        public override bool IsComplete(GameState state)
        {
            return state.score >= score;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Score");
            score = EditorGUILayout.IntField(score, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

#endif

        public override string ToString()
        {
            return "Reach " + score + " points";
        }
    }

    public class CollectBlockGoal : Goal
    {
        public BlockType blockType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedBlocks[blockType] >= amount;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            blockType = (BlockType) EditorGUILayout.EnumPopup(blockType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amount");
            amount = EditorGUILayout.IntField(amount, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

#endif

        public override string ToString()
        {
            return "Collect " + amount + " " + blockType;
        }
    }

    public class CollectBlockerGoal : Goal
    {
        public BlockerType blockerType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedBlockers[blockerType] >= amount;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            blockerType = (BlockerType) EditorGUILayout.EnumPopup(blockerType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amount");
            amount = EditorGUILayout.IntField(amount, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

#endif

        public override string ToString()
        {
            return "Collect " + amount + " " + blockerType;
        }
    }
}
