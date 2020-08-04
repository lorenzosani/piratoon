using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Inn : Building
{
  public Inn(){
    prefab = (GameObject)Resources.Load("Prefabs/Inn", typeof(GameObject));
    level = 0;
    name = "Inn";
    position = prefab.transform.position;
    value = 200;
    cost = new int[3] { 200, 250, 20 };
    completionTime = DateTime.UtcNow.AddSeconds(value/4 * (level+1));
    lastCollected = DateTime.UtcNow;
    built = false;
  }

  public override int getLocalStorage(){
    int frequency = (int) 7200/(level+1);
    int timePassed = (int) (DateTime.UtcNow - lastCollected).TotalSeconds;
    return (int) Math.Floor((double) timePassed/frequency);
  }

  public override void resetLocalStorage(){
    lastCollected = DateTime.UtcNow;
    API.SetUserData(new string[]{"Buildings"});
  }
}