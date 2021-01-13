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
  [JsonProperty]
  protected string name;
  [JsonProperty]
  protected int level;
  [JsonProperty]
  protected int[] hourlyProductionWeights;
  [JsonProperty]
  protected int[] resources;
  [JsonProperty]
  protected string owner;
  [JsonProperty]
  protected DateTime cooldownEnd;
  [JsonProperty]
  protected DateTime lastCollected;

  public City(string _name) {
    name = _name;
    level = randomLevel();
    hourlyProductionWeights = generateHourlyProductionWeights();
    resources = generateNewResources();
    owner = "";
    cooldownEnd = DateTime.UtcNow;
    lastCollected = DateTime.UtcNow;
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

  public void increaseLevel() {
    level += 1;
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

  public int[] generateHourlyProductionWeights() {
    return new int[3] {
      randomInt(name[0], 10), randomInt(name[1], 10), randomInt(name[2], 10)
    };
  }

  public int[] getHourlyProduction() {
    int[] w = hourlyProductionWeights;
    return new int[3] { w[0] * level, w[1] * level, w[2] * level / 3 };
  }

  public int[] generateNewResources() {
    return new int[3] {
      hourlyProductionWeights[0] * randomInt(name[0], 5) * 3 * level,
        hourlyProductionWeights[0] * randomInt(name[1], 5) * 4 * level,
        hourlyProductionWeights[0] * randomInt(name[2], 2) * 2 * level
    };
  }

  public int[] getResources() {
    int[] totalResources = new int[3];
    int[] resourcesProduced = getResourcesProducedSinceLastCollected();
    for (int i = 0; i < 3; i++) {
      totalResources[i] = resources[i] + resourcesProduced[i];
      if (totalResources[i] > level * 10000)totalResources[i] = level * 10000;
    }
    return totalResources;
  }

  int[] getResourcesProducedSinceLastCollected() {
    int[] hourlyProd = getHourlyProduction();
    int hoursPassed = (System.DateTime.UtcNow - lastCollected).Hours;
    return new int[3] { hourlyProd[0] * hoursPassed, hourlyProd[1] * hoursPassed, hourlyProd[2] * hoursPassed };
  }

  public void setResources(int[] r) {
    resources = r;
    lastCollected = DateTime.UtcNow;
  }

  public void setOwner(string o) {
    owner = o;
  }

  public string getOwner() {
    return owner;
  }

  public void setCooldownEnd(DateTime d) {
    cooldownEnd = d;
  }

  public DateTime getCooldownEnd() {
    return cooldownEnd;
  }
}