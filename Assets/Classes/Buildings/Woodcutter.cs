using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Woodcutter : Building
{
  int productionPerHour;
  int localStorage;

  public Woodcutter(){
    prefab = (GameObject)Resources.Load("Prefabs/Woodcutter", typeof(GameObject));
    level = 0;
    name = "Woodcutter";
    position = prefab.transform.position;
    value = 80;
    cost = new int[3] { 25, 50, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    productionPerHour = (level+1)*10;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.StartCoroutine(ProduceWood(controller));
  }

  public override int getLocalStorage(){
    return localStorage;
  }

  public override void resetLocalStorage(){
    localStorage = 0;
  }

  IEnumerator ProduceWood(ControllerScript controller) 
  {
    while(true)
    {
      if (controller.getUser().getStorageSpaceLeft()[0]-localStorage > 0) localStorage += 1;
      yield return new WaitForSeconds((float)3600/productionPerHour);
    }
  }

}