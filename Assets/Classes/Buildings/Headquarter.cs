using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Headquarter : Building
{
  public Headquarter(){
    prefab = (GameObject)Resources.Load("Prefabs/Headquarter", typeof(GameObject));
    level = 1;
    name = "Headquarter";
    position = prefab.transform.position;
    value = 150;
    cost = new int[3] { 400, 550, 25 };
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
  }
}