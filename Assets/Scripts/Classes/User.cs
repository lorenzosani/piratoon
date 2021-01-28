using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// USER: This represents a user or player (terms used interchangeably)
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class User {
  Village village; // This contains info about the user's hideout
  [JsonProperty]
  string id; // The user's unique uuid
  [JsonProperty]
  string username; // The user's username
  [JsonProperty]
  int level; // Can increase level by improving their hideout, building ships, conquering cities
  [JsonProperty]
  string serverId; // The uuid of the map on which the user's hideout is
  [JsonProperty]
  int bounty; // The user's bounty indicates their 'power'
  [JsonProperty]
  int[] resources; // The amount of each resource owned by the user
  [JsonProperty]
  int pearl; // The amount of premium pearls owned by the user
  [JsonProperty]
  int storage; // The max storage space for each resource
  [JsonProperty]
  List<AttackOutcome> latestAttacks; // The latest incoming attacks to the user by other players

  public User(string _id, Village _village) {
    id = _id;
    username = null;
    level = 1;
    village = _village;
    serverId = null;
    bounty = 0;
    resources = new int[3] {
      500,
      700,
      100
    };
    pearl = 25;
    storage = 700;
    latestAttacks = new List<AttackOutcome>();
  }

  public string getId() {
    return id;
  }

  public int getLevel() {
    return level;
  }

  public void increaseLevel(int l = -1) {
    level = l < 0 ? level += 1 : l;
    API.SetUserData(new string[] {
      "User"
    });
  }

  public void setUsername(string u) {
    username = u;
    API.SetUserData(new string[] {
      "User"
    });
  }

  public string getUsername() {
    return username;
  }

  public Village getVillage() {
    return village;
  }

  public void setVillage(Village v) {
    village = v;
  }

  public string getMapId() {
    return serverId;
  }

  public void setMapId(string id) {
    serverId = id;
    API.SetUserData(new string[] {
      "User"
    });
  }

  public int[] getResources() {
    return resources;
  }

  public int getStorage() {
    return storage;
  }

  public int[] getStorageSpaceLeft() {
    int[] spaceLeft = new int[3];
    for (int i = 0; i < resources.Length; i++) {
      spaceLeft[i] = storage - resources[i];
    }
    return spaceLeft;
  }

  public void setResources(int[] r) {
    for (int i = 0; i < r.Length; i++) {
      resources[i] = (r[i] > storage) ? storage : r[i];
    }
    API.SetUserData(new string[] {
      "User"
    });
  }

  public void increaseResource(int i, int n) {
    resources[i] = (resources[i] + n > storage) ?
      storage : resources[i] + n;
    API.SetUserData(new string[] {
      "User"
    });
  }

  public int getPearl() {
    return pearl;
  }

  public void setPearl(int p) {
    pearl = p;
  }

  public int getBounty() {
    return bounty;
  }

  public void addBounty(int value) {
    bounty += value;
    API.UpdateBounty(bounty);
  }

  public void setStorage(int s) {
    storage = s;
    API.SetUserData(new string[] {
      "User"
    });
  }

  public void registerAttack(AttackOutcome ao) {
    latestAttacks.Add(ao);
  }

  public void resetAttacks() {
    latestAttacks = new List<AttackOutcome>();
    API.SetUserData(new string[] {
      "User"
    });
  }

  public List<AttackOutcome> getLatestAttacks() {
    return latestAttacks;
  }
}