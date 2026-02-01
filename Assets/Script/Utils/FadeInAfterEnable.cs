using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FadeInAfterEnable : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] UnityEvent onFadeInComplete;
    [SerializeField] UnityEvent onFadeInStart;
    [SerializeField] private float delaySeconds = 2f;
    
    private readonly List<ColorTarget> _targets = new();
    private Coroutine _running;

    private void OnEnable()
    {
        CacheTargetsOnThisGameObjectOnly();

        for (int i = 0; i < _targets.Count; i++)
            _targets[i].SetAlpha(0f);

        _running = StartCoroutine(FadeInCoroutine());
    }

    private void OnDisable()
    {
        if (_running != null)
        {
            StopCoroutine(_running);
            _running = null;
        }
    }

    private IEnumerator FadeInCoroutine()
    {
        onFadeInStart?.Invoke();

        if (fadeDuration <= 0f)
        {
            for (int i = 0; i < _targets.Count; i++)
                _targets[i].RestoreOriginalAlpha();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            for (int i = 0; i < _targets.Count; i++)
                _targets[i].LerpToOriginalAlpha(t);

            yield return null;
        }

        for (int i = 0; i < _targets.Count; i++)
            _targets[i].RestoreOriginalAlpha();

        // invoke event after delay
        if (delaySeconds > 0f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }

        onFadeInComplete?.Invoke();
    }

    private void CacheTargetsOnThisGameObjectOnly()
    {
        _targets.Clear();

        // Avoid duplicates (e.g., TextMeshProUGUI is both TMP_Text and Graphic)
        var seen = new HashSet<Object>();

        // UI: Image / RawImage / (UGUI) Text etc.
        var graphics = GetComponents<Graphic>();
        for (int i = 0; i < graphics.Length; i++)
        {
            var g = graphics[i];
            if (g == null || !seen.Add(g)) continue;
            _targets.Add(ColorTarget.FromGraphic(g));
        }

        // TMP (TextMeshProUGUI, TextMeshPro, etc.)
        var tmps = GetComponents<TMP_Text>();
        for (int i = 0; i < tmps.Length; i++)
        {
            var t = tmps[i];
            if (t == null || !seen.Add(t)) continue;
            _targets.Add(ColorTarget.FromTMP(t));
        }

        // SpriteRenderer
        var srs = GetComponents<SpriteRenderer>();
        for (int i = 0; i < srs.Length; i++)
        {
            var sr = srs[i];
            if (sr == null || !seen.Add(sr)) continue;
            _targets.Add(ColorTarget.FromSpriteRenderer(sr));
        }
    }

    private struct ColorTarget
    {
        private readonly System.Func<Color> _get;
        private readonly System.Action<Color> _set;
        private readonly Color _original;

        public static ColorTarget FromGraphic(Graphic g)
            => new ColorTarget(() => g.color, c => g.color = c, g.color);

        public static ColorTarget FromTMP(TMP_Text t)
            => new ColorTarget(() => t.color, c => t.color = c, t.color);

        public static ColorTarget FromSpriteRenderer(SpriteRenderer sr)
            => new ColorTarget(() => sr.color, c => sr.color = c, sr.color);

        private ColorTarget(System.Func<Color> get, System.Action<Color> set, Color original)
        {
            _get = get;
            _set = set;
            _original = original;
        }

        public void SetAlpha(float alpha)
        {
            Color c = _get();
            c.a = alpha;
            _set(c);
        }

        public void LerpToOriginalAlpha(float t)
        {
            Color c = _get();
            c.a = Mathf.Lerp(0f, _original.a, t);
            _set(c);
        }

        public void RestoreOriginalAlpha()
        {
            Color c = _get();
            c.a = _original.a;
            _set(c);
        }
    }
}
