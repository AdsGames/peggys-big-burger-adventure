using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationManager : MonoBehaviour
{
  [System.Serializable]
  public struct Anim
  {
    public Animator animation;
    public string Name;
    public bool PlayOnStart;
  }

  public Anim[] Animations;
  private Dictionary<string, Animator> mdictAnimations = new Dictionary<string, Animator>();

  // Awake is called before the first frame update and before start
  private void Awake()
  {
    foreach (Anim anim in Animations) {
      mdictAnimations.Add(anim.Name, anim.animation);
      if (anim.PlayOnStart) {
        anim.animation.SetTrigger("Start");
      }
    }
  }

  // Update is called once per frame
  private void Update()
  {

  }

  /// <summary>
  /// Assumes animation is started with "Start" trigger
  /// </summary>
  /// <param name="pstrAnimationName"></param>
  public void StartAnimation(string pstrAnimationName, int pintAnimationIndex = 0)
  {
    if (mdictAnimations[pstrAnimationName] != null) {
      mdictAnimations[pstrAnimationName].SetTrigger("Start" + pintAnimationIndex);
    }
  }

  /// <summary>
  /// Assumes animation is started with "Start" trigger
  /// </summary>
  /// <param name="pstrAnimationName"></param>
  /// <returns></returns>
  public IEnumerator StartAndWaitForAnimation(string pstrAnimationName, int pintAnimationIndex = 0)
  {
    if (mdictAnimations[pstrAnimationName] != null) {
      mdictAnimations[pstrAnimationName].SetTrigger("Start" + pintAnimationIndex);
    }


    AnimationEvents animEvents = mdictAnimations[pstrAnimationName].gameObject.GetComponent<AnimationEvents>();
    while (!animEvents.complete) {
      yield return null;
    }
  }

  /// <summary>
  /// Assumes animation is started with "Start" trigger
  /// </summary>
  /// <param name="puniCallbackFunction"></param>
  /// <param name="pstrAnimationName"></param>
  public void RunFunctionAfterAnimation(UnityAction puniCallbackFunction, string pstrAnimationName)
  {
    StartCoroutine(CoroutineAnimationFunction(puniCallbackFunction, pstrAnimationName));
  }

  /// <summary>
  /// Run by RunFionctAfterAnimation
  /// Assumes animation is started with "Start" trigger
  /// </summary>
  /// <param name="puniCallbackFunction"></param>
  /// <param name="pstrAnimationName"></param>
  /// <returns></returns>
  private IEnumerator CoroutineAnimationFunction(UnityAction puniCallbackFunction, string pstrAnimationName, int pintAnimationIndex = 0)
  {
    if (mdictAnimations[pstrAnimationName] != null) {
      mdictAnimations[pstrAnimationName].SetTrigger("Start" + pintAnimationIndex);
    }

    AnimationEvents animEvents = mdictAnimations[pstrAnimationName].gameObject.GetComponent<AnimationEvents>();
    while (!animEvents.complete) {
      yield return null;
    }
    puniCallbackFunction.Invoke();
  }

  /// <summary>
  /// Add an animation at runtime
  /// Used to add player animations as they are created at runtime
  /// </summary>
  /// <param name="pstrAnimationName"></param>
  /// <param name="puniAnimator"></param>
  public void AddAnimation(string pstrAnimationName, Animator puniAnimator)
  {
    if (!mdictAnimations.ContainsKey(pstrAnimationName)) {
      mdictAnimations.Add(pstrAnimationName, puniAnimator);
    }
  }
}
