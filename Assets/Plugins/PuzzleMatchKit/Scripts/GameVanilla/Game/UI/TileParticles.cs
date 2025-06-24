// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class identifies the particles emitted when a tile entity is destroyed.
    /// </summary>
    public class TileParticles : MonoBehaviour
    {
        public ParticleSystem fragmentParticles;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(fragmentParticles);
        }
    }
}
