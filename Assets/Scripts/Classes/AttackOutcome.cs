using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// ATTACK OUTCOME: This stores the result of an attack perpetuated
// to an enemy hideout or city. 
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class AttackOutcome {
  [JsonProperty]
  protected char type; // The type of the attack can be either 'p' for "plunder" or 'c' for "conquer"
  [JsonProperty]
  protected char outcome; // Can be 'v' for victory or 'd' for defeat
  [JsonProperty]
  protected string attacker; // The name of the attacking player
  [JsonProperty]
  protected string target; // If the player's hideout was attacked then "hideout", otherwise the name of the city
  [JsonProperty]
  protected int[] resources; // The resources won in case of victory

  public AttackOutcome(char _type, char _outcome, string _attacker, string _target, int[] _resources = null) {
    type = _type;
    outcome = _outcome;
    attacker = _attacker;
    target = _target;
    resources = _resources;
  }

  public char getType() {
    return type;
  }

  public string getAttacker() {
    return attacker;
  }

  public string getTarget() {
    return target;
  }

  public int[] getResources() {
    return resources;
  }
}