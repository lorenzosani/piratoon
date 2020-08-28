using UnityEngine;
using System;
using System.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Map
{
  [JsonProperty]
  protected int id;
  [JsonProperty]
  protected Dictionary<int, string> playersPositions;

  public Map(int _id){
    id = _id;
    playersPositions = new Dictionary<int, string>();
  }

  public int getId(){
    return id;
  }

  public Dictionary<int, string> getPlayers(){
    return playersPositions;
  }

  public void addPlayer(int position, string playerId){
    playersPositions[position] = playerId;
  }
}