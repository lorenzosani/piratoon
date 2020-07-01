using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village
{
  List<Building> buildings;
  int level;
  Vector3 position;
  int strength;

  public Village(Vector3 _position)
  {
    buildings = new List<Building>();
    level = 1;
    position = _position;
    strength = 0;
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

  public int getStrength(){
    return strength + buildings.Count*10;
  }

  public void increaseStrength(int n){
    strength += n;
  }

  public void increaseLevel()
  {
    level += 1;
  }

  public int getLevel()
  {
    return level;
  }

  public void setPosition(Vector3 pos)
  {
    position = pos;
  }

  public Vector3 getPosition()
  {
    return position;
  }
}