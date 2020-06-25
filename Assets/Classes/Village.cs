using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village
{
  List<Building> buildings;
  int level;
  Vector3 position;

  public Village(Vector3 _position)
  {
    buildings = new List<Building>();
    level = 1;
    position = _position;
  }

  public void addBuilding(Building building)
  {
    buildings.Add(building);
  }

  public List<Building> getBuildings()
  {
    return buildings;
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