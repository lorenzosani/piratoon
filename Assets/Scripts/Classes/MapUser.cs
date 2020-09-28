using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class MapUser {
  [JsonProperty]
  string id;
  [JsonProperty]
  string username;

  public MapUser(string _id, string _username) {
    id = _id;
    username = _username;
  }

  public string getId() {
    return id;
  }

  public string getUsername() {
    return username;
  }
}