using UnityEngine;

public class SpawnerController : MonoBehaviour
{
  public float respawnTime = 30.0f;
  public float spawnHeight = 0.0f;
  private float time = 0.0f;

  public GameObject item = null;
  private GameObject spawnedItem = null;

  private void Start()
  {
    // Spawn immediately
    time = respawnTime;
  }

  private void Update()
  {
    // Increment time if no item
    if (spawnedItem == null) {
      time += Time.deltaTime;
    }

    // Respawn item
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
