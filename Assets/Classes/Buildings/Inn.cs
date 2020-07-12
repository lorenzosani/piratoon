using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Inn : Building
{
  int productionPerHour;
  int localStorage;

  public Inn(){
    prefab = (GameObject)Resources.Load("Prefabs/Inn", typeof(GameObject));
    level = 0;
    name = "Inn";
    position = prefab.transform.position;
    value = 200;
    cost = new int[3] { 200, 250, 20 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    productionPerHour = (level+1)*1;
    localStorage = 0;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.StartCoroutine(ProduceGold(controller));
  }

  public override int getLocalStorage(){
    return localStorage;
  }

  public override void resetLocalStorage(){
    localStorage = 0;
  }

  IEnumerator ProduceGold(ControllerScript controller) 
  {
    while(true)
    {
      if (controller.getUser().getStorageSpaceLeft()[0]-localStorage > 0) localStorage += 1;
      yield return new WaitForSeconds((float)3600/productionPerHour);
    }
  }
}