using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************************************
// INN: This is a subclass of Building. The inn produces an income 
// of the resource 'gold' for the player.
//*****************************************************************

public class Inn : Building {
  public Inn() {
    prefab = (GameObject)Resources.Load("Prefabs/Inn", typeof(GameObject));
    level = 0;
    name = "Inn";
    position = new float[3] { prefab.transform.position.x, prefab.transform.position.y, prefab.transform.position.z };
    value = 200;
    cost = new int[3] {
      250,
      200,
      20
    };
    completionTime = DateTime.UtcNow.AddSeconds(value * (level + 1));
    lastCollected = DateTime.UtcNow;
    built = false;
  }

  public override int getLocalStorage() {
    int frequency = (int)7200 / (level + 1);
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