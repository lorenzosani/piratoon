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

  public City(string _name) {
    name = _name;
    level = randomLevel();
    hourlyProductionWeights = generateHourlyProductionWeights();
    resources = generateNewResources();
    owner = "";
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
      hourlyProductionWeights[0] * randomInt(name[0], 5) * 19 * level,
        hourlyProductionWeights[0] * randomInt(name[1], 5) * 21 * level,
        hourlyProductionWeights[0] * randomInt(name[2], 2) * 7 * level
    };
  }

  public int[] getResources() {
    return resources;
  }

  public void setResources(int[] r) {
    resources = r;
  }

  public void setOwner(string o) {
    owner = o;
  }

  public string getOwner() {
    return owner;
  }
}