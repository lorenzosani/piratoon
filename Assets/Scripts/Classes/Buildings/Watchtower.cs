using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watchtower : Building {
  public Watchtower() {
    prefab = (GameObject)Resources.Load("Prefabs/Watchtower", typeof(GameObject));
    level = 0;
    name = "Watchtower";
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