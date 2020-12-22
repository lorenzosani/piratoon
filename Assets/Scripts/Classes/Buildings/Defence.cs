using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// DEFENCE: This is a subclass of Building. It represents the defence building,
// which increases the strength of the village when under attack
//*****************************************************************

public class Defence : Building {
  public Defence() {
    level = 0;
    name = "Defence";
    prefab = (GameObject)getPrefab();
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 240;
    cost = new int[3] {
      100,
      150,
      5
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    built = false;
  }

  public override void startFunctionality(ControllerScript controller) {
    controller.getUser().getVillage().increaseStrength(100 * (level + 1));
  }
}