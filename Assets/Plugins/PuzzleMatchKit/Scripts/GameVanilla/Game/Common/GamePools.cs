// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

using GameVanilla.Core;

namespace GameVanilla.Game.Common
{
	/// <summary>
	/// This class manages the object pools used in the game. Object pooling is a common game development technique
	/// that helps reduce the amount of garbage generated at runtime when creating and destroying a lot of objects.
	/// We use it for all the tile entities and their associated particle effects in the game.
	///
	/// You can find an official tutorial from Unity about object pooling here:
	/// https://unity3d.com/learn/tutorials/topics/scripting/object-pooling
	/// </summary>
	public class GamePools : MonoBehaviour
	{
		public ObjectPool block1Pool;
		public ObjectPool block2Pool;
		public ObjectPool block3Pool;
		public ObjectPool block4Pool;
		public ObjectPool block5Pool;
		public ObjectPool block6Pool;
		public ObjectPool emptyTilePool;
		public ObjectPool ballPool;
		public ObjectPool stonePool;
		public ObjectPool collectablePool;

		public ObjectPool horizontalBombPool;
		public ObjectPool verticalBombPool;
		public ObjectPool dynamitePool;
		public ObjectPool colorBombPool;

		public ObjectPool icePool;

		public ObjectPool block1ParticlesPool;
		public ObjectPool block2ParticlesPool;
		public ObjectPool block3ParticlesPool;
		public ObjectPool block4ParticlesPool;
		public ObjectPool block5ParticlesPool;
		public ObjectPool block6ParticlesPool;
		public ObjectPool ballParticlesPool;
		public ObjectPool stoneParticlesPool;
		public ObjectPool collectableParticlesPool;
		public ObjectPool boosterSpawnParticlesPool;
		public ObjectPool horizontalBombParticlesPool;
		public ObjectPool verticalBombParticlesPool;
		public ObjectPool dynamiteParticlesPool;
		public ObjectPool colorBombParticlesPool;
		public ObjectPool iceParticlesPool;

    	private readonly List<ObjectPool> blockPools = new List<ObjectPool>();

		/// <summary>
		/// Unity's Awake method.
		/// </summary>
		private void Awake()
		{
			Assert.IsNotNull(block1Pool);
			Assert.IsNotNull(block2Pool);
			Assert.IsNotNull(block3Pool);
			Assert.IsNotNull(block4Pool);
			Assert.IsNotNull(block5Pool);
			Assert.IsNotNull(block6Pool);
			Assert.IsNotNull(emptyTilePool);
			Assert.IsNotNull(ballPool);
			Assert.IsNotNull(stonePool);
			Assert.IsNotNull(collectablePool);
			Assert.IsNotNull(horizontalBombPool);
			Assert.IsNotNull(verticalBombPool);
			Assert.IsNotNull(dynamitePool);
			Assert.IsNotNull(colorBombPool);
			Assert.IsNotNull(icePool);
			Assert.IsNotNull(block1ParticlesPool);
			Assert.IsNotNull(block2ParticlesPool);
			Assert.IsNotNull(block3ParticlesPool);
			Assert.IsNotNull(block4ParticlesPool);
			Assert.IsNotNull(block5ParticlesPool);
			Assert.IsNotNull(block6ParticlesPool);
			Assert.IsNotNull(ballParticlesPool);
			Assert.IsNotNull(stoneParticlesPool);
			Assert.IsNotNull(collectableParticlesPool);
			Assert.IsNotNull(boosterSpawnParticlesPool);
			Assert.IsNotNull(horizontalBombParticlesPool);
			Assert.IsNotNull(verticalBombParticlesPool);
			Assert.IsNotNull(dynamiteParticlesPool);
			Assert.IsNotNull(colorBombParticlesPool);
			Assert.IsNotNull(iceParticlesPool);

			blockPools.Add(block1Pool);
			blockPools.Add(block2Pool);
			blockPools.Add(block3Pool);
			blockPools.Add(block4Pool);
			blockPools.Add(block5Pool);
			blockPools.Add(block6Pool);
		}

		/// <summary>
		/// Returns the tile entity corresponding to the specified level tile.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="tile">The level tile.</param>
		/// <returns>The tile entity corresponding to the specified level tile.</returns>
		public TileEntity GetTileEntity(Level level, LevelTile tile)
		{
			if (tile is BlockTile)
			{
				var blockTile = (BlockTile) tile;
				switch (blockTile.type)
				{
					case BlockType.Block1:
						return block1Pool.GetObject().GetComponent<TileEntity>();

					case BlockType.Block2:
						return block2Pool.GetObject().GetComponent<TileEntity>();

					case BlockType.Block3:
						return block3Pool.GetObject().GetComponent<TileEntity>();

					case BlockType.Block4:
						return block4Pool.GetObject().GetComponent<TileEntity>();

					case BlockType.Block5:
						return block5Pool.GetObject().GetComponent<TileEntity>();

					case BlockType.Block6:
						return block6Pool.GetObject().GetComponent<TileEntity>();

					case BlockType.RandomBlock:
					{
						var randomIdx = Random.Range(0, level.availableColors.Count);
						return blockPools[(int) level.availableColors[randomIdx]].GetObject().GetComponent<TileEntity>();
					}

					case BlockType.Empty:
						return emptyTilePool.GetObject().GetComponent<TileEntity>();

					case BlockType.Ball:
						return ballPool.GetObject().GetComponent<TileEntity>();

					case BlockType.Stone:
						return stonePool.GetObject().GetComponent<TileEntity>();

					case BlockType.Collectable:
						return collectablePool.GetObject().GetComponent<TileEntity>();
				}
			}
			else if (tile is BoosterTile)
			{
				var boosterTile = (BoosterTile) tile;
				switch (boosterTile.type)
				{
					case BoosterType.HorizontalBomb:
						return horizontalBombPool.GetObject().GetComponent<TileEntity>();

					case BoosterType.VerticalBomb:
						return verticalBombPool.GetObject().GetComponent<TileEntity>();

					case BoosterType.Dynamite:
						return dynamitePool.GetObject().GetComponent<TileEntity>();

					case BoosterType.ColorBomb:
						return colorBombPool.GetObject().GetComponent<TileEntity>();
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the particles corresponding to the specified tile entity.
		/// </summary>
		/// <param name="tileEntity">The tile entity.</param>
		/// <returns>The particles corresponding to the specified tile entity.</returns>
        public GameObject GetParticles(TileEntity tileEntity)
        {
            GameObject particles = null;
            var block = tileEntity as Block;
            if (block != null)
            {
                switch (block.type)
                {
                    case BlockType.Block1:
                        particles = block1ParticlesPool.GetObject();
                        break;

                    case BlockType.Block2:
                        particles = block2ParticlesPool.GetObject();
                        break;

                    case BlockType.Block3:
                        particles = block3ParticlesPool.GetObject();
                        break;

                    case BlockType.Block4:
                        particles = block4ParticlesPool.GetObject();
                        break;

                    case BlockType.Block5:
                        particles = block5ParticlesPool.GetObject();
                        break;

                    case BlockType.Block6:
                        particles = block6ParticlesPool.GetObject();
                        break;

                    case BlockType.Ball:
                        particles = ballParticlesPool.GetObject();
                        break;

                    case BlockType.Stone:
                        particles = stoneParticlesPool.GetObject();
                        break;

                    case BlockType.Collectable:
                        particles = collectableParticlesPool.GetObject();
                        break;

                    default:
                        return null;
                }
            }

            return particles;
        }
	}
}
