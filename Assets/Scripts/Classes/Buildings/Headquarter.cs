using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// HEADQUARTER: This is a subclass of Building. Buildings in the hideout
// can't be upgraded to a level higher of that of the headquarter.
// The headquarter also contributes to the strength of a village.
//*****************************************************************

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

  public override void startFunctionality(ControllerScript controller) {
    controller.getUser().getVillage().increaseStrength(50 * (level + 1));
  }
}