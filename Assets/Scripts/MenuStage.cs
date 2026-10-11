using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menu backdrop: a display-only Peggy spinning on a patch of grass while burgers rain down
/// and bounce off her. Built by Peggy > Rebuild UI.
/// </summary>
public class MenuStage : MonoBehaviour
{
  public GameObject peggyPrefab;
  public GameObject burgerPrefab;
  [Tooltip("Spins, carrying the grass and Peggy. Kinematic, so burgers get knocked about properly")]
  public Rigidbody turntable;
  [Tooltip("Peggy stands on top of this")]
  public Collider ground;
  [Tooltip("Degrees per second")]
  public float spinSpeed = 20f;
  [Tooltip("Which way Peggy faces on the turntable, in degrees")]
  public float peggyYaw = 150f;

  [Header("Burger rain")]
  [Tooltip("Seconds between burgers")]
  public float burgerInterval = 0.5f;
  [Tooltip("The oldest burger disappears once there are more than this")]
  public int maxBurgers = 30;
  [Tooltip("Multiplies the prefab's own scale")]
  public float burgerScale = 0.6f;
  [Tooltip("Height above the turntable that burgers drop from")]
  public float dropHeight = 8f;
  [Tooltip("Burgers fall within this distance of the turntable's centre")]
  public float dropRadius = 3f;
  [Range(0f, 1f)]
  public float bounciness = 0.6f;

  private Transform dormant;
  private PhysicsMaterial bouncy;
  private readonly Queue<GameObject> burgers = new Queue<GameObject>();
  private float nextBurger;

  private void Start()
  {
    // Prefabs are instantiated under an inactive parent so their gameplay scripts never wake up
    dormant = new GameObject("Dormant").transform;
    dormant.SetParent(transform, false);
    dormant.gameObject.SetActive(false);

    bouncy = new PhysicsMaterial("Bouncy")
    {
      bounciness = bounciness,
      bounceCombine = PhysicsMaterialCombine.Maximum,
    };

    if (peggyPrefab && turntable && ground)
    {
      SpawnPeggy();
    }
  }

  private void Update()
  {
    if (burgerPrefab && turntable && Time.time >= nextBurger)
    {
      nextBurger = Time.time + burgerInterval;
      SpawnBurger();
    }

    while (burgers.Count > maxBurgers)
    {
      Destroy(burgers.Dequeue());
    }
  }

  private void FixedUpdate()
  {
    if (turntable)
    {
      turntable.MoveRotation(turntable.rotation * Quaternion.Euler(0f, spinSpeed * Time.fixedDeltaTime, 0f));
    }
  }

  private void SpawnPeggy()
  {
    GameObject peggy = SpawnProp(peggyPrefab);
    // Keep her solid colliders so burgers bounce off her, but not the gameplay triggers
    foreach (Collider collider in peggy.GetComponentsInChildren<Collider>(true))
    {
      if (collider.isTrigger)
      {
        DestroyImmediate(collider);
      }
    }

    Transform t = peggy.transform;
    t.SetParent(turntable.transform, false);
    t.localPosition = Vector3.zero;
    t.localRotation = Quaternion.Euler(0f, peggyYaw, 0f);

    // Stand her on the grass, centred on the turntable
    Bounds bounds = RendererBounds(peggy);
    Vector3 centre = turntable.position;
    t.position += new Vector3(centre.x - bounds.center.x, ground.bounds.max.y - bounds.min.y, centre.z - bounds.center.z);
  }

  private void SpawnBurger()
  {
    GameObject burger = SpawnProp(burgerPrefab);
    foreach (Collider collider in burger.GetComponentsInChildren<Collider>(true))
    {
      DestroyImmediate(collider);
    }

    Transform t = burger.transform;
    t.SetParent(transform, false);
    t.localScale *= burgerScale;
    Vector2 offset = Random.insideUnitCircle * dropRadius;
    t.SetPositionAndRotation(turntable.position + new Vector3(offset.x, dropHeight, offset.y), Quaternion.identity);

    // The prefab's trigger is sized for the bobbing animation, so fit a fresh box to the
    // stacked buns and patties while the burger is still upright
    Bounds bounds = RendererBounds(burger);
    BoxCollider box = burger.AddComponent<BoxCollider>();
    Vector3 scale = t.lossyScale;
    box.center = t.InverseTransformPoint(bounds.center);
    box.size = new Vector3(bounds.size.x / scale.x, bounds.size.y / scale.y, bounds.size.z / scale.z);
    box.sharedMaterial = bouncy;

    t.rotation = Random.rotation;
    Rigidbody body = burger.AddComponent<Rigidbody>();
    body.mass = 0.5f;
    body.angularVelocity = Random.insideUnitSphere * 5f;

    burgers.Enqueue(burger);
  }

  /// <summary>
  /// A copy of the prefab with its scripts, joints and rigidbodies removed, left inactive
  /// </summary>
  private GameObject SpawnProp(GameObject prefab)
  {
    GameObject prop = Instantiate(prefab, dormant);
    // Joints and scripts first, since they can require the rigidbodies
    foreach (Joint joint in prop.GetComponentsInChildren<Joint>(true))
    {
      DestroyImmediate(joint);
    }
    foreach (MonoBehaviour behaviour in prop.GetComponentsInChildren<MonoBehaviour>(true))
    {
      DestroyImmediate(behaviour);
    }
    foreach (Rigidbody body in prop.GetComponentsInChildren<Rigidbody>(true))
    {
      DestroyImmediate(body);
    }
    return prop;
  }

  private static Bounds RendererBounds(GameObject root)
  {
    Bounds bounds = new Bounds(root.transform.position, Vector3.zero);
    bool first = true;
    foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
    {
      if (!(renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
      {
        continue;
      }
      if (first)
      {
        bounds = renderer.bounds;
        first = false;
      }
      else
      {
        bounds.Encapsulate(renderer.bounds);
      }
    }
    return bounds;
  }
}
