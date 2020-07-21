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
  UIScript ui;

  void Start()
  {
    spawner = GetComponent<BuildingSpawner>();
    ui = GameObject.Find("Rendered UI").GetComponent<UIScript>();
    user = new User("user18", "x", new Village(Vector3.zero));
    API.RegisterScripts(this, ui);
    login();
  }

  // Show buildings from json data
  public void populateVillage(string buildingsJson){
    Village village = user.getVillage();
    List<Building> buildingsList = new List<Building>();

    JArray buildingsObject = JArray.Parse(buildingsJson);
    spawner.removeAllBuildings();
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

  public void retryConnection(){
    List<string> keys = new List<string> { "User", "Buildings", "Village" };  
    API.GetUserData(keys, result => {
      if (result != null)
      { 
        ui.showConnectionError(false);
        User u = JsonConvert.DeserializeObject<User>((string) result.Data["User"].Value);
        Village village = JsonConvert.DeserializeObject<Village>((string) result.Data["Village"].Value);
        u.setVillage(village);
        setUser(u);
        populateVillage(result.Data["Buildings"].Value);
        Debug.Log("BUILDINGS: "+ result.Data["Buildings"].Value);
        Debug.Log("USER: "+ result.Data["User"].Value);
        ui.showLoadingScreen(false);
      } else {
        Debug.Log("API Error: fetched data is null.");
      }
    });
  }

  public void login(){
    API.Login();
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