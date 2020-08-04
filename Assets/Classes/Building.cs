using UnityEngine;
using UnityEngine.UI;
using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Building
{
  protected GameObject prefab;
  [JsonProperty]
  protected int level;
  [JsonProperty]
  protected string name;
  [JsonProperty]
  protected Vector3 position;
  [JsonProperty]
  protected DateTime completionTime;
  [JsonProperty]
  protected int value;
  [JsonProperty]
  protected int[] cost;
  [JsonProperty]
  protected DateTime lastCollected;
  [JsonProperty]
  protected bool built;

  public void increaseLevel()
  {
    level += 1;
    API.SetUserData(new string[]{"Buildings"});
  }

  public void setLevel(int l)
  {
    level = l;
  }

  public int getLevel()
  {
    return level;
  }

  public string getName()
  {
    return name;
  }

  public Vector3 getPosition()
  {
    return position;
  }

  public void setPosition(Vector3 v)
  {
    position = v;
  }

  public GameObject getPrefab()
  {
    return prefab;
  }

  public int getValue()
  {
    return value * level;
  }

  public void setValue(int v)
  {
    value = v;
  }

  public int getFutureValue(){
    return value * (level+1);
  }

  public int[] getBaseCost(){
    return cost;
  }
  
  public int[] getCost()
  {
    int[] c = new int[3] {cost[0]*(level+1), cost[1]*(level+1), cost[2]*(level+1)};
    return c;
  }

  public void setCompletionTime(DateTime t)
  {
    completionTime = t;
  }

  public DateTime getCompletionTime()
  {
    return completionTime;
  }

  public void setLastCollected(DateTime l){
    lastCollected = l;
  }

  public virtual int getLocalStorage(){
    return 0;
  }

  public void setBuilt(bool v){
    built = v;
  }

  public bool isBuilt(){
    return built;
  }

  public virtual void resetLocalStorage(){
    Debug.Log("No virtual storage present for building " + name);
  }

  public virtual void startFunctionality(ControllerScript controller){
    Debug.Log("No functionality to implement for this building");
  }
}