using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Clouds : MonoBehaviour {
  ControllerScript controller;

  // TODO: automatically get from Grid size
  int X_SIZE = 5;
  int Y_SIZE = 5;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    // Get the tilemap
    Tilemap tileMap = transform.GetChild(0).GetComponent<Tilemap>();
    // Get the position of my hideout
    Vector3 position = MapPositions.get(controller.getUser().getVillage().getPosition());
    // Set the tile at that position to null
    int tileX = ((int)position.x / X_SIZE) + 2;
    int tileY = (int)position.y / Y_SIZE;
    // TODO: add watchtower level instead of 1
    removeSurroundingTiles(tileMap, tileX, tileY, 1);
  }

  // TODO: Not necessarily surrounding but remove n tiles in total wherever they are
  // TODO: Do not remove 'border clouds' (probably to be put on a different level)
  void removeSurroundingTiles(Tilemap tileMap, int xPos, int yPos, int n) {
    int tilesToBeRemoved = (6 * n) + 3;
    for (int x = (-n); x <= n; x++) {
      for (int y = (-n); y <= n; y++) {
        int tileX = xPos + x;
        int tileY = yPos + y;
        tileMap.SetTile(new Vector3Int(tileX, tileY, 0), null);
        Debug.Log("X:" + tileX + " Y:" + tileY);
      }
    }
  }
}