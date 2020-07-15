using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Stonecutter : Building
{
  public Stonecutter(ObserverScript observer){
    prefab = (GameObject)Resources.Load("Prefabs/Stonecutter", typeof(GameObject));
    level = 0;
    name = "Stonecutter";
    position = prefab.transform.position;
    value = 80;
    cost = new int[3] { 25, 50, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    localStorage = 0;
    attachObserver(observer);
  }

  public override void startFunctionality(ControllerScript controller){
    controller.StartCoroutine(ProduceStone(controller));
  }

  public override int getLocalStorage(){
    return localStorage;
  }

  public override void resetLocalStorage(){
    localStorage = 0;
    notifyChange();
  }

  IEnumerator ProduceStone(ControllerScript controller) 
  {
    while(true)
    {
      if (controller.getUser().getStorageSpaceLeft()[0]-localStorage > 0){
        localStorage += 1;
        notifyChange();
      } 
      yield return new WaitForSeconds((float)3600/(level+1)*10);
    }
  }
}