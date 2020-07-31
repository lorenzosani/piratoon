using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

public class ControllerScript : MonoBehaviour
{
  public bool rememberLoginDetails = true;

  User user;
  BuildingSpawner spawner;
  UIScript ui;

  void Awake()
  {
    spawner = GetComponent<BuildingSpawner>();
    ui = GameObject.Find("Rendered UI").GetComponent<UIScript>();
    user = new User("user", "x", new Village(Vector3.zero));
    API.RegisterScripts(this, spawner);
    login();
  }

  // Attempts to login a player using either stored or device details
  public void login(){
    string storedId = API.GetStoredPlayerId();
    if (!isEmpty(storedId) && rememberLoginDetails){
      API.StoredLogin(storedId);
    } else {
      API.StorePlayerId("");
      API.StoreRegistered(false);
      API.AnonymousLogin();
    }
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
        spawner.populateVillage(result.Data["Buildings"].Value);
        ui.hideLoadingScreen();
      } else {
        Debug.Log("API Error: fetched data is null.");
      }
    });
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

  public UIScript getUI(){
    return ui;
  }

  //*****************************************************************
  // HELPER methods
  //*****************************************************************
  bool isEmpty(string value){
    return value == "" || value == " " || value == null;
  }
}