// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages the score text that is displayed when a tile entity is destroyed.
    /// </summary>
    public class ScoreText : MonoBehaviour
    {
        private RectTransform rect;
        private Text text;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            text = GetComponent<Text>();
        }

        /// <summary>
        /// Unity's OnEnable method.
        /// </summary>
        private void OnEnable()
        {
            var color = text.color;
            color.a = 1.0f;
            text.color = color;
        }

        /// <summary>
        /// Starts the score animation.
        /// </summary>
        public void StartAnimation()
        {
            StartCoroutine(SmoothMove(rect.anchoredPosition + new Vector2(0, 150), 1.0f));
            StartCoroutine(SmoothFade(0.0f, 0.5f));
        }

        /// <summary>
        /// Utility method to smoothly move a UI object to the target position in the specified amount of time.
        /// </summary>
        /// <param name="pos">The target position.</param>
        /// <param name="time">The movement duration in seconds.</param>
        /// <returns>The coroutine.</returns>
        private IEnumerator SmoothMove(Vector3 pos, float time)
        {
            var startPos = rect.anchoredPosition;
            var t = 0.0f;
            while (t <= 1.0f)
            {
                t += Time.deltaTime / time;
                rect.anchoredPosition = Vector2.Lerp(startPos, pos, Mathf.SmoothStep(0, 1, t));
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Utility method to smoothly fade a UI object to the target alpha value in the specified amount of time.
        /// </summary>
        /// <param name="alpha">The target alpha value.</param>
        /// <param name="time">The fade duration in seconds.</param>
        /// <returns>The coroutine.</returns>
        private IEnumerator SmoothFade(float alpha, float time)
        {
            yield return new WaitForSeconds(0.5f);
            var startColor = text.color;
            var t = 0.0f;
            while (t <= 1.0f)
            {
                t += Time.deltaTime / time;
                var newColor = startColor;
                newColor.a = Mathf.Lerp(startColor.a, alpha, Mathf.SmoothStep(0, 1, t));
                text.color = newColor;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
