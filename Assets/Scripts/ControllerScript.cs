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
  float volume = 0.1f;
  public bool rememberLoginDetails = true;
  List<PlayerLeaderboardEntry> leaderboard = null;

  void Awake() {
    spawner = GetComponent<Buildings>();
    tutorial = GetComponent<Tutorial>();
    ui = GameObject.Find("Rendered UI").GetComponent<UI>();
    user = new User(Guid.NewGuid().ToString(), new Village(0));
    setVolume(getSavedVolume());
    InvokeRepeating("saveVolume", 5.0f, 5.0f);

    DontDestroyOnLoad(transform.gameObject);
    API.RegisterScripts(this, spawner);
    Mapmaking.RegisterScripts(this, GetComponent<Ships>());
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

  public void setVolume(float newVolume) {
    volume = newVolume;
    AudioSource[] sources = GameObject.FindObjectsOfType(typeof(AudioSource))as AudioSource[];
    foreach (AudioSource source in sources) {
      source.volume = source.gameObject.name == "Rendered UI" ? volume * 5 : volume;
    }
  }

  public void setLanguage(string lang) {
    PlayerPrefs.SetString("Language", lang);
    Language.LoadLanguage();
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

  public float getSavedVolume() {
    return float.Parse(PlayerPrefs.GetString("Volume", "0.1"));
  }

  void saveVolume() {
    PlayerPrefs.SetString("Volume", volume.ToString());
  }
}