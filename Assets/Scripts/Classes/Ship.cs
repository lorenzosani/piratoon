using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Ship {
  [JsonProperty]
  protected string name;
  [JsonProperty]
  protected int level;
  [JsonProperty]
  protected int condition;
  [JsonProperty]
  protected ShipJourney currentJourney;

  public Ship(string _name) {
    name = _name;
    level = 1;
    condition = 100;
    currentJourney = null;
  }

  public string getName() {
    return name;
  }

  public void changeName(string n) {
    name = n;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public int getLevel() {
    return level;
  }

  public void increaseLevel() {
    level = level + 1;
  }

  public int getCondition() {
    return condition;
  }

  public int getMaxCondition() {
    return level * 100;
  }

  public void restoreCondition() {
    condition = level * 100;
  }

  public bool canStartJourney(ShipJourney j) {
    return condition - j.getShipConditionCost() > 0;
  }

  public void startJourney(ShipJourney j) {
    currentJourney = j;
  }

  public ShipJourney getCurrentJourney() {
    return currentJourney;
  }

  public void finishJourney() {
    condition = condition - currentJourney.getShipConditionCost();
    currentJourney = null;
  }
}