using UnityEngine;
using System.Collections;

public class User
{
  int id;
  int level;
  Village village;
  int serverId;
  int bounty;
  int[] resources;
  int pearl;
  int storage;

  public User(int _id, int _serverId, Village _village)
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

  public int getId()
  {
    return id;
  }

  public int getLevel(){
    return level;
  }

  public void increaseLevel(){
    level=level+=1;
  }

  public Village getVillage()
  {
    return village;
  }

  public int getServerId()
  {
    return serverId;
  }

  public int[] getResources()
  {
    return resources;
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
  }

  public void increaseResource(int i, int n)
  {
    resources[i] = (resources[i]+n > storage) ? storage : resources[i] + n;
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
  }
}