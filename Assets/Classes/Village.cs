using UnityEngine;
using System.Collections;
using System.Collections.Generic;using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Village : Observable
{
  [JsonProperty]
  List<Building> buildings;
  [JsonProperty]
  int level;
  [JsonProperty]
  Vector3 position;
  [JsonProperty]
  int strength;

  public Village(Vector3 _position, ObserverScript _observer)
  {
    buildings = new List<Building>();
    level = 1;
    position = _position;
    strength = 0;
    attachObserver(_observer);
  }

  public void addBuilding(Building building)
  {
    buildings.Add(building);
  }

  public List<Building> getBuildings()
  {
    return buildings;
  }

  public Building getBuildingInfo(string buildingName){
    foreach(Building building in buildings){
      if (building.getName() == buildingName){
        return building;
      }
    }
    return null;
  }

  public void setBuildingsFromList(List<Building> b){
    buildings = b;
  }

  public int getStrength(){
    return strength + buildings.Count*10;
  }

  public void increaseStrength(int n){
    strength += n;
    notifyChange();
  }

  public void increaseLevel()
  {
    level += 1;
    notifyChange();
  }

  public int getLevel()
  {
    return level;
  }

  public void setPosition(Vector3 pos)
  {
    position = pos;
    notifyChange();
  }

  public Vector3 getPosition()
  {
    return position;
  }
}