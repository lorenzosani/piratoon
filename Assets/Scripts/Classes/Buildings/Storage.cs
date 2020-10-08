using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : Building {
  int storage;

  public Storage() {
    prefab = (GameObject)Resources.Load("Prefabs/Storage", typeof(GameObject));
    level = 0;
    name = "Storage";
    position = prefab.transform.position;
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