// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Scenes;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying boosters.
    /// </summary>
	public class BuyBoostersPopup : Popup
    {
#pragma warning disable 649
	    [SerializeField]
	    private Sprite horizontalBombSprite;

	    [SerializeField]
	    private Sprite verticalBombSprite;

	    [SerializeField]
	    private Sprite dynamiteSprite;

	    [SerializeField]
	    private Sprite colorBombSprite;

	    [SerializeField]
	    private Text boosterNameText;

	    [SerializeField]
	    private Text boosterDescriptionText;

	    [SerializeField]
	    private Image boosterImage;

	    [SerializeField]
	    private Text boosterAmountText;

	    [SerializeField]
	    private Text boosterCostText;

	    [SerializeField]
	    private Text numCoinsText;

	    [SerializeField]
	    private ParticleSystem coinParticles;
#pragma warning restore 649

	    private BuyBoosterButton buyButton;

	    /// <summary>
	    /// Unity's Awake method.
	    /// </summary>
		protected override void Awake()
		{
			base.Awake();
			Assert.IsNotNull(horizontalBombSprite);
			Assert.IsNotNull(verticalBombSprite);
			Assert.IsNotNull(dynamiteSprite);
			Assert.IsNotNull(colorBombSprite);
			Assert.IsNotNull(boosterNameText);
			Assert.IsNotNull(boosterDescriptionText);
			Assert.IsNotNull(boosterImage);
			Assert.IsNotNull(boosterAmountText);
			Assert.IsNotNull(boosterCostText);
			Assert.IsNotNull(numCoinsText);
			Assert.IsNotNull(coinParticles);
		}

	    /// <summary>
	    /// Unity's Start method.
	    /// </summary>
	    protected override void Start()
	    {
		    base.Start();
		    numCoinsText.text = PlayerPrefs.GetInt("num_coins").ToString();
	    }

	    /// <summary>
	    /// Sets the booster button associated to this popup.
	    /// </summary>
	    /// <param name="button">The booster button.</param>
	    public void SetBooster(BuyBoosterButton button)
		{
			buyButton = button;
			switch (button.boosterType)
			{
				case BoosterType.HorizontalBomb:
					boosterImage.sprite = horizontalBombSprite;
					boosterNameText.text = "Horizontal bomb";
					boosterDescriptionText.text = "Destroys all the blocks in the same row.";
					break;

				case BoosterType.VerticalBomb:
					boosterImage.sprite = verticalBombSprite;
					boosterNameText.text = "Vertical bomb";
					boosterDescriptionText.text = "Destroys all the blocks in the same column.";
					break;

				case BoosterType.Dynamite:
					boosterImage.sprite = dynamiteSprite;
					boosterNameText.text = "Dynamite";
					boosterDescriptionText.text = "Destroys all the adjacent blocks.";
					break;

				case BoosterType.ColorBomb:
					boosterImage.sprite = colorBombSprite;
					boosterNameText.text = "Color bomb";
					boosterDescriptionText.text = "Destroys all the blocks of the same random color.";
					break;
			}

			boosterAmountText.text = PuzzleMatchManager.instance.gameConfig.ingameBoosterAmount[buyButton.boosterType].ToString();
			boosterCostText.text = PuzzleMatchManager.instance.gameConfig.ingameBoosterCost[buyButton.boosterType].ToString();
		}

	    /// <summary>
	    /// Called when the buy button is pressed.
	    /// </summary>
	    public void OnBuyButtonPressed()
	    {
		    var playerPrefsKey = string.Format("num_boosters_{0}", (int)buyButton.boosterType);
		    var numBoosters = PlayerPrefs.GetInt(playerPrefsKey);

		    Close();

		    var gameScene = parentScene as GameScene;
		    if (gameScene != null)
		    {
			    var cost = PuzzleMatchManager.instance.gameConfig.ingameBoosterCost[buyButton.boosterType];
			    var coins = PlayerPrefs.GetInt("num_coins");
			    if (cost > coins)
			    {
				    var scene = parentScene;
				    if (scene != null)
				    {
                    	SoundManager.instance.PlaySound("Button");
					    var button = buyButton;
					    scene.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup",
						    popup =>
						    {
							    popup.onClose.AddListener(
								    () =>
								    {
									    scene.OpenPopup<BuyBoostersPopup>("Popups/BuyBoostersPopup",
										    buyBoostersPopup => { buyBoostersPopup.SetBooster(button); });

								    });
						    });
				    }
			    }
			    else
			    {
				    PuzzleMatchManager.instance.coinsSystem.SpendCoins(cost);
                    coinParticles.Play();
                    SoundManager.instance.PlaySound("CoinsPopButton");
				    numBoosters += PuzzleMatchManager.instance.gameConfig.ingameBoosterAmount[buyButton.boosterType];
				    PlayerPrefs.SetInt(playerPrefsKey, numBoosters);
				    buyButton.UpdateAmount(numBoosters);
			    }
		    }
	    }
	}
}
