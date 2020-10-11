using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class FloatingObject {
  [JsonProperty]
  protected int id;
  [JsonProperty]
  protected DateTime time;
  [JsonProperty]
  protected float[] position;

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