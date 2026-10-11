using UnityEngine;

public class HeadCollider : MonoBehaviour
{
  private Peggy parent;
  private ButtScript butt;
  public GameObject explodeParticle;
  public GameObject itemParticle;
  public SoundEffectManager soundManager;

  private void Start()
  {
    parent = transform.parent.GetComponent<Peggy>();
    butt = GameObject.FindGameObjectWithTag("Butt").GetComponent<ButtScript>();
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!other || !parent)
    {
      Debug.Log("Other or parent is null!");
      return;
    }

    if (other.CompareTag("Segment") && other.name != "FirstBody")
    {
      parent.breakTimer = 0;
      Effects.PlayOnce(explodeParticle, other.transform.position);
      other.GetComponent<HingeJoint>().connectedBody = null;
      Segment.PruneDetached();
      TripController.Pulse(1.5f);

      if (butt.connected && soundManager)
      {
        soundManager.playLegsLost();
      }
    }

    if (other.CompareTag("Food"))
    {
      Effects.PlayOnce(itemParticle, other.transform.position);
      parent.AddSegment();
      TripController.Pulse();
      Destroy(other.gameObject);

      if (soundManager)
      {
        soundManager.playEat();
      }
      else
      {
        Debug.Log("No sound manager");
      }
    }

    if (other.CompareTag("Butt"))
    {
      if (!other.gameObject.GetComponent<ButtScript>().connected)
      {
        parent.HandleButtCollision(other);
      }
    }
  }
}
