using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// VILLAGE: This represents a village or hideout (terms used interchangeably)
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class Village {
  List<Building> buildings; // A list of all the buildings in the hideout
  [JsonProperty]
  int level; // The level of the hideout increases with its strength
  [JsonProperty]
  int position; // The position of the hideout on the map
  [JsonProperty]
  int strength; // The strength is influenced by the amount and level of buildings in the hideout
  [JsonProperty]
  FloatingObject[] floatingObjects; // The collectable floating objects that appear in the bay
  [JsonProperty]
  Ship[] ships; // The ships that are in this hidoeut

  public Village(int _position) {
    buildings = new List<Building>();
    level = 1;
    position = _position;
    strength = 0;
    floatingObjects = initialiseFloatingObjects(8);
    ships = new Ship[3] { null, null, null };
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

  public Ship[] getShips() {
    return ships;
  }

  public Ship getShip(int i) {
    if (i < 0 || i >= ships.Length)return null;
    return ships[i];
  }

  public void setShip(Ship s, int i) {
    ships[i] = s;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  FloatingObject[] initialiseFloatingObjects(int n) {
    // Declare and initialise variables
    FloatingObject[] objects = new FloatingObject[n];
    int maxMinutes = (int)((TimeSpan.FromHours(16) - TimeSpan.FromHours(0)).TotalMinutes);
    System.Random rnd = new System.Random();

    // Generate n FloatingObjects with randomized values
    for (int i = 0; i < n; i++) {
      TimeSpan time = TimeSpan.FromHours(0).Add(TimeSpan.FromMinutes(rnd.Next(maxMinutes)));
      // Check if other objects have already this position
      int randomPos = rnd.Next(23);
      int[] positionsUsed = objects.Select(o => o != null ? o.getPositionId() : -1).ToArray();
      do { randomPos = rnd.Next(23); } while (positionsUsed.Contains(randomPos));
      FloatingObject obj = new FloatingObject(i, DateTime.Now + time, randomPos);
      objects[i] = obj;
    }
    return objects;
  }

  public void setFloatingObjects(FloatingObject[] flo) {
    floatingObjects = flo;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public int getWatchtowerLevel() {
    Building wt = getBuildingInfo("Watchtower");
    return wt == null ? -1 : wt.getLevel();
  }
}