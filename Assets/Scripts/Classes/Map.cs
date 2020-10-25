using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Map {
  [JsonProperty]
  protected string id; // Each map has an unique uuid
  [JsonProperty]
  protected MapUser[] players; // List of players that have their hideout on this map

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