using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Woodcutter : Building
{
  public Woodcutter(){
    prefab = (GameObject)Resources.Load("Prefabs/Woodcutter", typeof(GameObject));
    level = 0;
    name = "Woodcutter";
    position = prefab.transform.position;
    value = 80;
    cost = new int[3] { 25, 50, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    localStorage = 0;
    built = false;
  }

  public override void startFunctionality(ControllerScript controller){
    controller.StartCoroutine(ProduceWood(controller));
  }

  public override int getLocalStorage(){
    return localStorage;
  }

  public override void resetLocalStorage(){
    localStorage = 0;
    API.SetUserData(new string[]{"Buildings"});
  }

  IEnumerator ProduceWood(ControllerScript controller) 
  {
    while(true)
    {
      if (controller.getUser().getStorageSpaceLeft()[0]-localStorage > 0){
        localStorage += 1;
        API.SetUserData(new string[]{"Buildings"});
      }
      yield return new WaitForSeconds((float)3600/(level+1)*10);
    }
  }

}