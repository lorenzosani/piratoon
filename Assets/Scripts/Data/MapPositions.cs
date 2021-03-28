using UnityEngine;

public static class MapPositions {
  static Vector3[] positions = new Vector3[19] {
    new Vector3(-22f, 10f, 0f),
    new Vector3(-19f, 5f, 0f),
    new Vector3(-29f, 6f, 0f),
    new Vector3(-21.5f, -5f, 0f),
    new Vector3(-19f, -14f, 0f),
    new Vector3(-15f, -2f, 0f),
    new Vector3(-9f, 7f, 0f),
    new Vector3(-6f, -6f, 0f),
    new Vector3(-10.5f, -13.5f, 0f),
    new Vector3(-2f, -11f, 0f),
    new Vector3(0f, 5f, 0f),
    new Vector3(7f, -9f, 0f),
    new Vector3(1.5f, 10f, 0f),
    new Vector3(7.5f, 0f, 0f),
    new Vector3(12.5f, 5f, 0f),
    new Vector3(17f, -0.5f, 0f),
    new Vector3(19f, -11.5f, 0f),
    new Vector3(23f, -1f, 0f),
    new Vector3(24.5f, 9.5f, 0f)
  };

  public static Vector3 get(int n) {
    return positions[n];
  }

  public static Vector3[] get() {
    return positions;
  }
}