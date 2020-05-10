using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
  public bool complete = false;

  public void AnimationFinished()
  {
    complete = true;
  }

  public void StartAnimation()
  {
    complete = false;
  }
}
