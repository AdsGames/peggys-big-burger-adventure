using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
public class MenuDialogController : MonoBehaviour
{
  public Button closeButton;
  public Button openButton;
  public GameObject canvas;

  public void Start()
  {
    if (closeButton != null) {
      closeButton.onClick.AddListener(HandleClose);
    }

    if (openButton != null) {
      openButton.onClick.AddListener(HandleOpen);
    }
  }

  private void Update()
  {
    EventSystem events = EventSystem.current;
    if (canvas == null || !canvas.activeSelf || events == null) {
      return;
    }

    // Escape or the gamepad's back button closes the dialog
    if (events.currentInputModule is InputSystemUIInputModule module && module.cancel != null &&
        module.cancel.action != null && module.cancel.action.WasPressedThisFrame()) {
      HandleClose();
      return;
    }

    // Keep keyboard and gamepad focus inside the dialog
    if (events.currentSelectedGameObject == null) {
      Select(closeButton);
    }
  }

  private void HandleClose()
  {
    if (canvas != null) {
      canvas.SetActive(false);
    }
    Select(openButton);
  }

  private void HandleOpen()
  {
    if (canvas != null) {
      canvas.SetActive(true);
    }
    Select(closeButton);
  }

  private static void Select(Button button)
  {
    if (button != null && EventSystem.current != null) {
      EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
  }
}
