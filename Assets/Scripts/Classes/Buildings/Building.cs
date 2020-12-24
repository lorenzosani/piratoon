using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

//*****************************************************************
// BUILDING: This represents any building in the player's hideout
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class Building {
  protected GameObject prefab; // The Unity object that is shown for this building
  [JsonProperty]
  protected int level; // The level of the building can be increased, never decreased
  [JsonProperty]
  protected string name; // Each building has a specific name
  [JsonProperty]
  protected float[] position; // The position in the hideout where this building is shown
  [JsonProperty]
  protected DateTime completionTime; // How long does it take to build this building
  [JsonProperty]
  protected int value; // How 'valuable' is this building, this is consider in increasing the bounty
  [JsonProperty]
  protected int[] cost; // The price of creating this building
  [JsonProperty]
  protected DateTime lastCollected; // Last time that resources produced by this building have been collected
  [JsonProperty]
  protected bool built; // If false, the building is still under construction

  public void increaseLevel() {
    level += 1;
    API.SetUserData(new string[] {
      "Buildings",
      "Village"
    });
  }

  public void setLevel(int l) {
    level = l;
    API.SetUserData(new string[] {
      "Buildings"
    });
  }

  public int getLevel() {
    return level;
  }

  public string getName() {
    return name;
  }

  public Vector3 getPosition() {
    return new Vector3(position[0], position[1], position[2]);
  }

  public void setPosition(Vector3 v) {
    position[0] = v.x;
    position[1] = v.y;
    position[2] = v.z;
  }

  public GameObject getPrefab() {
    UnityEngine.Object[] allPrefabs = Resources.LoadAll("Prefabs/" + name + "/", typeof(GameObject));
    UnityEngine.Object correctPrefab = null;
    for (int i = 0; i < allPrefabs.Length; i++) {
      correctPrefab = allPrefabs[i];
      if (allPrefabs[i].name == name + level.ToString())break;
    }
    return level == 0 ? (GameObject)allPrefabs[0] : (GameObject)correctPrefab;
  }

  public int getValue() {
    return value * level;
  }

  public void setValue(int v) {
    value = v;
  }

  public int getFutureValue() {
    return value * (level + 1);
  }

  public int[] getBaseCost() {
    return cost;
  }

  public int[] getCost() {
    int[] c = new int[3] {
      cost[0] * (level + 1), cost[1] * (level + 1), cost[2] * (level + 1)
    };
    return c;
  }

  public void setCompletionTime(DateTime t) {
    completionTime = t;
  }

  public DateTime getCompletionTime() {
    return completionTime;
  }

  public void setLastCollected(DateTime l) {
    lastCollected = l;
  }

  public virtual int getLocalStorage() {
    return 0;
  }

  public void setBuilt(bool v) {
    built = v;
    API.SetUserData(new string[] {
      "Buildings"
    });
  }

  public bool isBuilt() {
    return built;
  }

  public virtual void resetLocalStorage() {
    Debug.Log("No virtual storage present for building " + name);
  }

  public virtual void startFunctionality(ControllerScript controller) {
    Debug.Log("No functionality to implement for this building");
  }
}