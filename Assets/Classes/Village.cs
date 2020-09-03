using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Village {
  List<Building> buildings;
  [JsonProperty]
  int level;
  [JsonProperty]
  int position;
  [JsonProperty]
  int strength;
  [JsonProperty]
  FloatingObject[] floatingObjects;

  public Village(int _position) {
    buildings = new List<Building>();
    level = 1;
    position = _position;
    strength = 0;
    floatingObjects = initialiseFloatingObjects(8);
  }

  public void addBuilding(Building building) {
    buildings.Add(building);
  }

  public List<Building> getBuildings() {
    return buildings;
  }

  public Building getBuildingInfo(string buildingName) {
    foreach (Building building in buildings) {
      if (building.getName() == buildingName) {
        return building;
      }
    }
    return null;
  }

  public void setBuildingsFromList(List<Building> b) {
    buildings = b;
  }

  public int getStrength() {
    return strength + buildings.Count * 10;
  }

  public void increaseStrength(int n) {
    strength += n;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public void increaseLevel() {
    level += 1;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public int getLevel() {
    return level;
  }

  public void setPosition(int pos) {
    position = pos;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public int getPosition() {
    return position;
  }

  public void replaceFloatingObject(int i, FloatingObject obj) {
    floatingObjects[i] = obj;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public FloatingObject[] getFloatingObjects() {
    return floatingObjects;
  }

  FloatingObject[] initialiseFloatingObjects(int n) {
    // Declare and initialise variables
    FloatingObject[] objects = new FloatingObject[n];
    int maxMinutes = (int)((TimeSpan.FromHours(16) - TimeSpan.FromHours(0)).TotalMinutes);
    System.Random rnd = new System.Random();

    // Generate n FloatingObjects with randomized values
    for (int i = 0; i < n; i++) {
      TimeSpan time = TimeSpan.FromHours(0).Add(TimeSpan.FromMinutes(rnd.Next(maxMinutes)));
      FloatingObject obj = new FloatingObject(i, DateTime.Now + time, FloatingObjectsPositions.get(rnd.Next(23)));
      objects[i] = obj;
    }
    return objects;
  }
}