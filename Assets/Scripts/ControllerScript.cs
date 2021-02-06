using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControllerScript : MonoBehaviour {
  User user = null;
  Map map = null;

  Buildings spawner;
  UI ui;
  Tutorial tutorial;

  string email;
  public bool rememberLoginDetails = true;
  List<PlayerLeaderboardEntry> leaderboard = null;

  void Awake() {
    spawner = GetComponent<Buildings>();
    tutorial = GetComponent<Tutorial>();
    ui = GameObject.Find("Rendered UI").GetComponent<UI>();
    user = new User(Guid.NewGuid().ToString(), new Village(0));

    DontDestroyOnLoad(transform.gameObject);
    API.RegisterScripts(this, spawner);
    Mapmaking.RegisterScripts(this);
    login();
  }

  // Attempts to login a player using either stored or device details
  public void login() {
    string storedId = API.GetStoredPlayerId();
    if (PlayerPrefs.GetInt("FirstAccess", 1) == 1) {
      newPlayerLogin();
      PlayerPrefs.SetInt("FirstAccess", 0);
    } else if (!isEmpty(storedId) && rememberLoginDetails) {
      API.NewPlayerLogin(storedId);
    } else {
      ui.showNewGameOrLogin();
    }
  }

  public void newPlayerLogin() {
    API.StoreUsername("");
    API.StoreRegistered(false);
    API.StorePlayerId(user.getId());
    API.NewPlayerLogin();
    // Show the tutorial the first time the user logs in
    tutorial.showTutorial();
  }

  public void retryConnection() {
    List<string> keys = new List<string> {
      "User",
      "Buildings",
      "Village"
    };
    API.GetUserData(keys, result => {
      if (result != null) {
        ui.showConnectionError(false);
        User u = JsonConvert.DeserializeObject<User>((string)result.Data["User"].Value);
        Village village = JsonConvert.DeserializeObject<Village>((string)result.Data["Village"].Value);
        u.setVillage(village);
        setUser(u);
        spawner.populateVillage(result.Data["Buildings"].Value);
        ui.hideLoadingScreen();
      } else {
        Debug.Log("API Error: fetched data is null.");
      }
    });
  }

  public void openURL(string url) {
    Application.OpenURL(url);
  }

  //*****************************************************************
  // GETTERS and SETTERS methods
  //*****************************************************************
  public User getUser() {
    return user;
  }

  public void setUser(User u) {
    user = u;
  }

  public UI getUI() {
    return ui;
  }

  public void setMap(Map m) {
    map = m;
  }

  public Map getMap() {
    return map;
  }

  public void setLeaderboard(List<PlayerLeaderboardEntry> llb, string type) {
    ui.populateLeaderboard(llb, type);
  }

  public List<PlayerLeaderboardEntry> getLeaderboard() {
    return leaderboard;
  }

  //*****************************************************************
  // HELPER methods
  //*****************************************************************
  bool isEmpty(string value) {
    return value == "" || value == " " || value == null;
  }
}