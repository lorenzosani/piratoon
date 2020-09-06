using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Map {
  [JsonProperty]
  protected int id;
  [JsonProperty]
  protected Dictionary<int, User> playersPositions;

  public Map(int _id) {
    id = _id;
    playersPositions = new Dictionary<int, User>();
  }

  public int getId() {
    return id;
  }

  public Dictionary<int, User> getPlayers() {
    return playersPositions;
  }

  public void addPlayer(int position, User player) {
    playersPositions[position] = player;
  }

  public void removePlayer(int position) {
    playersPositions.Remove(position);
  }
}