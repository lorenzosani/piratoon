using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Woodcutter : Building
{
  int productionPerHour;

  public Woodcutter(){
    prefab = (GameObject)Resources.Load("Prefabs/Woodcutter", typeof(GameObject));
    level = 0;
    name = "Woodcutter";
    position = prefab.transform.position;
    value = 80;
    cost = new int[3] { 25, 50, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    productionPerHour = level*10;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.InvokeRepeating("produceWood", (float)3600/productionPerHour, (float)3600/productionPerHour);
  }
}