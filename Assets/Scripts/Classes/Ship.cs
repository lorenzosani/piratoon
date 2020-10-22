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
  [JsonProperty]
  protected float[] currentPosition;
  [JsonProperty]
  protected int[] cost;
  [JsonProperty]
  protected int slot;
  [JsonProperty]
  protected DateTime completionTime;

  public Ship(string _name, Vector3 initialPosition, int _slot) {
    name = _name;
    level = 1;
    condition = 100;
    currentJourney = null;
    currentPosition = new float[3] { initialPosition.x, initialPosition.y, initialPosition.z };
    cost = new int[3] { 100 + 100 * level * 2, 50 + 50 * level * 2, 50 + 50 * level * 2 };
    slot = _slot;
    completionTime = DateTime.UtcNow.AddSeconds(level * slot * 300);
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
    completionTime = DateTime.UtcNow.AddSeconds(level * slot * 300);
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

  public Vector3 getCurrentPosition() {
    return new Vector3(currentPosition[0], currentPosition[1], currentPosition[2]);
  }

  public void setCurrentPosition(Vector3 p) {
    currentPosition = new float[3] { p.x, p.y, p.z };
  }

  public int[] getCost(int slot = 0) {
    int[] costAdjusted = new int[3];
    for (int i = 0; i < 3; i++) {
      costAdjusted[i] = cost[i] + cost[i] * (i * 4);
    }
    return costAdjusted;
  }

  public int getSlot() {
    return slot;
  }

  public DateTime getCompletionTime() {
    return completionTime;
  }
}