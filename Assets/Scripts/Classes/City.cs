using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// CITY: This represents any world map in the game
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class City {
  public DateTime[] lc = null; // Last Collected (when resources have been last collected)
  public int[] hpw = null; // Hourly Production Weights (which resources is produced most in this city)
  [JsonProperty]
  public string name;
  [JsonProperty]
  public int level;
  [JsonProperty]
  public int[] res; // Resources in the city
  [JsonProperty]
  public string owner;
  [JsonProperty]
  public DateTime cde; // CoolDown End (when the city can be attacked again)

  public City(string _name) {
    name = _name;
    level = randomLevel();
    hpw = generatehpw();
    res = generateNewResources();
    owner = "";
    cde = DateTime.UtcNow;
    lc = new DateTime[] { DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow };
  }

  public string getName() {
    return name;
  }

  public void setName(string n) {
    name = n;
  }

  public int getLevel() {
    return level;
  }

  public void setLevel(int l) {
    level = l;
  }

  public DateTime[] getLastCollected() {
    return lc;
  }

  public void setLastCollected(DateTime[] lastCollected) {
    lc = lastCollected;
  }

  public void increaseLevel() {
    level += 1;
    API.UpdateCities();
  }

  public int randomInt(char seed, int max) {
    System.Random rnd = new System.Random(seed);
    return rnd.Next(1, max + 1);
  }

  public int randomLevel() {
    int firstVal = randomInt(name[2], 3);
    if (firstVal == 1)return firstVal;
    int secondVal = randomInt(name[1], 3);
    if (secondVal == 1)return secondVal;
    return randomInt(name[0], 3);
  }

  public int[] generatehpw() {
    return new int[3] {
      randomInt(name[0], 10), randomInt(name[1], 10), randomInt(name[2], 10)
    };
  }

  public void sethpw() {
    hpw = generatehpw();
  }

  public int[] getHourlyProduction() {
    int[] w = hpw;
    return new int[3] { w[0] * level, w[1] * level, w[2] * level / 3 };
  }

  public int[] generateNewResources() {
    return new int[3] {
      hpw[0] * randomInt(name[0], 5) * 3 * level,
        hpw[0] * randomInt(name[1], 5) * 4 * level,
        hpw[0] * randomInt(name[2], 2) * 2 * level
    };
  }

  public int[] getResources() {
    int[] totalResources = new int[3];
    int[] resourcesProduced = getResourcesProducedSinceLastCollected();
    for (int i = 0; i < 3; i++) {
      totalResources[i] = res[i] + resourcesProduced[i];
      if (totalResources[i] > level * 10000)totalResources[i] = level * 10000;
    }
    return totalResources;
  }

  int[] getResourcesProducedSinceLastCollected() {
    int[] hourlyProd = getHourlyProduction();
    int[] hoursPassed = new int[3];
    for (int i = 0; i < 3; i++) {
      hoursPassed[i] = (System.DateTime.UtcNow - lc[i]).Hours;
    }
    return new int[3] { hourlyProd[0] * hoursPassed[0], hourlyProd[1] * hoursPassed[1], hourlyProd[2] * hoursPassed[2] };
  }

  public void setResource(int i, int n) {
    res[i] = n;
    lc[i] = DateTime.UtcNow;
    API.UpdateCities(true);
  }

  public void setResources(int[] r, bool updateData = true) {
    res = r;
    lc = new DateTime[] { DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow };
    if (updateData)API.UpdateCities(true);
  }

  public void setOwner(string o, bool updateData = true) {
    owner = o;
    if (updateData)API.UpdateCities();
  }

  public string getOwner() {
    return owner;
  }

  public void setCooldownEnd(DateTime d, bool updateData = true) {
    cde = d;
    if (updateData)API.UpdateCities();
  }

  public DateTime getCooldownEnd() {
    return cde;
  }

  public int[] getUpgradeCost() {
    return new int[3] {
      (level + 1) * 1000, (level + 1) * 1000, (level + 1) * 500
    };
  }
}