using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Inventor : Building
{
  public Inventor(){
    prefab = (GameObject)Resources.Load("Prefabs/Inventor", typeof(GameObject));
    level = 1;
    name = "Inventor";
    position = prefab.transform.position;
    value = 120;
    cost = new int[3] { 300, 500, 50 };
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
  }
}