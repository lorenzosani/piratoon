using UnityEngine;
using System;
using System.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class User
{
  Village village;
  [JsonProperty]
  string id;
  [JsonProperty]
  string username;
  [JsonProperty]
  int level;
  [JsonProperty]
  string serverId;
  [JsonProperty]
  int bounty;
  [JsonProperty]
  int[] resources;
  [JsonProperty]
  int pearl;
  [JsonProperty]
  int storage;

  public User(string _id, Village _village)
  {
    id = _id;
    username = null;
    level = 1;
    village = _village;
    serverId = null;
    bounty = 0;
    resources = new int[3] { 500, 700, 100 };
    pearl = 5;
    storage = 700;
  }

  public string getId()
  {
    return id;
  }

  public int getLevel(){
    return level;
  }

  public void increaseLevel(int l=-1){
    level = l<0 ? level+=1 : l;
    API.SetUserData(new string[]{"User"});
  }

  public void setUsername(string u){
    username = u;
    API.SetUserData(new string[]{"User"});
  }

  public string getUsername(){
    return username;
  }

  public Village getVillage()
  {
    return village;
  }

  public void setVillage(Village v)
  {
    village = v;
  }

  public string getMapId()
  {
    return serverId;
  }
    
  public void setMapId(string id)
  {
    serverId = id;
  }

  public int[] getResources()
  {
    return resources;
  }

  public int getStorage(){
    return storage;
  }

  public int[] getStorageSpaceLeft(){
    int[] spaceLeft = new int[3];
    for(int i=0; i<resources.Length; i++) {
      spaceLeft[i] = storage - resources[i];
    }
    return spaceLeft;
  }

  public void setResources(int[] r)
  {
    for(int i=0; i<r.Length; i++) {
      resources[i] = (r[i] > storage) ? storage : r[i];
    }
    API.SetUserData(new string[]{"User"});
  }

  public void increaseResource(int i, int n)
  {
    resources[i] = (resources[i]+n > storage) ? storage : resources[i] + n;
    API.SetUserData(new string[]{"User"});
  }

  public int getPearl()
  {
    return pearl;
  }

  public int getBounty()
  {
    return bounty;
  }

  public void addBounty(int value)
  {
    bounty += value;
    API.UpdateBounty(bounty);
  }

  public void setStorage(int s){
    storage = s;
    API.SetUserData(new string[]{"User"});
  }
}