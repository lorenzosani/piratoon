using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Watchtower : Building
{
  public Watchtower(){
    prefab = (GameObject)Resources.Load("Prefabs/Watchtower", typeof(GameObject));
    level = 1;
    name = "Watchtower";
    position = prefab.transform.position;
    value = 100;
    cost = new int[3] { 400, 400, 15 };
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
  }
}