using UnityEngine;

public class SplashAnimationController : MonoBehaviour
{
  private Animation animation;

  private void Start()
  {
    animation = gameObject.GetComponent<Animation>();
    animation.Play("FadeIn");

  }

  private void Update()
  {
  }
}
