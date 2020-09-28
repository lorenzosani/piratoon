using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Map {
  [JsonProperty]
  protected string id;
  [JsonProperty]
  protected MapUser[] players;

  public Map(string _id, MapUser[] _players) {
    id = _id;
    players = _players;
  }

  public string getId() {
    return id;
  }

  public MapUser[] getPlayers() {
    return players;
  }
}