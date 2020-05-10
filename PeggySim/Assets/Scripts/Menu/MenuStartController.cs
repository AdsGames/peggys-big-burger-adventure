using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuStartController : MonoBehaviour
{
  public Button startButton;

  private void Start()
  {
    if (startButton != null) {
      Button btn = startButton.GetComponent<Button>();
      btn.onClick.AddListener(HandleStart);
    }
  }

  private void HandleStart()
  {
    SceneManager.LoadScene(sceneName: "AllanLand");
  }
}
