using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Clouds : MonoBehaviour {
  ControllerScript controller;

  float X_SIZE = 0;
  float Y_SIZE = 0;

  public void startRemoval() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    // Get grid size, which is used to compute which tiles should be hidden
    X_SIZE = GetComponent<Grid>().cellSize.x;
    Y_SIZE = GetComponent<Grid>().cellSize.y;
    // Get the position of the user's hideout
    Vector3 position = MapPositions.get(controller.getUser().getVillage().getPosition());
    // Set the coordinates of the tile above the hideout
    int tileX = (int)Math.Round((position.x / X_SIZE) + 1);
    int tileY = (int)Math.Round((position.y / Y_SIZE));
    removeClouds(
      transform.GetChild(0).GetComponent<Tilemap>(), tileX, tileY,
      controller.getUser().getVillage().getWatchtowerLevel()
    );
  }

  void removeClouds(Tilemap tileMap, int xPos, int yPos, int n) {
    // Handle the case where no watchtower has been built
    if (n == -1 || n == 0) {
      tileMap.SetTile(new Vector3Int(xPos, yPos, 0), null);
      if (tileMap.GetTile(new Vector3Int(xPos + 1, yPos, 0)) != null) {
        tileMap.SetTile(new Vector3Int(xPos + 1, yPos, 0), null);
      } else {
        tileMap.SetTile(new Vector3Int(xPos - 1, yPos, 0), null);
      }
      return;
    }
    // If there's a watchtower do this
    removeTiles(tileMap, xPos, yPos, (6 * n) + 3, n);
  }

  void removeTiles(Tilemap tileMap, int xPos, int yPos, int tilesToBeRemoved, int n) {
    for (int x = (-n); x <= n; x++) {
      for (int y = (-n); y <= n; y++) {
        int tileX = xPos + x;
        int tileY = yPos + y;
        if (tileMap.GetTile(new Vector3Int(tileX, tileY, 0)) != null) {
          tileMap.SetTile(new Vector3Int(tileX, tileY, 0), null);
          tilesToBeRemoved--;
        }
      }
    }
    if (tilesToBeRemoved > 0) {
      removeTiles(tileMap, xPos, yPos, tilesToBeRemoved, n + 1);
    }
  }
}