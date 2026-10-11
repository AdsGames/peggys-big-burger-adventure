using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Grows a button a little while it's selected, so keyboard and gamepad players can see where they are
/// </summary>
public class SelectedPop : MonoBehaviour, ISelectHandler, IDeselectHandler
{
  public float selectedScale = 1.08f;
  [Tooltip("How quickly the button grows and shrinks. Higher is snappier")]
  public float sharpness = 14f;

  private bool selected;

  public void OnSelect(BaseEventData eventData)
  {
    selected = true;
  }

  public void OnDeselect(BaseEventData eventData)
  {
    selected = false;
  }

  private void OnDisable()
  {
    selected = false;
    transform.localScale = Vector3.one;
  }

  private void Update()
  {
    float target = selected ? selectedScale : 1f;
    float scale = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-sharpness * Time.unscaledDeltaTime));
    transform.localScale = Vector3.one * scale;
  }
}
