using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Stonecutter : Building
{
  int productionPerHour;

  public Stonecutter(){
    prefab = (GameObject)Resources.Load("Prefabs/Stonecutter", typeof(GameObject));
    level = 0;
    name = "Stonecutter";
    position = prefab.transform.position;
    value = 80;
    cost = new int[3] { 25, 50, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    productionPerHour = level*10;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.InvokeRepeating("produceStone", (float)3600/productionPerHour, (float)3600/productionPerHour);
  }
}