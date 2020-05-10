using UnityEngine;

public class HeadCollider : MonoBehaviour
{
  private Peggy parent;
  public GameObject explodeParticle;
  public GameObject itemParticle;
  public SoundEffectManager soundManager;

  private void Start()
  {
    parent = transform.parent.GetComponent<Peggy>();
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!other || !parent) {
      Debug.Log("Other or parent is null!");
      return;
    }

    if (other.tag == "Segment" && other.name != "FirstBody") {
      GameObject new_particle = Instantiate(explodeParticle);
      new_particle.transform.position = other.transform.position;
      other.GetComponent<HingeJoint>().connectedBody = null;
    }

    if (other.tag == "Food") {
      GameObject new_particle = Instantiate(itemParticle);
      new_particle.transform.position = other.transform.position;
      parent.AddSegment();
      Destroy(other.gameObject);

      if (soundManager) {
        soundManager.playEat();
      } else {
        Debug.Log("No sound manager");
      }
    }
  }
}