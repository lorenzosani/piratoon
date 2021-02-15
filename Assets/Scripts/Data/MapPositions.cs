using UnityEngine;

public static class MapPositions {
  static Vector3[] positions = new Vector3[34] {
    new Vector3(-33.33f, 13.2f, 0f),
    new Vector3(-28.3f, 16.03f, 0f),
    new Vector3(-22.18f, 10.14f, 0f),
    new Vector3(-29.08f, 4.84f, 0f),
    new Vector3(-33.00f, -2f, 0f),
    new Vector3(-30.3f, -11.83f, 0f),
    new Vector3(-21.43f, -5.45f, 0f),
    new Vector3(-18.87f, -13.69f, 0f),
    new Vector3(-15.81f, -9.22f, 0f),
    new Vector3(-14.15f, -2.01f, 0f),
    new Vector3(-18.87f, 4.95f, 0f),
    new Vector3(-13f, 12.74f, 0f),
    new Vector3(-10.13f, 7.19f, 0f),
    new Vector3(-6.68f, -6.41f, 0f),
    new Vector3(-8.719999f, -13.88f, 0f),
    new Vector3(-2.02f, -12.29f, 0f),
    new Vector3(5.26f, -9.16f, 0f),
    new Vector3(-2.08f, -2.01f, 0f),
    new Vector3(-0.1599998f, 5.46f, 0f),
    new Vector3(-0.3500004f, 10.18f, 0f),
    new Vector3(4.25f, 14.97f, 0f),
    new Vector3(12.04f, 5.26f, 0f),
    new Vector3(6.87f, -0.74f, 0f),
    new Vector3(17.28f, -0.74f, 0f),
    new Vector3(18.24f, -13.25f, 0f),
    new Vector3(28.45f, -13.89f, 0f),
    new Vector3(31.9f, -8.85f, 0f),
    new Vector3(32.92f, -2.72f, 0f),
    new Vector3(22.58f, -3.87f, 0f),
    new Vector3(23.86f, 1.75f, 0f),
    new Vector3(25.26f, 7.62f, 0f),
    new Vector3(32.54f, 10.24f, 0f),
    new Vector3(24.68f, 12.99f, 0f),
    new Vector3(15.74f, 12.29f, 0f)
  };

  public static Vector3 get(int n) {
    return positions[n];
  }

  public static Vector3[] get() {
    return positions;
  }
}