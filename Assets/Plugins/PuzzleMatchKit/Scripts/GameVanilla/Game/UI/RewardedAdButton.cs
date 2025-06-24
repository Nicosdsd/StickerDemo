// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Popups;
using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.UI
{
	/// <summary>
	/// The rewarded advertisement button that is present in the level scene.
	/// </summary>
#if UNITY_ADS
	public class RewardedAdButton : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#else
	public class RewardedAdButton : MonoBehaviour
#endif
	{
        private AnimatedButton button;

        private string adUnitId;

        private const string iOSAdUnitId = "Rewarded_iOS";
        private const string androidAdUnitId = "Rewarded_Android";

        private void Awake()
        {
            button = GetComponent<AnimatedButton>();
        }

        private void Start()
        {
            gameObject.SetActive(false);

            adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
                ? iOSAdUnitId
                : androidAdUnitId;

#if UNITY_ADS
            if (Advertisement.isInitialized)
            {
                LoadAd();
            }
#endif
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        public void ShowAd()
        {
#if UNITY_ADS
            Advertisement.Show(adUnitId, this);
#endif
        }

#if UNITY_ADS
        public void LoadAd()
        {
            Advertisement.Load(adUnitId, this);
        }

        public void OnInitializationComplete()
        {
            LoadAd();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            //Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }

        public void OnUnityAdsAdLoaded(string id)
        {
            if (id.Equals(adUnitId))
            {
                gameObject.SetActive(true);
            }
        }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {
            if (adUnitId.Equals(adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
				// Reward the user for watching the ad to completion.
                var gameManager = PuzzleMatchManager.instance;
				var rewardCoins = gameManager.gameConfig.rewardedAdCoins;
				gameManager.coinsSystem.BuyCoins(rewardCoins);
				var levelScene = GameObject.Find("LevelScene");
				if (levelScene != null)
				{
					levelScene.GetComponent<LevelScene>().OpenPopup<AlertPopup>("Popups/AlertPopup", popup =>
					{
						popup.SetTitle("Reward");
						popup.SetText(string.Format("You earned {0} coins!", rewardCoins));
					}, false);
				}

                // Load another ad.
                Advertisement.Load(adUnitId, this);
            }
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            //Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            //Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        }

        public void OnUnityAdsShowStart(string adUnitId) { }
        public void OnUnityAdsShowClick(string adUnitId) { }
#endif
	}
}
