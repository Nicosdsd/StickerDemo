// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Editor
{
    /// <summary>
    /// The "Game settings" tab in the editor.
    /// </summary>
    public class GameSettingsTab : EditorTab
    {
        private ReorderableList tileScoreOverridesList;
        private TileScoreOverride currentTileScoreOverride;

        private ReorderableList resolutionOverridesList;
        private ResolutionOverride currentResolutionOverride;

        private ReorderableList iapItemsList;
        private IapItem currentIapItem;

        private const string editorPrefsName = "game_settings_path";

        private int selectedTabIndex;

        private Vector2 scrollPos;

        private int newLevel;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="editor">The parent editor.</param>
        public GameSettingsTab(PuzzleMatchKitEditor editor) : base(editor)
        {
            if (EditorPrefs.HasKey(editorPrefsName))
            {
                var path = EditorPrefs.GetString(editorPrefsName);
                parentEditor.gameConfig = LoadJsonFile<GameConfiguration>(path);
                if (parentEditor.gameConfig != null)
                {
                    CreateTileScoreOverridesList();
                    CreateResolutionOverridesList();
                    CreateIapItemsList();
                }
            }

            newLevel = PlayerPrefs.GetInt("next_level");
        }

        /// <summary>
        /// Called when this tab is drawn.
        /// </summary>
        public override void Draw()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.Space(15);

            DrawMenu();

            GUILayout.Space(15);

            var prevSelectedIndex = selectedTabIndex;
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
                new[] {"Game", "Resolutions", "Monetization", "Player preferences"}, GUILayout.Width(500));

            if (selectedTabIndex != prevSelectedIndex)
            {
                GUI.FocusControl(null);
            }

            if (selectedTabIndex == 0)
            {
                DrawGameTab();
            }
            else if (selectedTabIndex == 1)
            {
                DrawResolutionsTab();
            }
            else if (selectedTabIndex == 2)
            {
                DrawMonetizationTab();
            }
            else
            {
                DrawPreferencesTab();
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the game tab.
        /// </summary>
        private void DrawGameTab()
        {
            if (parentEditor.gameConfig != null)
            {
                GUILayout.Space(15);

                DrawScoreSettings();

                GUILayout.Space(15);

                DrawLivesSettings();

                GUILayout.Space(15);

                DrawCoinsSettings();

                GUILayout.Space(15);

                DrawBoosterSettings();

                GUILayout.Space(15);

                DrawPurchasableBoosterSettings();

                GUILayout.Space(15);

                DrawContinueGameSettings();
            }
        }

        /// <summary>
        /// Draws the resolutions tab.
        /// </summary>
        private void DrawResolutionsTab()
        {
            if (parentEditor.gameConfig != null)
            {
                GUILayout.Space(15);

                var oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 180;

                EditorGUILayout.LabelField("Resolution settings", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal(GUILayout.Width(300));
                const string helpText =
                    "The resolution settings of the game. Here you can define device-specific values for the camera zoom and the canvas scaler to be used in the game.";
                EditorGUILayout.HelpBox(helpText, MessageType.Info);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Default zoom level", "The default camera zoom to use in the game screen."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                parentEditor.gameConfig.defaultZoomLevel = EditorGUILayout.FloatField(parentEditor.gameConfig.defaultZoomLevel, GUILayout.Width(70));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Default canvas scaling match", "The default scaling match value to use in the canvas scaler."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                parentEditor.gameConfig.defaultCanvasScalingMatch = EditorGUILayout.FloatField(parentEditor.gameConfig.defaultCanvasScalingMatch, GUILayout.Width(70));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                EditorGUIUtility.labelWidth = oldLabelWidth;

                GUILayout.BeginVertical(GUILayout.Width(350));
                if (resolutionOverridesList != null)
                {
                    resolutionOverridesList.DoLayoutList();
                }

                GUILayout.EndVertical();

                if (currentResolutionOverride != null)
                {
                    DrawResolutionOverride(currentResolutionOverride);
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draws the monetization tab.
        /// </summary>
        private void DrawMonetizationTab()
        {
            if (parentEditor.gameConfig != null)
            {
                GUILayout.Space(15);

                DrawRewardedAdSettings();

                GUILayout.Space(15);

                DrawIapSettings();
            }
        }

        /// <summary>
        /// Draws the preferences tab.
        /// </summary>
        private void DrawPreferencesTab()
        {
            if (parentEditor.gameConfig != null)
            {
                GUILayout.Space(15);

                DrawPreferencesSettings();
            }
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        private void DrawMenu()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New", GUILayout.Width(100), GUILayout.Height(50)))
            {
                parentEditor.gameConfig = new GameConfiguration();
                CreateTileScoreOverridesList();
                CreateResolutionOverridesList();
                CreateIapItemsList();
            }

            if (GUILayout.Button("Open", GUILayout.Width(100), GUILayout.Height(50)))
            {
                var path = EditorUtility.OpenFilePanel("Open game configuration", Application.dataPath + "/PuzzleMatchKit/Resources",
                    "json");
                if (!string.IsNullOrEmpty(path))
                {
                    parentEditor.gameConfig = LoadJsonFile<GameConfiguration>(path);
                    if (parentEditor.gameConfig != null)
                    {
                        CreateTileScoreOverridesList();
                        CreateResolutionOverridesList();
                        CreateIapItemsList();
                        EditorPrefs.SetString(editorPrefsName, path);
                    }
                }
            }

            if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(50)))
            {
                SaveGameConfiguration(Application.dataPath + "/PuzzleMatchKit/Resources");
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the score settings.
        /// </summary>
        private void DrawScoreSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Score", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The default score given to the player when a tile entity explodes. You can change this value for specific tile types in the override list.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Default score", "The default score of tile entities."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.defaultTileScore =
                EditorGUILayout.IntField(gameConfig.defaultTileScore, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(250));
            if (tileScoreOverridesList != null)
            {
                tileScoreOverridesList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentTileScoreOverride != null)
            {
                DrawTileScoreOverride(currentTileScoreOverride);
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the lives settings.
        /// </summary>
        private void DrawLivesSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Lives", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the lives system.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Max lives",
                    "The maximum number of lives that the player can have."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.maxLives = EditorGUILayout.IntField(gameConfig.maxLives, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Time to next life",
                    "The number of seconds that need to pass before the player is given a free life."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.timeToNextLife = EditorGUILayout.IntField(gameConfig.timeToNextLife, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Refill cost",
                    "The cost in coins of refilling the lives of the player up to its maximum number."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.livesRefillCost = EditorGUILayout.IntField(gameConfig.livesRefillCost, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the coins settings.
        /// </summary>
        private void DrawCoinsSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Coins", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the coins system.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Initial coins",
                    "The initial number of coins given to the player."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.initialCoins = EditorGUILayout.IntField(gameConfig.initialCoins, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the booster settings.
        /// </summary>
        private void DrawBoosterSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Boosters", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "Enter the number of matches that need to occur in order for the booster to appear.", MessageType.Info);
            GUILayout.EndHorizontal();

            foreach (var booster in Enum.GetValues(typeof(BoosterType)))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(StringUtils.DisplayCamelCaseString(booster.ToString()));
                var neededMatches = gameConfig.boosterNeededMatches;
                neededMatches[(BoosterType)booster] =
                    EditorGUILayout.IntField(neededMatches[(BoosterType) booster], GUILayout.Width(30));
                GUILayout.EndHorizontal();
            }
        }

         /// <summary>
        /// Draws the specified resolution override.
        /// </summary>
        /// <param name="resolution">The resolution override to draw.</param>
        private void DrawResolutionOverride(ResolutionOverride resolution)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 150;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            resolution.name = EditorGUILayout.TextField(resolution.name, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Width");
            resolution.width = EditorGUILayout.IntField(resolution.width, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Height");
            resolution.height = EditorGUILayout.IntField(resolution.height, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Zoom level");
            resolution.zoomLevel = EditorGUILayout.FloatField(resolution.zoomLevel, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Canvas scaling match");
            resolution.canvasScalingMatch = EditorGUILayout.FloatField(resolution.canvasScalingMatch, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Draws the purchasable booster settings.
        /// </summary>
        private void DrawPurchasableBoosterSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            var oldLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUILayout.LabelField("Purchasable boosters", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the boosters that can be purchased by the player in-game.", MessageType.Info);
            GUILayout.EndHorizontal();

            foreach (var booster in Enum.GetValues(typeof(BoosterType)))
            {
                EditorGUIUtility.labelWidth = 150;

                var boosterText = StringUtils.DisplayCamelCaseString(booster.ToString());
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(string.Format("{0} amount", boosterText));
                var amount = gameConfig.ingameBoosterAmount;
                amount[(BoosterType)booster] =
                    EditorGUILayout.IntField(amount[(BoosterType)booster], GUILayout.Width(30));

                GUILayout.Space(15);

                EditorGUIUtility.labelWidth = 130;

                EditorGUILayout.PrefixLabel(string.Format("{0} cost", boosterText));
                var cost = gameConfig.ingameBoosterCost;
                cost[(BoosterType)booster] =
                    EditorGUILayout.IntField(cost[(BoosterType)booster], GUILayout.Width(70));
                GUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Draws the continue game settings.
        /// </summary>
        private void DrawContinueGameSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Continue game", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the options given to the player when losing a game.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Extra moves",
                    "The number of extra moves that can be purchased by the player when a move-limited game is lost."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.numExtraMoves = EditorGUILayout.IntField(gameConfig.numExtraMoves, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Moves cost",
                    "The cost in coins of the extra moves."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.extraMovesCost = EditorGUILayout.IntField(gameConfig.extraMovesCost, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Extra time",
                    "The number of extra seconds that can be purchased by the player when a time-limited game is lost."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.numExtraTime = EditorGUILayout.IntField(gameConfig.numExtraTime, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Time cost",
                    "The cost in coins of the extra time."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.extraTimeCost = EditorGUILayout.IntField(gameConfig.extraTimeCost, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the rewarded ad settings.
        /// </summary>
		private void DrawRewardedAdSettings()
		{
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Rewarded ad", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The settings for Unity Ads rewarded video.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            const int labelWidth = 190;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Unity Ads Game ID - App Store", "The Unity Ads App Store game ID."),
                GUILayout.Width(labelWidth));
            gameConfig.adsGameIdIos =
                EditorGUILayout.TextField(gameConfig.adsGameIdIos, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Unity Ads Game ID - Google Play", "The Unity Ads Google Play game ID."),
                GUILayout.Width(labelWidth));
            gameConfig.adsGameIdAndroid =
                EditorGUILayout.TextField(gameConfig.adsGameIdAndroid, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Unity Ads - Test Mode", "Indicates if using Unity Ads test mode."),
                GUILayout.Width(labelWidth));
            gameConfig.adsTestMode = EditorGUILayout.Toggle(gameConfig.adsTestMode);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Coins", "The number of coins awarded to the player after watching a rewarded ad."),
                GUILayout.Width(labelWidth));
            gameConfig.rewardedAdCoins =
                EditorGUILayout.IntField(gameConfig.rewardedAdCoins, GUILayout.Width(70));
            GUILayout.EndHorizontal();
		}

        /// <summary>
        /// Draws the in-app purchases settings.
        /// </summary>
        private void DrawIapSettings()
        {
            EditorGUILayout.LabelField("In-app purchases", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The settings of your in-game purchasable items.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(350));
            if (iapItemsList != null)
            {
                iapItemsList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentIapItem != null)
            {
                DrawIapItem(currentIapItem);
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the preferences settings.
        /// </summary>
        private void DrawPreferencesSettings()
        {
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Level", "The current level number."),
                GUILayout.Width(50));
            newLevel =
                EditorGUILayout.IntField(newLevel, GUILayout.Width(50));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Set progress", GUILayout.Width(120), GUILayout.Height(30)))
            {
                PlayerPrefs.SetInt("next_level", newLevel);
            }

            GUILayout.Space(15);

            EditorGUILayout.LabelField("PlayerPrefs", EditorStyles.boldLabel);
            if (GUILayout.Button("Delete PlayerPrefs", GUILayout.Width(120), GUILayout.Height(30)))
            {
                PlayerPrefs.DeleteAll();
            }

            GUILayout.Space(15);

            EditorGUILayout.LabelField("EditorPrefs", EditorStyles.boldLabel);
            if (GUILayout.Button("Delete EditorPrefs", GUILayout.Width(120), GUILayout.Height(30)))
            {
                EditorPrefs.DeleteAll();
            }
        }

        /// <summary>
        /// Creates the list of tile score overrides.
        /// </summary>
        private void CreateTileScoreOverridesList()
        {
            tileScoreOverridesList = SetupReorderableList("Score overrides", parentEditor.gameConfig.tileScoreOverrides,
                ref currentTileScoreOverride, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.ToString());
                },
                (x) =>
                {
                    currentTileScoreOverride = x;
                },
                () =>
                {
                    var menu = new GenericMenu();
                    var goalTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(TileScoreOverride));
                    foreach (var type in goalTypes)
                    {
                        menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.Name)), false,
                            CreateTileScoreOverrideCallback, type);
                    }
                    menu.ShowAsContext();
                },
                (x) =>
                {
                    currentTileScoreOverride = null;
                });
        }

        /// <summary>
        /// Creates the list of resolution overrides.
        /// </summary>
        private void CreateResolutionOverridesList()
        {
            resolutionOverridesList = SetupReorderableList("Resolution overrides", parentEditor.gameConfig.resolutionOverrides,
                ref currentResolutionOverride, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.name);
                },
                (x) =>
                {
                    currentResolutionOverride = x;
                },
                () =>
                {
                    var newOverride = new ResolutionOverride();
                    parentEditor.gameConfig.resolutionOverrides.Add(newOverride);
                },
                (x) =>
                {
                    currentResolutionOverride = null;
                });
        }

        /// <summary>
        /// Creates the list of in-app purchase items.
        /// </summary>
        private void CreateIapItemsList()
        {
            iapItemsList = SetupReorderableList("In-app purchase items", parentEditor.gameConfig.iapItems,
                ref currentIapItem, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 350, EditorGUIUtility.singleLineHeight), x.storeId);
                },
                (x) =>
                {
                    currentIapItem = x;
                },
                () =>
                {
                    var newItem = new IapItem();
                    parentEditor.gameConfig.iapItems.Add(newItem);
                },
                (x) =>
                {
                    currentIapItem = null;
                });
        }

        /// <summary>
        /// Callback to call when a new tile score override is created.
        /// </summary>
        /// <param name="obj">The type of object to create.</param>
        private void CreateTileScoreOverrideCallback(object obj)
        {
            var goal = Activator.CreateInstance((Type) obj) as TileScoreOverride;
            parentEditor.gameConfig.tileScoreOverrides.Add(goal);
        }

        /// <summary>
        /// Draws the specified tile score override item.
        /// </summary>
        /// <param name="tileScoreOverride">The tile score override item to draw.</param>
        private void DrawTileScoreOverride(TileScoreOverride tileScoreOverride)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60;

            tileScoreOverride.Draw();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Draws the specified in-app purchase item.
        /// </summary>
        /// <param name="item">The in-app purchase item to draw.</param>
        private void DrawIapItem(IapItem item)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            item.Draw();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Saves the current game configuration to the specified path.
        /// </summary>
        /// <param name="path">The path to which to save the current game configuration.</param>
        public void SaveGameConfiguration(string path)
        {
#if UNITY_EDITOR
            var fullPath = path + "/game_configuration.json";
            SaveJsonFile(fullPath, parentEditor.gameConfig);
            EditorPrefs.SetString(editorPrefsName, fullPath);
            AssetDatabase.Refresh();
#endif
        }
    }
}
