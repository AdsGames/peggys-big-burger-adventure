using UnityEngine;
using UnityEngine.UI;
public class MenuDialogController : MonoBehaviour
{
  public Button closeButton;
  public Button openButton;
  public GameObject canvas;

  public void Start()
  {
    if (closeButton != null) {
      Button btn = closeButton.GetComponent<Button>();
      btn.onClick.AddListener(HandleClose);
    }

    if (openButton != null) {
      Button btn = openButton.GetComponent<Button>();
      btn.onClick.AddListener(HandleOpen);
    }
  }

  private void HandleClose()
  {
    if (canvas != null) {
      canvas.SetActive(false);
    }
  }

  private void HandleOpen()
  {
    if (canvas != null) {
      canvas.SetActive(true);
    }
  }
}
