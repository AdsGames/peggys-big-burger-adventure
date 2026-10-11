using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashController : MonoBehaviour
{
  public float splashTime = 3.0f;
  public float fadeTime = 0.5f;
  public RawImage image;
  private float timer = 0.0f;

  private void Update()
  {
    timer += Time.deltaTime;

    if (image) {
      Color colour = image.color;
      colour.a = Mathf.Min(Mathf.Clamp01(timer / fadeTime), Mathf.Clamp01((splashTime - timer) / fadeTime));
      image.color = colour;
    }

    // Skip splash
    if (GameInput.SkipSplash.WasPressedThisFrame() || timer > splashTime) {
      SceneManager.LoadScene(sceneName: "Menu");
    }
  }
}
