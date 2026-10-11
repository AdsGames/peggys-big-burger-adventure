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

    if (other.tag == "Segment" && other.name != "FirstBody")
    {
      parent.breakTimer = 0;
      GameObject new_particle = Instantiate(explodeParticle);
      new_particle.transform.position = other.transform.position;
      other.GetComponent<HingeJoint>().connectedBody = null;
      Segment.PruneDetached();

      if (butt.connected && soundManager)
      {
        soundManager.playLegsLost();
      }
    }

    if (other.tag == "Food")
    {
      GameObject new_particle = Instantiate(itemParticle);
      new_particle.transform.position = other.transform.position;
      parent.AddSegment();
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

    if (other.tag == "Butt")
    {
      if (!other.gameObject.GetComponent<ButtScript>().connected)
      {
        parent.HandleButtCollision(other);
      }
    }
  }
}
