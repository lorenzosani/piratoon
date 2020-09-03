using UnityEngine;

public static class FloatingObjectsPositions {
  static Vector3[] possiblePositions = new Vector3[23] {
    new Vector3(-3.23f, -6.02f, 0),
    new Vector3(-3.37f, -8.14f, 0),
    new Vector3(0.37f, -5.35f, 0),
    new Vector3(0.66f, -7.45f, 0),
    new Vector3(-4.19f, -9.34f, 0),
    new Vector3(-0.81f, -5.3f, 0),
    new Vector3(3.21f, -7.48f, 0),
    new Vector3(-2.08f, -5.29f, 0),
    new Vector3(3.1f, -6.2f, 0),
    new Vector3(-1.29f, -7.09f, 0),
    new Vector3(-0.47f, -7.84f, 0),
    new Vector3(1.56f, -5.66f, 0),
    new Vector3(0.74f, -6.11f, 0),
    new Vector3(-2.65f, -9.1f, 0),
    new Vector3(-0.75f, -8.75f, 0),
    new Vector3(-4.25f, -7f, 0),
    new Vector3(-1.86f, -8.14f, 0),
    new Vector3(-0.53f, -6.12f, 0),
    new Vector3(1.98f, -7.09f, 0),
    new Vector3(-3.05f, -6.89f, 0),
    new Vector3(-1.37f, -9.67f, 0),
    new Vector3(2.31f, -8.06f, 0),
    new Vector3(-1.96f, -6.11f, 0)
  };

  public static Vector3 get(int n) {
    return possiblePositions[n];
  }

  public static Vector3[] get() {
    return possiblePositions;
  }
}