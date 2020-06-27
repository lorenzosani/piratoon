using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Defence : Building
{
  public Defence(){
    prefab = (GameObject)Resources.Load("Prefabs/Defence", typeof(GameObject));
    level = 1;
    name = "Defence";
    position = prefab.transform.position;
    value = 60;
    cost = new int[3] { 100, 300, 5 };
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
  }
}