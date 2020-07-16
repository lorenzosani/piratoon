using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ControllerScript : MonoBehaviour
{
  User user;
  BuildingSpawner spawner;

  void Awake()
  {
    spawner = GetComponent<BuildingSpawner>();
    user = new User("user16", "x", new Village(Vector3.zero));
    API.RegisterScripts(this);
    API.Login();
  }

  // Show buildings from json data
  public void populateVillage(string buildingsJson){
    Village village = user.getVillage();
    List<Building> buildingsList = new List<Building>();

    JArray buildingsObject = JArray.Parse(buildingsJson);
    for(int i=0; i<buildingsObject.Count; i++){
      Building b = spawner.createBuilding((string) buildingsObject[i]["name"]);
      b.setLevel((int) buildingsObject[i]["level"]);
      b.setPosition(
        new Vector3(
          (float)buildingsObject[i]["position"]["x"],
          (float)buildingsObject[i]["position"]["y"],
          (float)buildingsObject[i]["position"]["z"]
        )
      );
      string t = (string) buildingsObject[i]["completionTime"];
      t = t.Replace("T", " ").Replace("Z", "");
      b.setCompletionTime(DateTime.ParseExact(t, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
      b.setValue((int) buildingsObject[i]["value"]);
      b.setLocalStorage((int) buildingsObject[i]["localStorage"]);
      b.setBuilt((bool) buildingsObject[i]["built"]);
      buildingsList.Add(b);
      spawner.addBuildingFromServer(b);
    }
    village.setBuildingsFromList(buildingsList);
  }

  //*****************************************************************
  // GETTERS and SETTERS methods
  //*****************************************************************
  public User getUser()
  {
    return user;
  }

  public void setUser(User u)
  {
    user = u;
  }
}