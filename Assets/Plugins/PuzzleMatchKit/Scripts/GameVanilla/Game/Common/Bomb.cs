// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Core;
using GameVanilla.Game.Scenes;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The bomb is a booster that destroys all the blocks in the same row or column.
    /// </summary>
    public class Bomb : Booster
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }

        public Direction direction;

        protected bool hasMatchingCombo;
        protected bool hasNonMatchingCombo;

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
            switch (direction)
            {
                case Direction.Horizontal:
                {
                    for (var i = 0; i < scene.level.width; i++)
                    {
                        AddTile(tiles, scene, i, y);
                    }

                    var combo = GetCombo(scene, x, y);
                    switch (combo)
                    {
                        case ComboType.Matching:
                            for (var i = 0; i < scene.level.width; i++)
                            {
                                AddTile(tiles, scene, i, y - 1);
                            }
                            for (var i = 0; i < scene.level.width; i++)
                            {
                                AddTile(tiles, scene, i, y + 1);
                            }
                            hasMatchingCombo = true;
                            break;

                        case ComboType.NonMatching:
                            for (var j = 0; j < scene.level.height; j++)
                            {
                                AddTile(tiles, scene, x, j);
                            }
                            hasNonMatchingCombo = true;
                            break;
                    }
                }
                    break;

                case Direction.Vertical:
                {
                    for (var j = 0; j < scene.level.height; j++)
                    {
                        AddTile(tiles, scene, x, j);
                    }

                    var combo = GetCombo(scene, x, y);
                    switch (combo)
                    {
                        case ComboType.Matching:
                            for (var j = 0; j < scene.level.height; j++)
                            {
                                AddTile(tiles, scene, x - 1, j);
                            }
                            for (var j = 0; j < scene.level.height; j++)
                            {
                                AddTile(tiles, scene, x + 1, j);
                            }
                            hasMatchingCombo = true;
                            break;

                        case ComboType.NonMatching:
                            for (var i = 0; i < scene.level.width; i++)
                            {
                                AddTile(tiles, scene, i, y);
                            }
                            hasNonMatchingCombo = true;
                            break;
                    }
                }
                    break;
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
            if (hasMatchingCombo)
            {
                if (direction == Direction.Horizontal)
                {
                    ShowFx(gamePools, scene, x, y - 1, direction);
                    ShowFx(gamePools, scene, x, y, direction);
                    ShowFx(gamePools, scene, x, y + 1, direction);
                }
                else if (direction == Direction.Vertical)
                {
                    ShowFx(gamePools, scene, x - 1, y, direction);
                    ShowFx(gamePools, scene, x, y, direction);
                    ShowFx(gamePools, scene, x + 1, y, direction);
                }
            }
            else if (hasNonMatchingCombo)
            {
                ShowFx(gamePools, scene, x, y, Direction.Horizontal);
                ShowFx(gamePools, scene, x, y, Direction.Vertical);
            }
            else
            {
                ShowFx(gamePools, scene, x, y, direction);
            }
            SoundManager.instance.PlaySound("Bomb");
        }

        /// <summary>
        /// Shows the visual effects when the booster effect is resolved.
        /// </summary>
        /// <param name="gamePools">The game pools containing the visual effects.</param>
        /// <param name="scene">The scene in which to apply the booster.</param>
        /// <param name="x">The x coordinate where the booster is located.</param>
        /// <param name="y">The y coordinate where the booster is located.</param>
        /// <param name="fxDirection">The direction of the visual effects.</param>
        protected virtual void ShowFx(GamePools gamePools, GameScene scene, int x, int y, Direction fxDirection)
        {
            if (!IsValidTile(scene.level, x, y))
            {
                return;
            }

            GameObject particles;
            if (fxDirection == Direction.Horizontal)
            {
                particles = gamePools.horizontalBombParticlesPool.GetObject();
            }
            else
            {
                particles = gamePools.verticalBombParticlesPool.GetObject();
            }
            particles.AddComponent<AutoKillPooled>();
            var tileIndex = x + (y * scene.level.width);
            var hitPos = scene.tilePositions[tileIndex];
            particles.transform.position = hitPos;
            if (fxDirection == Direction.Horizontal)
            {
                var newPos = particles.transform.position;
                newPos.x = scene.levelLocation.position.x;
                particles.transform.position = newPos;
            }
            else
            {
                var newPos = particles.transform.position;
                newPos.y = scene.levelLocation.position.y;
                particles.transform.position = newPos;
            }

            foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
            {
                child.Play();
            }
        }

        /// <summary>
        /// The available bomb combo types.
        /// </summary>
        protected enum ComboType
        {
            None,
            Matching,
            NonMatching
        }

        /// <summary>
        /// Returns the combo type at the specified coordinates.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="x">The x coordinate of the booster.</param>
        /// <param name="y">The y coordinate of the booster.</param>
        /// <returns>The combo type at the specified coordinates.</returns>
        protected ComboType GetCombo(GameScene scene, int x, int y)
        {
            var up = new TileDef(x, y - 1);
            var down = new TileDef(x, y + 1);
            var left = new TileDef(x - 1, y);
            var right = new TileDef(x + 1, y);

            var matchingCombos = 0;
            var nonMatchingCombos = 0;

            if (IsCombo(scene, up.x, up.y))
            {
                if (IsMatchingCombo(scene, up.x, up.y)) matchingCombos += 1;
                else nonMatchingCombos += 1;
            }
            if (IsCombo(scene, down.x, down.y))
            {
                if (IsMatchingCombo(scene, down.x, down.y)) matchingCombos += 1;
                else nonMatchingCombos += 1;
            }
            if (IsCombo(scene, left.x, left.y))
            {
                if (IsMatchingCombo(scene, left.x, left.y)) matchingCombos += 1;
                else nonMatchingCombos += 1;
            }
            if (IsCombo(scene, right.x, right.y))
            {
                if (IsMatchingCombo(scene, right.x, right.y)) matchingCombos += 1;
                else nonMatchingCombos += 1;
            }

            if (nonMatchingCombos > 0)
            {
                return ComboType.NonMatching;
            }
            if (matchingCombos > 0)
            {
                return ComboType.Matching;
            }
            return ComboType.None;
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
                scene.tileEntities[idx].GetComponent<Bomb>() != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if there is a matching combo at the specified coordinates and false otherwise.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="x">The x coordinate of the booster.</param>
        /// <param name="y">The y coordinate of the booster.</param>
        /// <returns>True if there is a matching combo at the specified coordinates; false otherwise.</returns>
        protected bool IsMatchingCombo(GameScene scene, int x, int y)
        {
            var idx = x + (y * scene.level.width);
            if (IsValidTile(scene.level, x, y) &&
                scene.tileEntities[idx] != null &&
                scene.tileEntities[idx].GetComponent<Bomb>() != null)
            {
                var bomb = scene.tileEntities[idx].GetComponent<Bomb>();
                return bomb.direction == direction;
            }
            return false;
        }
    }
}
