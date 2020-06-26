using UnityEngine;
using System.Collections;

public class User
{
  int id;
  Village village;
  int serverId;
  int bounty;
  int[] resources;
  int pearl;

  public User(int _id, int _serverId, Village _village)
  {
    id = _id;
    village = _village;
    serverId = _serverId;
    bounty = 0;
    resources = new int[3] { 900, 900, 99 };
    pearl = 5;
  }

  public int getId()
  {
    return id;
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

  public void setResources(int[] r)
  {
    resources = r;
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
}