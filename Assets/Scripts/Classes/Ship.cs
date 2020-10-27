using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// SHIP: This represents any ship in the game
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class Ship {
  [JsonProperty]
  protected string name; // Each ship can be given a name, the default is used otherwise
  [JsonProperty]
  protected int level; // The ship can be upgrade to a higher level, never downgraded
  [JsonProperty]
  protected int condition; // The 'health status' of the ship. When 0, the ship can't be used anymore
  [JsonProperty]
  protected ShipJourney currentJourney; // The journey on which the ship is currently on
  [JsonProperty]
  protected float[] currentPosition; // The current position in the map
  [JsonProperty]
  protected int[] cost; // The price to build this ship
  [JsonProperty]
  protected int slot; // A player can have up to 3 ships. This indicates which of the three ships this one is
  [JsonProperty]
  protected DateTime completionTime; // If under construction, upgrade or repair - this indicates when that's complete

  public Ship(string _name, Vector3 initialPosition, int _slot) {
    name = _name;
    level = 1;
    condition = 100;
    currentJourney = null;
    currentPosition = new float[3] { initialPosition.x, initialPosition.y, initialPosition.z };
    slot = _slot;
    cost = new int[3] { 100 + 100 * (slot * 4 * level), 50 + 50 * (slot * 4 * level), 50 + 50 * (slot * 4 * level) };
    completionTime = DateTime.UtcNow.AddSeconds(level * (slot + 1) * 300);
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

  public int[] getCost() {
    return cost;
  }

  public int getSlot() {
    return slot;
  }

  public DateTime getCompletionTime() {
    return completionTime;
  }
}