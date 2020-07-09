using UnityEngine;
using UnityEngine.UI;
using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;

public class Building
{
  protected GameObject prefab;
  protected int level;
  protected string name;
  protected Vector3 position;
  protected DateTime completionTime;
  protected int value;
  protected int[] cost;

  public void increaseLevel()
  {
    level += 1;
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

  public GameObject getPrefab()
  {
    return prefab;
  }

  public int getValue()
  {
    return value * level;
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

  public virtual int getLocalStorage(){
    return -1;
  }

  public virtual void resetLocalStorage(){
    Debug.Log("No virtual storage present for building " + name);
  }

  public virtual void startFunctionality(ControllerScript controller){
    Debug.Log("No functionality to implement for this building");
  }
}