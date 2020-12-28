using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// FLOATING OBJECT: This represents one of those objects that appear
// in the hideout's bay and give resources when collected.
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class FloatingObject {
  [JsonProperty]
  protected int id; // Each floating object has an id between 0 and 7 (8 total objects)
  [JsonProperty]
  protected DateTime time; // The time when this object appears and is collectable
  [JsonProperty]
  protected float[] position; // The position in the hideout where this appears
  [JsonProperty]
  protected int positionId; // Each position has an id, to check that there are not two objs at the same pos

  public FloatingObject(int _id, DateTime _time, int _posId) {
    id = _id;
    time = _time;
    positionId = _posId;
    Vector3 posVector = FloatingObjectsPositions.get(positionId);
    position = new float[3] { posVector.x, posVector.y, posVector.z };
  }

  public int getId() {
    return id;
  }

  public DateTime getTime() {
    return time;
  }

  public Vector3 getPosition() {
    return new Vector3(position[0], position[1], position[2]);
  }

  public int getPositionId() {
    return positionId;
  }
}