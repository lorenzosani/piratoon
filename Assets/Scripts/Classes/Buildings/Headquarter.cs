using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Headquarter : Building {
  public Headquarter() {
    prefab = (GameObject)Resources.Load("Prefabs/Headquarter", typeof(GameObject));
    level = 0;
    name = "Headquarter";
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 150;
    cost = new int[3] {
      200,
      250,
      10
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    built = false;
  }
}