using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 🔹 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("⚠ FadeImage is not assigned.");
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);

        // Fade In
        yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);
            if (fadeImage != null)
            {
                fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            }
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(c.r, c.g, c.b, to);
    }
}
