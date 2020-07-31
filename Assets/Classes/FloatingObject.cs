using UnityEngine;
using System;
using System.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class FloatingObject
{
  [JsonProperty]
  protected int id;
  [JsonProperty]
  protected DateTime time;
  [JsonProperty]
  protected Vector3 position;

  public FloatingObject(int _id, DateTime _time, Vector3 _position){
    id = _id;
    time = _time;
    position = _position;
  }

  public int getId(){
    return id;
  }

  public DateTime getTime(){
    return time;
  }

  public Vector3 getPosition(){
    return position;
  }
}