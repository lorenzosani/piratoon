using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// INVENTOR: This is a subclass of Building. The inventor can produce
// special objects and consumable that can be used by the user. 
// FUNCTIONALITY NOT YET IMPLEMENTED
//*****************************************************************

public class Inventor : Building {
  public Inventor() {
    prefab = (GameObject)Resources.Load("Prefabs/Inventor", typeof(GameObject));
    level = 0;
    name = "Inventor";
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 480;
    cost = new int[3] {
      450,
      450,
      50
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    built = false;
  }
}