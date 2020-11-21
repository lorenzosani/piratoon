using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// SHIPYARD: This is a subclass of Building. The shipyard is where ships
// are built, upgraded and repaired.
//*****************************************************************

public class Shipyard : Building {
  public Shipyard() {
    prefab = (GameObject)getPrefab();
    level = 0;
    name = "Shipyard";
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 160;
    cost = new int[3] {
      100,
      150,
      0
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    built = false;
  }
}