using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Storage : Building
{
  public Storage(){
    prefab = (GameObject)Resources.Load("Prefabs/Storage", typeof(GameObject));
    level = 1;
    name = "Storage";
    position = prefab.transform.position;
    value = 30;
    cost = new int[3] { 100, 150, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
  }
}