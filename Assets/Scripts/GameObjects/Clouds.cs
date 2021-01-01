using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Clouds : MonoBehaviour {

  void Start() {
    // Get the tilemap
    Tilemap tileMap = transform.GetChild(0).GetComponent<Tilemap>();
    // Get the position of my hideout
    // Set the tile at that position to null
    tileMap.SetTile(new Vector3Int(3, 3, 0), null);
  }
}