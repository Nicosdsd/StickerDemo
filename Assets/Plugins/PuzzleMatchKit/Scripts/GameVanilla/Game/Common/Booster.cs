// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.Common
{
	/// <summary>
	/// The base class for boosters. Boosters are tile entities that provide a useful gameplay effect when used
	/// (usually exploding a specific set of blocks from the level).
	/// </summary>
	public class Booster : TileEntity
	{
		public BoosterType type;

		/// <summary>
		/// Resolves the booster's effect at the specified tile index of the specified scene.
		/// </summary>
		/// <param name="scene">The scene in which to apply the booster.</param>
		/// <param name="idx">The tile index in which the booster is located.</param>
		/// <returns>The list containing the blocks destroyed by the booster.</returns>
	    public virtual List<GameObject> Resolve(GameScene scene, int idx)
	    {
	        return new List<GameObject>();
	    }

		/// <summary>
		/// Shows the visual effects when the booster effect is resolved.
		/// </summary>
		/// <param name="gamePools">The game pools containing the visual effects.</param>
		/// <param name="scene">The scene in which to apply the booster.</param>
		/// <param name="idx">The tile index in which the booster is located.</param>
	    public virtual void ShowFx(GamePools gamePools, GameScene scene, int idx)
	    {
	    }

		/// <summary>
		/// Adds the tile located at the specified indexes to the specified list of tiles.
		/// </summary>
		/// <param name="tiles">The list of tiles.</param>
		/// <param name="scene">The game scene.</param>
		/// <param name="x">The x index of the tile to add.</param>
		/// <param name="y">The y index of the tile to add.</param>
	    protected virtual void AddTile(List<GameObject> tiles, GameScene scene, int x, int y)
	    {
	        if (x < 0 || x >= scene.level.width ||
	            y < 0 || y >= scene.level.height)
	        {
	            return;
	        }

	        var tileIndex = x + (y * scene.level.width);
	        var tile = scene.tileEntities[tileIndex];
		    if (tile != null)
		    {
			    var block = tile.GetComponent<Block>();
			    if (block != null && (block.type == BlockType.Empty || block.type == BlockType.Collectable))
			    {
				    return;
			    }
			    if (tiles.Contains(tile))
			    {
				    return;
			    }
			    tiles.Add(tile);
		    }
	    }

		/// <summary>
		/// Returns true if the specified coordinates are valid within the specified level and false otherwise.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="x">The x coordinate to check.</param>
		/// <param name="y">The y coordinate to check.</param>
		/// <returns>True if the specified coordinates are valid within the specified level; false otherwise.</returns>
        protected bool IsValidTile(Level level, int x, int y)
        {
            return x >= 0 && x < level.width && y >= 0 && y < level.height;
        }
	}
}
