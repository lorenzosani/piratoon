using System;
using System.Collections;
using System.Collections.Generic;
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
  [JsonProperty]
  protected float[, ] path = null; // The actual path that the ship has to follow on the map to get to the destination
  [JsonProperty]
  protected int duration; // The duration of the journey
  [JsonProperty]
  protected string destinationName; // The name of the object towards which the ship is navigating

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
    return startTime.Add(TimeSpan.FromSeconds(getDuration()));
  }

  public DateTime getStartTime() {
    return startTime;
  }

  public void setPath(List<Vector3> p) {
    float[, ] pathArr = new float[p.Count, 3];
    for (int i = 0; i < p.Count; i++) {
      pathArr[i, 0] = p[i].x;
      pathArr[i, 1] = p[i].y;
      pathArr[i, 2] = p[i].z;
    }
    path = pathArr;
  }

  public List<Vector3> getPath() {
    if (path == null)return new List<Vector3>();
    List<Vector3> listPath = new List<Vector3>();
    for (int i = 0; i < path.GetLength(0); i++) {
      Vector3 v = new Vector3(path[i, 0], path[i, 1], path[i, 2]);
      listPath.Add(v);
    }
    return listPath;
  }

  public void setDuration(int d) {
    duration = d;
  }

  public int getDuration() {
    return duration;
  }

  public string getDestinationName() {
    return destinationName;
  }

  public void setDestinationName(string d) {
    destinationName = d;
  }

  public int getShipConditionCost() {
    // Needs implementation
    return 10;
  }
}