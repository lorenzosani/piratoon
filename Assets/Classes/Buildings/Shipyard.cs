using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Shipyard : Building
{
  public Shipyard(){
    prefab = (GameObject)Resources.Load("Prefabs/Shipyard", typeof(GameObject));
    level = 1;
    name = "Shipyard";
    position = prefab.transform.position;
    value = 40;
    cost = new int[3] { 100, 150, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
  }
}