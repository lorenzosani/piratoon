using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class FloatingObject {
  [JsonProperty]
  protected int id; // Each floating object has an id between 0 and 7 (8 total objects)
  [JsonProperty]
  protected DateTime time; // The time when this object appears and is collectable
  [JsonProperty]
  protected float[] position; // The position in the hideout where this appears

  public FloatingObject(int _id, DateTime _time, Vector3 _position) {
    id = _id;
    time = _time;
    position = new float[3] { _position.x, _position.y, _position.z };
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
}