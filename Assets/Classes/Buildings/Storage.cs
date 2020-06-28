using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Storage : Building
{
  int storage;
  public Storage(){
    prefab = (GameObject)Resources.Load("Prefabs/Storage", typeof(GameObject));
    level = 1;
    name = "Storage";
    position = prefab.transform.position;
    value = 30;
    cost = new int[3] { 100, 150, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
    storage = 1000*level;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.getUser().setStorage(storage);
  }
}