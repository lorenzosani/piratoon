using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defence : Building {
  public Defence() {
    prefab = (GameObject)Resources.Load("Prefabs/Defence", typeof(GameObject));
    level = 0;
    name = "Defence";
    position = prefab.transform.position;
    value = 240;
    cost = new int[3] {
      100,
      300,
      5
    };
    completionTime = DateTime.UtcNow.AddSeconds(value / 4 * (level + 1));
    built = false;
  }

  public override void startFunctionality(ControllerScript controller) {
    controller.getUser().getVillage().increaseStrength(140);
  }
}