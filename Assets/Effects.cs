using UnityEngine;

public static class Effects
{
  /// <summary>
  /// Spawns a particle effect that plays once and cleans itself up. Forces every system in it
  /// to stop looping, since the effect pack's explosions have looping children (embers, smoke)
  /// that would otherwise keep them alive forever.
  /// </summary>
  public static GameObject PlayOnce(GameObject prefab, Vector3 position)
  {
    if (!prefab)
      return null;

    GameObject effect = Object.Instantiate(prefab, position, prefab.transform.rotation);
    float longest = 0f;
    foreach (ParticleSystem system in effect.GetComponentsInChildren<ParticleSystem>())
    {
      ParticleSystem.MainModule main = system.main;
      main.loop = false;
      longest = Mathf.Max(longest, main.startDelay.constantMax + main.duration + main.startLifetime.constantMax);
    }

    Object.Destroy(effect, longest + 0.5f);
    return effect;
  }
}
