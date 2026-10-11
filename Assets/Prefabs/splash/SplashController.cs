using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
  public float splashTime = 3.0f;
  private float timer = 0.0f;

  private void Update()
  {
    timer += Time.deltaTime;

    // Skip splash
    if (Input.GetMouseButtonDown(0) || Input.anyKey || timer > splashTime) {
      SceneManager.LoadScene(sceneName: "Menu");
    }
  }
}
