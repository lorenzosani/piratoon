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
    localStorage = 0;
    built = false;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.StartCoroutine(ProduceGold(controller));
  }

  public override int getLocalStorage(){
    return localStorage;
  }

  public override void resetLocalStorage(){
    localStorage = 0;
    API.SetUserData();
  }

  IEnumerator ProduceGold(ControllerScript controller) 
  {
    while(true)
    {
      if (controller.getUser().getStorageSpaceLeft()[0]-localStorage > 0){
        localStorage += 1;
        API.SetUserData();
      } 
      yield return new WaitForSeconds((float)3600/(level+1)*1);
    }
  }
}