using System.Collections;
using UnityEngine;

namespace Fusion.XR.Shared.Locomotion
{
    /**
     * Allow to fade in / fade out a black overlay in front of the user
     * Used in locomotion, loading, ...
     */
    public class Fader : MonoBehaviour
    {
        [Header("Fader description")]
        [Tooltip("The actual renderer to show/hide/fade")]
        public Renderer target;
        public Color fadeColor = Color.black;
        public float startFadeLevel = 0;
        public string colorNameMaterialProperty = "_Color";

        [Header("Blink default durations")]
        public float blinkDurationIn = 0.1f;
        public float blinkDurationSpentIn = 0.1f;
        public float blinkDurationOut = 0.1f;


        // Start is called before the first frame update
        void Start()
        {
            Camera camera = GetComponent<Camera>();
            target.transform.localPosition = new Vector3(0, 0, camera.nearClipPlane + 0.01f);
            SetFade(startFadeLevel);

        }

        [ContextMenu("Blink")]
        private void LaunchBlink()
        {

            StartCoroutine(Blink());
        }

        public IEnumerator Blink(float durationIn = -1, float durationSpentIn = -1, float durationOut = -1)
        {
            if (durationIn == -1) durationIn = blinkDurationIn;
            if (durationSpentIn == -1) durationSpentIn = blinkDurationSpentIn;
            if (durationOut == -1) durationOut = blinkDurationOut;
            yield return FadeIn(durationIn);
            yield return WaitBlinkDuration(durationSpentIn);
            yield return FadeOut(durationOut);
        }

        public IEnumerator WaitBlinkDuration(float durationSpentIn = -1)
        {
            if (durationSpentIn == -1) durationSpentIn = blinkDurationSpentIn;
            yield return new WaitForSeconds(durationSpentIn);
        }

        public void SetFade(float level)
        {
            if (target)
            {
                Color color = fadeColor;
                color.a = level;
                target.material.SetColor(colorNameMaterialProperty, color);
                if (level == 0)
                {
                    target.gameObject.SetActive(false);
                }
                else if (!target.gameObject.activeSelf)
                {
                    target.gameObject.SetActive(true);
                }
            }
        }


        float fadeRequestId = 0;
        public IEnumerator Fade(float duration, float sourceAlpha = 1, float targetAlpha = 0)
        {
            float durationMS = 1000f * duration;
            fadeRequestId = Time.realtimeSinceStartup;
            float currentRequestId = fadeRequestId;
            float elapsed = 0;
            int step = 10;
            float stepS = ((float)step) / 1000f;
            SetFade(sourceAlpha);
            while (elapsed < durationMS && currentRequestId == fadeRequestId)
            {
                float level = Mathf.Lerp(sourceAlpha, targetAlpha, elapsed / durationMS);
                SetFade(level);
                yield return new WaitForSeconds(stepS);
                elapsed += step;
            }
            SetFade(targetAlpha);
        }

        public IEnumerator FadeOut(float duration = -1)
        {
            if (duration == -1) duration = blinkDurationOut;
            yield return Fade(duration, 1, 0);
        }

        public IEnumerator FadeIn(float duration = -1)
        {
            if (duration == -1) duration = blinkDurationIn;
            yield return Fade(duration, 0, 1);
        }

        public void AnimateFadeOut(float duration = -1)
        {
            if (duration == -1) duration = blinkDurationOut;
            if (isActiveAndEnabled) StartCoroutine(FadeOut(duration));
        }
        public void AnimateFadeIn(float duration = -1)
        {
            if (duration == -1) duration = blinkDurationIn;
            if (isActiveAndEnabled) StartCoroutine(FadeIn(duration));
        }
    }
}

