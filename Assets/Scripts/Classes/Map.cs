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
    cities = _cities ?? generateNewCities(103);
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

  public City getCity(int i) {
    return cities[i];
  }

  public City[] generateNewCities(int n) {
    City[] citiesArray = new City[n];
    for (int i = 0; i < n; i++) {
      citiesArray[i] = new City(CityNames.getCity(i));
    }
    return citiesArray;
  }

  public void setCityConquered(int cityNo, string owner) {
    City city = cities[cityNo];
    city.setOwner(owner, true);
    city.setCooldownEnd(DateTime.UtcNow.AddHours(24));
  }

  public int[] getCitiesOwnedBy(string owner) {
    List<int> citiesOwned = new List<int>();
    for (int i = 0; i < cities.Length; i++) {
      if (cities[i].getOwner() == owner)citiesOwned.Add(i);
    }
    return citiesOwned.ToArray();
  }

  public void upgradeCity(int i) {
    cities[i].increaseLevel();
  }
}