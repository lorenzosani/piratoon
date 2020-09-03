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
  protected Dictionary<int, string> playersPositions;

  public Map(int _id) {
    id = _id;
    playersPositions = new Dictionary<int, string>();
  }

  public int getId() {
    return id;
  }

  public Dictionary<int, string> getPlayers() {
    return playersPositions;
  }

  public void addPlayer(int position, string playerId) {
    playersPositions[position] = playerId;
  }

  public void removePlayer(int position) {
    playersPositions.Remove(position);
  }
}