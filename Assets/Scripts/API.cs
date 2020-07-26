using UnityEngine;
using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class API 
{
  public static string androidId = string.Empty;
  public static string iosId = string.Empty;
  public static string customId = string.Empty;

  static string playFabId = string.Empty;
  static string playFabUsername = string.Empty;

  static ControllerScript controller;
  static UIScript ui;
  
  public static void RegisterScripts(ControllerScript c, UIScript u){
    controller = c;
    ui = u;
  }

  // Register a recoverable account with the provided details, for the current player
  public static void RegisterUser(string username, string email, string password, Action<string,string> callback)
  {
    PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest() {
        Email = email,
        Password = password,
        Username = username
      }, result => {
        playFabUsername = result.Username;
        if (GetStoredPlayerId() == "") {
          Debug.Log("linking new custom id");
          string guid = Guid.NewGuid().ToString();
          PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest() {
              CustomId = guid
            }, r => {
              StorePlayerId(guid);
              callback("SUCCESS", "");
            }, e => {
              OnPlayFabError(e);
            }
          );
        } else {
          StoreRegistered(true);
          callback("SUCCESS", "");
        }
      }, error => {
        if (error.ErrorDetails != null) {
          List<string> message;
          if(error.ErrorDetails.TryGetValue("Username", out message)) callback(message[0], "Username");
          else if(error.ErrorDetails.TryGetValue("Email", out message)) callback(message[0], "Email");
          else if(error.ErrorDetails.TryGetValue("Password", out message)) callback(message[0], "Password");
        } else {
          Debug.Log(error);
          callback(Language.Field["REG_ALREADY"], "");
        }
      }
    );
  }

  public static void StoredLogin(string storedId){
  Debug.Log("login with id " + storedId);
    PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest {
      CustomId = storedId, 
      CreateAccount = true
    }, OnLogin, e => OnPlayFabError(e, true));
  }

  public static void AnonymousLogin(){
    string guid = Guid.NewGuid().ToString();
    StorePlayerId(guid);
    StoredLogin(guid);
  }

  public static void OnLogin(LoginResult login)
  { 
    List<string> keys = new List<string> { "User", "Buildings", "Village" };  
    if (!login.NewlyCreated) {
      // Fetch user data
      GetUserData(keys, result => {
        if (result != null)
        { 
          // If yes, set the local user data to match the remote
          User user = JsonConvert.DeserializeObject<User>((string) result.Data["User"].Value);
          Village village = JsonConvert.DeserializeObject<Village>((string) result.Data["Village"].Value);
          user.setVillage(village);
          controller.setUser(user);
          controller.populateVillage(result.Data["Buildings"].Value);
          ui.showLoadingScreen(false);
        } else {
          Debug.Log("API Error: fetched data is null.");
        }
      });
    } else {
      // Set default data for a new user
      SetUserData(keys.ToArray());
      ui.showLoadingScreen(false);
    }
  }

  public static void SetUserData(string[] keys){
    Dictionary<string, string> data = new Dictionary<string, string>() {
      {"Buildings", JsonConvert.SerializeObject(
        controller.getUser().getVillage().getBuildings(),
        new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        })
      },
      {"Village", JsonConvert.SerializeObject(
        controller.getUser().getVillage(),
        new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        })
      },
      {"User", JsonConvert.SerializeObject(
        controller.getUser(),
        new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        })
      }
    };
    Dictionary<string, string> request = new Dictionary<string, string>();
    foreach(string key in keys)
    {
      if (data.ContainsKey(key)){
        request[key] = data[key];
      }
    }
    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
      Data=request
    }, result => {
      Debug.Log("Server-side data updated successfully.");
    }, e => OnPlayFabError(e));
  }

  public static void GetUserData(List<string> keys, Action<GetUserDataResult> callback){
    // Fetch user data
    PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
      PlayFabId = playFabId,
      Keys = keys
    }, result => { 
      // Check if user has already data
      callback(result.Data != null && result.Data.ContainsKey("User") ? result : null);
    }, e => OnPlayFabError(e));
  }

  public static string GetStoredPlayerId(){
    return PlayerPrefs.GetString("PlayerId", "");
  }

  public static void StorePlayerId(string id){
    PlayerPrefs.SetString("PlayerId", id);
  }

  public static bool IsRegistered(){
    return PlayerPrefs.GetInt("Registered", 0) > 0;
  }

  public static void StoreRegistered(bool registered){
    PlayerPrefs.SetInt("Registered", registered ? 1 : 0);
  }

  public static string GetUsername(){
    return playFabUsername;
  }

  // If something goes wrong with the API
  public static void OnPlayFabError(PlayFabError error, bool login = false)
  {
    Debug.LogWarning("Something went wrong with your API call.");
    Debug.LogError(error.GenerateErrorReport());
    if (!login) ui.showConnectionError(true);
    // Retry to connect
    if (login) {
      controller.Invoke("login", 3.0f); 
    } else {
      controller.Invoke("retryConnection", 3.0f); 
    }
  }
}