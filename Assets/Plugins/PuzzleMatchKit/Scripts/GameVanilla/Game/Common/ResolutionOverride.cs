// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace GameVanilla.Game.Common
{
	/// <summary>
	/// Helper class to define device-specific resolution settings.
	/// </summary>
	public class ResolutionOverride
	{
		public string name;
		public int width;
		public int height;
		public float zoomLevel;
		public float canvasScalingMatch;
	}
}
