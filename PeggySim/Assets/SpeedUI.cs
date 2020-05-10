using UnityEngine;
using UnityEngine.UI;

public class SpeedUI : MonoBehaviour
{

  private Text speedText;
  private Color speedColour;

  private void Start()
  {
    speedText = GetComponent<Text>();
    speedColour = new Color();
    speedColour.a = 1;
    speedColour.b = 0;
  }

  private void FixedUpdate()
  {
    if (GameObject.FindGameObjectWithTag("Head").GetComponent<Rigidbody>().velocity.magnitude != 0.0f) {
      speedText.text = GameObject.FindGameObjectWithTag("Head").GetComponent<Rigidbody>().velocity.magnitude.ToString().Substring(0, 4);
      speedColour.r = GameObject.FindGameObjectWithTag("Head").GetComponent<Rigidbody>().velocity.magnitude / 50f;
      speedColour.g = 1 - speedColour.r;
      speedText.color = speedColour;
    }
  }
}
