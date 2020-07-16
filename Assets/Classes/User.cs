using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class User
{
  Village village;
  [JsonProperty]
  string id;
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

  public User(string _id, string _serverId, Village _village)
  {
    id = _id;
    level = 1;
    village = _village;
    serverId = _serverId;
    bounty = 0;
    resources = new int[3] { 500, 500, 100 };
    pearl = 5;
    storage = 500;
  }

  public string getId()
  {
    return id;
  }

  public int getLevel(){
    return level;
  }

  public void increaseLevel(){
    level=level+=1;
    API.SetUserData();
  }

  public Village getVillage()
  {
    return village;
  }

  public void setVillage(Village v)
  {
    village = v;
  }

  public string getServerId()
  {
    return serverId;
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
    API.SetUserData();
  }

  public void increaseResource(int i, int n)
  {
    resources[i] = (resources[i]+n > storage) ? storage : resources[i] + n;
    API.SetUserData();
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
  }

  public void setStorage(int s){
    storage = s;
    API.SetUserData();
  }
}