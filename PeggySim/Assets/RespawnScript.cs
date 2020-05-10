using UnityEngine;

public class RespawnScript : MonoBehaviour
{
  // Start is called before the first frame update
  public float respawnTime = 5.0f;
  public float spawnHeight = 0.0f;
  private float time = 99999.0f;

  public GameObject item;
  private GameObject spawnedItem;
  private BoxCollider boxCollider;

  private void Start()
  {
    boxCollider = GetComponent<BoxCollider>();
    boxCollider.center = new Vector3(
      boxCollider.center.x,
      boxCollider.center.y + spawnHeight,
      boxCollider.center.z
    );
  }

  // Update is called once per frame
  private void Update()
  {
    if (spawnedItem == null) {
      time += Time.deltaTime;
    }

    if (time >= respawnTime) {
      time = 0.0f;
      spawnedItem = Instantiate(item);
      spawnedItem.transform.position = new Vector3(
        transform.position.x,
        transform.position.y + spawnHeight + 1.0f,
        transform.position.z
      );
    }
  }
}
