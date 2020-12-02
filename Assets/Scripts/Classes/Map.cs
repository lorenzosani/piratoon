using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

//*****************************************************************
// MAP: This represents any world map in the game
//*****************************************************************

[JsonObject(MemberSerialization.OptIn)]
public class Map {
  [JsonProperty]
  protected string id; // Each map has an unique uuid
  [JsonProperty]
  protected MapUser[] players; // List of players that have their hideout on this map
  [JsonProperty]
  protected City[] cities;

  public Map(string _id, MapUser[] _players, City[] _cities = null) {
    id = _id;
    players = _players;
    cities = cities ?? generateNewCities(103);
  }

  public string getId() {
    return id;
  }

  public MapUser[] getPlayers() {
    return players;
  }

  public City[] getCities() {
    return cities;
  }

  public City[] generateNewCities(int n) {
    City[] citiesArray = new City[n];
    for (int i = 0; i < n; i++) {
      citiesArray[i] = new City(CityNames.getCity(i));
    }
    return citiesArray;
  }
}