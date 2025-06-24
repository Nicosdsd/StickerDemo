// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Core;
using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The color bomb is a booster that destroys all the blocks of a given (random) color in a level.
    /// </summary>
    public class ColorBomb : Booster
    {
		/// <summary>
		/// Resolves the booster's effect at the specified tile index of the specified scene.
		/// </summary>
		/// <param name="scene">The scene in which to apply the booster.</param>
		/// <param name="idx">The tile index in which the booster is located.</param>
		/// <returns>The list containing the blocks destroyed by the booster.</returns>
        public override List<GameObject> Resolve(GameScene scene, int idx)
        {
            var tiles = new List<GameObject>();

            var x = idx % scene.level.width;
            var y = idx / scene.level.width;
            var combo = GetCombo(scene, x, y);
            if (combo)
            {
                for (var j = 0; j < scene.level.height; j++)
                {
                    for (var i = 0; i < scene.level.width; i++)
                    {
                        var tileIndex = i + (j * scene.level.width);
                        var tile = scene.tileEntities[tileIndex];
                        if (tile != null)
                        {
                            var block = tile.GetComponent<Block>();
                            if (block != null &&
                                (block.type == BlockType.Block1 ||
                                 block.type == BlockType.Block2 ||
                                 block.type == BlockType.Block3 ||
                                 block.type == BlockType.Block4 ||
                                 block.type == BlockType.Block5 ||
                                 block.type == BlockType.Block6))
                            {
                                AddTile(tiles, scene, i, j);
                            }
                        }
                    }
                }
                AddTile(tiles, scene, x, y);

                var up = new TileDef(x, y - 1);
                var down = new TileDef(x, y + 1);
                var left = new TileDef(x - 1, y);
                var right = new TileDef(x + 1, y);

                if (IsCombo(scene, up.x, up.y))
                {
                    AddTile(tiles, scene, x, y - 1);
                }

                if (IsCombo(scene, down.x, down.y))
                {
                    AddTile(tiles, scene, x, y + 1);
                }

                if (IsCombo(scene, left.x, left.y))
                {
                    AddTile(tiles, scene, x - 1, y);
                }

                if (IsCombo(scene, right.x, right.y))
                {
                    AddTile(tiles, scene, x + 1, y);
                }
            }
            else
            {
                var randomIdx = Random.Range(0, scene.level.availableColors.Count);
                var randomBlock = scene.level.availableColors[randomIdx];
                var randomType = BlockType.Block1;
                switch (randomBlock)
                {
                    case ColorBlockType.ColorBlock1:
                        randomType = BlockType.Block1;
                        break;
                    case ColorBlockType.ColorBlock2:
                        randomType = BlockType.Block2;
                        break;
                    case ColorBlockType.ColorBlock3:
                        randomType = BlockType.Block3;
                        break;
                    case ColorBlockType.ColorBlock4:
                        randomType = BlockType.Block4;
                        break;
                    case ColorBlockType.ColorBlock5:
                        randomType = BlockType.Block5;
                        break;
                    case ColorBlockType.ColorBlock6:
                        randomType = BlockType.Block6;
                        break;
                }
                for (var j = 0; j < scene.level.height; j++)
                {
                    for (var i = 0; i < scene.level.width; i++)
                    {
                        var tileIndex = i + (j * scene.level.width);
                        var tile = scene.tileEntities[tileIndex];
                        if (tile != null)
                        {
                            var block = tile.GetComponent<Block>();
                            if (block != null && block.type == randomType)
                            {
                                AddTile(tiles, scene, i, j);
                            }
                        }
                    }
                }
                AddTile(tiles, scene, x, y);
            }

            return tiles;
        }

		/// <summary>
		/// Shows the visual effects when the booster effect is resolved.
		/// </summary>
		/// <param name="gamePools">The game pools containing the visual effects.</param>
		/// <param name="scene">The scene in which to apply the booster.</param>
		/// <param name="idx">The tile index in which the booster is located.</param>
        public override void ShowFx(GamePools gamePools, GameScene scene, int idx)
        {
            var x = idx % scene.level.width;
            var y = idx / scene.level.width;
            var particles = gamePools.colorBombParticlesPool.GetObject();
            particles.AddComponent<AutoKillPooled>();
            var tileIndex = x + (y * scene.level.width);
            var hitPos = scene.tilePositions[tileIndex];
            particles.transform.position = hitPos;

            foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
            {
                child.Play();
            }

            SoundManager.instance.PlaySound("ColorBomb");
        }

        /// <summary>
        /// Returns the combo type at the specified coordinates.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="x">The x coordinate of the booster.</param>
        /// <param name="y">The y coordinate of the booster.</param>
        /// <returns>The combo type at the specified coordinates.</returns>
        protected bool GetCombo(GameScene scene, int x, int y)
        {
            var up = new TileDef(x, y - 1);
            var down = new TileDef(x, y + 1);
            var left = new TileDef(x - 1, y);
            var right = new TileDef(x + 1, y);

            if (IsCombo(scene, up.x, up.y) ||
                IsCombo(scene, down.x, down.y) ||
                IsCombo(scene, left.x, left.y) ||
                IsCombo(scene, right.x, right.y))
            {
	            return true;
            }
	        return false;
        }

        /// <summary>
        /// Returns true if there is a combo at the specified coordinates and false otherwise.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="x">The x coordinate of the booster.</param>
        /// <param name="y">The y coordinate of the booster.</param>
        /// <returns>True if there is a combo at the specified coordinates; false otherwise.</returns>
        protected bool IsCombo(GameScene scene, int x, int y)
        {
            var idx = x + (y * scene.level.width);
            if (IsValidTile(scene.level, x, y) &&
                scene.tileEntities[idx] != null &&
                scene.tileEntities[idx].GetComponent<ColorBomb>() != null)
            {
                return true;
            }
            return false;
        }
    }
}
