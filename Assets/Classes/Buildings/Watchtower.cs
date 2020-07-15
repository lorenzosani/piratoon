using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Watchtower : Building
{
  public Watchtower(ObserverScript observer){
    prefab = (GameObject)Resources.Load("Prefabs/Watchtower", typeof(GameObject));
    level = 0;
    name = "Watchtower";
    position = prefab.transform.position;
    value = 400;
    cost = new int[3] { 400, 400, 15 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    attachObserver(observer);
  }
}