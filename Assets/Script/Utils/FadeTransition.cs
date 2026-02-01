using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;


public class FadeTransition : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Image fadeImage; 
    [SerializeField] private bool FadeInOnStart = true;

    private void Start()
    {
        if (FadeInOnStart)
        {
            StartCoroutine(FadeIn());
        }
    }

    public void FadeOut(String sceneName)
    {
        StartCoroutine(StartFadeOut(sceneName));
    }

    public IEnumerator FadeIn()
    {
        // Implement fade-in logic here
        Debug.Log("Fade In Started");
        SetImageAlpha(1f); // start fully opaque
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            SetImageAlpha(1f - normalizedTime);
            yield return null;
        }
        SetImageAlpha(0f); // ensure it's fully transparent at the end
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public IEnumerator StartFadeOut(String sceneName)
    {
        // Implement fade-out logic here
        Debug.Log("Fade Out Started");
        yield return FadeOutCoroutine();
        // After fade-out is complete, load the specified scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOutCoroutine()
    {
        // slowly increase the alpha of the image to create a fade-out effect
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            SetImageAlpha(normalizedTime);
            yield return null;
        }
        SetImageAlpha(1f); // ensure it's fully opaque at the end
    }

    private void SetImageAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
