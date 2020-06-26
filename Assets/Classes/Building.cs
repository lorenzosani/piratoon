using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Building
{
  GameObject prefab;
  int level;
  string name;
  Vector3 position;
  int value;
  int[] cost;
  DateTime completionTime;

  Dictionary<string, int> buildingsValue = new Dictionary<string, int>(){
    { "Inventor", 120 },
    { "Woodcutter", 20 },
    { "Stonecutter", 20 },
    { "Watchtower", 100 },
    { "Headquarter", 150 },
    { "Defence", 60 },
    { "Shipyard", 40 },
    { "Storage", 30 }
  };

  Dictionary<string, int[]> buildingsCost = new Dictionary<string, int[]>(){
    { "Inventor", new int[3] { 300, 500, 50 } },
    { "Woodcutter", new int[3] { 25, 50, 0 } },
    { "Stonecutter", new int[3] { 25, 50, 0 } },
    { "Watchtower", new int[3] { 400, 400, 15 } },
    { "Headquarter", new int[3] { 400, 550, 25 } },
    { "Defence", new int[3] { 100, 300, 5 } },
    { "Shipyard", new int[3] { 100, 150, 0 } },
    { "Storage", new int[3] { 100, 150, 0 } }
  };

  public Building(string _name, Vector3 _position, GameObject _prefab)
  {
    prefab = _prefab;
    level = 1;
    name = _name;
    position = _position;
    value = buildingsValue[_name];
    cost = buildingsCost[_name];
    completionTime = DateTime.UtcNow.AddSeconds(value * level);
  }

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

  public int[] getCost()
  {
    return cost;
  }

  public void setCompletionTime(DateTime t)
  {
    completionTime = t;
  }

  public DateTime getCompletionTime()
  {
    return completionTime;
  }
}