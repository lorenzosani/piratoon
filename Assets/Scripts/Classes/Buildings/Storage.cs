using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// STORAGE: This is a subclass of Building. The storage buildings
// stores the resources owned by the user. Higher the level of the storage,
// higher the number of resources that can be stored.
//*****************************************************************

public class Storage : Building {
  int storage;

  public Storage() {
    prefab = (GameObject)Resources.Load("Prefabs/Storage", typeof(GameObject));
    level = 0;
    name = "Storage";
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 120;
    cost = new int[3] {
      100,
      150,
      0
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    storage = 1000 * level;
    built = false;
  }

  public override void startFunctionality(ControllerScript controller) {
    controller.getUser().setStorage(1000 * level);
  }
}