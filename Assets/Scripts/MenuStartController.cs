using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuStartController : MonoBehaviour
{
  public Button startButton;
  [Tooltip("Shows the longest Peggy so far. Hidden until there is one")]
  public TMP_Text bestText;

  private void Start()
  {
    if (startButton != null) {
      startButton.onClick.AddListener(HandleStart);
    }

    if (bestText != null) {
      int best = BestLength.Value;
      bestText.gameObject.SetActive(best > 0);
      bestText.text = $"LONGEST PEGGY: {best} {(best == 1 ? "SEGMENT" : "SEGMENTS")}";
    }
  }

  private void LateUpdate()
  {
    // Keep something selected so keyboard and gamepad always work, even after a mouse click
    // on empty space. Open dialogs claim focus in their own Update, which runs first.
    EventSystem events = EventSystem.current;
    if (startButton != null && events != null && events.currentSelectedGameObject == null) {
      events.SetSelectedGameObject(startButton.gameObject);
    }
  }

  private void HandleStart()
  {
    SceneManager.LoadScene(sceneName: "AllanLand");
  }
}
