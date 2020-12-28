using System;
using System.Collections.Generic;
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
  [JsonProperty]
  protected bool built; // True if the ship has been constructed and already delivered

  public Ship(string _name, Vector3 initialPosition, int _slot) {
    name = _name;
    level = 0;
    condition = 100;
    currentJourney = null;
    currentPosition = new float[3] { initialPosition.x, initialPosition.y, initialPosition.z };
    slot = _slot;
    cost = new int[3] { computeCost(100), computeCost(50), computeCost(50) };
    completionTime = DateTime.UtcNow.AddSeconds(level * (slot + 1) * 300);
    built = false;
  }

  int computeCost(int weight) {
    return weight + (weight * slot * 2) + (weight * level);
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
    completionTime = DateTime.UtcNow.AddSeconds(level * (slot + 1) * 300);
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public int getCondition() {
    return condition;
  }

  public int getMaxCondition() {
    return level * 100;
  }

  public void restoreCondition() {
    condition = level * 100;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public bool canStartJourney(ShipJourney j) {
    return condition - j.getShipConditionCost() > 0;
  }

  public void startJourney(ShipJourney j) {
    currentJourney = j;
    API.SetUserData(new string[] {
      "Village"
    });
  }

  public void finishJourney(Vector3 arrivalPos) {
    if (currentJourney != null) {
      condition = condition - currentJourney.getShipConditionCost();
      currentPosition = new float[3] { arrivalPos.x, arrivalPos.y, arrivalPos.z };
      currentJourney = null;
      API.SetUserData(new string[] {
        "Village"
      });
    }
  }

  public ShipJourney getCurrentJourney() {
    return currentJourney;
  }

  public Vector3 getCurrentPosition() {
    if (currentJourney == null || currentJourney.getDuration() == 0 || currentJourney.getPath().Count == 0) {
      // This returns the position where the ship is waiting when not navigating
      return new Vector3(currentPosition[0], currentPosition[1], currentPosition[2]);
    } else {
      // This returns the updated position as the ship navigates
      // First, it gets the current percentage of the path already went through (in seconds)
      int timeNavigating = (int)(DateTime.Now - currentJourney.getStartTime()).TotalSeconds;
      Debug.Log("Time navigating: " + timeNavigating);
      int totalDuration = currentJourney.getDuration();
      Debug.Log("Total duration: " + totalDuration);
      int percentageNavigated = (int)timeNavigating * 100 / totalDuration;
      Debug.Log("Percentage navigated: " + percentageNavigated);
      // Then, it checks on the actual path where that percentage corresponds
      List<Vector3> path = currentJourney.getPath();
      // Compute path length
      double pathLength = 0;
      for (int i = 1; i < path.Count; i++) {
        pathLength += Vector3.Distance(path[i - 1], path[i]);
      }
      // Compute what's the distance navigated already based on the time percentage above
      double distanceNavigated = percentageNavigated * pathLength / 100;
      if (distanceNavigated > pathLength)distanceNavigated = pathLength;
      // Compute the position on the map that corresponds to that distance navigated
      double currentDistance = 0;
      double percentageLeftToPosition = 0;
      int index = 0;
      for (int i = 1; i < path.Count; i++) {
        currentDistance += Vector3.Distance(path[i - 1], path[i]);
        if (currentDistance >= distanceNavigated) {
          // Current position is in between path[i-1] and path[i]
          // previousDistance = distance at path[i-1]
          // currentDistance = distance at path[i]
          // distanceLeftToPosition = previousDistance + (distanceNavigated - previousDistance)
          double previousDistance = currentDistance - Vector3.Distance(path[i - 1], path[i]);
          double distanceLeftToPosition = previousDistance + (distanceNavigated - previousDistance);
          percentageLeftToPosition = distanceLeftToPosition / Vector3.Distance(path[i - 1], path[i]);
          index = i;
          break;
        }
      }
      Debug.Log(Vector3.Lerp(path[index - 1], path[index], (float)percentageLeftToPosition));
      return Vector3.Lerp(path[index - 1], path[index], (float)percentageLeftToPosition);
    }
  }

  public void setCurrentPosition(Vector3 p) {
    currentPosition = new float[3] { p.x, p.y, p.z };
  }

  public int[] getCost() {
    cost = new int[3] { computeCost(100), computeCost(50), computeCost(50) };
    return cost;
  }

  public int getSlot() {
    return slot;
  }

  public DateTime getCompletionTime() {
    return completionTime;
  }

  public void setBuilt(bool b) {
    built = b;
  }

  public bool isBuilt() {
    return built;
  }
}