using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Stonecutter : Building
{
  public Stonecutter(){
    prefab = (GameObject)Resources.Load("Prefabs/Stonecutter", typeof(GameObject));
    level = 0;
    name = "Stonecutter";
    position = prefab.transform.position;
    value = 80;
    cost = new int[3] { 25, 50, 0 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    lastCollected = DateTime.UtcNow;
    built = false;
  }

  public override int getLocalStorage(){
    int frequency = (int) 3600/((level+1)*4);
    int timePassed = (int) (DateTime.UtcNow - lastCollected).TotalSeconds;
    return (int) Math.Floor((double) timePassed/frequency);
  }

  public override void resetLocalStorage(){
    lastCollected = DateTime.UtcNow;
    API.SetUserData(new string[]{"Buildings"});
  }
}