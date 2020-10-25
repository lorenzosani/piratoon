using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// MAP USER: This represents any user on a given map. It's a compact
// version of the class User.
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class MapUser {
  [JsonProperty]
  string id; // The uuid of the user
  [JsonProperty]
  string username; // The username of the user

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