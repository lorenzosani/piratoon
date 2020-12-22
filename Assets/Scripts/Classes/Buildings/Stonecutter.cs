using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// STONECUTTER: This is a subclass of Building. This produces 'stone'.
//*****************************************************************

public class Stonecutter : Building {
  public Stonecutter() {
    level = 0;
    name = "Stonecutter";
    prefab = (GameObject)getPrefab();
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 80;
    cost = new int[3] {
      25,
      50,
      0
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    lastCollected = DateTime.UtcNow;
    built = false;
  }

  public override int getLocalStorage() {
    int frequency = (int)3600 / ((level + 1) * 5);
    int timePassed = (int)(DateTime.UtcNow - lastCollected).TotalSeconds;
    return Math.Min(level * 100, (int)Math.Floor((double)timePassed / frequency));
  }

  public override void resetLocalStorage() {
    lastCollected = DateTime.UtcNow;
    API.SetUserData(new string[] {
      "Buildings"
    });
  }
}