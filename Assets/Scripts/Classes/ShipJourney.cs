using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// SHIP JOURNEY: This represents any journey of a player's ship from
// point A to point B on the map.
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class ShipJourney {
  [JsonProperty]
  protected float[] startPoint; // The starting point on the map for this journey
  [JsonProperty]
  protected float[] arrivalPoint; // The arrival point on the map for this journey
  [JsonProperty]
  protected DateTime startTime; // When this journey has started

  public ShipJourney(Vector3 _startPoint, Vector3 _arrivalPoint, DateTime _startTime) {
    startPoint = new float[3] { _startPoint.x, _startPoint.y, _startPoint.z };
    arrivalPoint = new float[3] { _arrivalPoint.x, _arrivalPoint.y, _arrivalPoint.z };
    startTime = _startTime;
  }

  public Vector3 getStartPoint() {
    return new Vector3(startPoint[0], startPoint[1], startPoint[2]);
  }

  public Vector3 getArrivalPoint() {
    return new Vector3(arrivalPoint[0], arrivalPoint[1], arrivalPoint[2]);
  }

  public DateTime getArrivalTime() {
    return startTime.Add(getDuration());
  }

  public DateTime getStartTime() {
    return startTime;
  }

  public TimeSpan getDuration() {
    // Needs implementation
    return new TimeSpan();
  }

  public int getShipConditionCost() {
    // Needs implementation
    return 0;
  }
}