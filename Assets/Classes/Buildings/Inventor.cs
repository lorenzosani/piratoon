using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Inventor : Building
{
  public Inventor(){
    prefab = (GameObject)Resources.Load("Prefabs/Inventor", typeof(GameObject));
    level = 0;
    name = "Inventor";
    position = prefab.transform.position;
    value = 480;
    cost = new int[3] { 300, 500, 50 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
  }
}