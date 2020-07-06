using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Inn : Building
{
  int productionPerHour;

  public Inn(){
    prefab = (GameObject)Resources.Load("Prefabs/Inn", typeof(GameObject));
    level = 0;
    name = "Inn";
    position = prefab.transform.position;
    value = 200;
    cost = new int[3] { 200, 250, 20 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    productionPerHour = level*1;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.InvokeRepeating("produceGold", (float)3600/productionPerHour, (float)3600/productionPerHour);
  }
}