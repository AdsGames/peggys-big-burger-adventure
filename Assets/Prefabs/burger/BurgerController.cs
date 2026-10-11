using System;
using System.Collections.Generic;
using UnityEngine;

public class BurgerController : MonoBehaviour
{
  private double offsetY = 0;
  public float translationSpeed = 0.1f;
  public float childOffset = 0.5f;
  public float rotationSpeed = 10;

  private List<float> initialYPostions = new List<float>();

  private void Start()
  {
    foreach (Transform child in transform) {
      initialYPostions.Add(child.transform.position.y);
    }
  }

  private void FixedUpdate()
  {
    // Translate children
    float mappedSin = (float)((Math.Sin(offsetY) + 1.0) / 2.0);

    if (initialYPostions.Count == transform.childCount) {
      for (int i = 0; i < transform.childCount; i++) {
        Transform child = transform.GetChild(i);

        child.transform.position = new Vector3(
          child.transform.position.x,
          initialYPostions[i] + mappedSin * (1 + childOffset * (i + 1)),
          child.transform.position.z
        );
      }
    }

    // Rotate parent
    transform.Rotate(0, rotationSpeed * mappedSin, 0);


    // Increment counter
    this.offsetY += translationSpeed;
  }
}
