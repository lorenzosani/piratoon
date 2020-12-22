using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// WATCHTOWER: This is a subclass of Building. The watchtower enables
// the player to unlock new areas of the map
// FUNCTIONALITY NOT YET IMPLEMENTED
//*****************************************************************

public class Watchtower : Building {
  public Watchtower() {
    level = 0;
    name = "Watchtower";
    prefab = (GameObject)getPrefab();
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 400;
    cost = new int[3] {
      400,
      400,
      15
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    built = false;
  }
}