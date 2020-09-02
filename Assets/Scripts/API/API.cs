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
  static string sessionTicket = string.Empty;

  static ControllerScript controller;
  static Buildings spawner;
  
  public static void RegisterScripts(ControllerScript c, Buildings s){
    controller = c;
    spawner = s;
  }

  // Register a recoverable account with the provided details, for the current player
  public static void RegisterUser(string username, string email, string password, Action<string,string> callback)
  {
    PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest() {
        Email = email,
        Password = password,
        Username = username
      }, result => {
        PlayFabClientAPI.AddOrUpdateContactEmail( new AddOrUpdateContactEmailRequest() {
          EmailAddress = email
        }, res => {
          StoreUsername(result.Username);
          StoreRegistered(true);
          callback("SUCCESS", "");
          PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest() {
            DisplayName = username
          }, a => { 
            controller.getUser().setUsername(a.DisplayName);
          }, b => OnPlayFabError(b));
        }, err => {
          OnPlayFabError(err);
        });
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

  public static void UsernameLogin(string username, string password, Action<string> callback=null){
    PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest() {
      Username = username,
      TitleId = "E206C",
      Password = password
    }, result => {
      PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), r => {
        StorePlayerId(r.AccountInfo.CustomIdInfo.CustomId);
        StoreUsername(r.AccountInfo.Username);
        StoreRegistered(true);
        OnLogin(result);
        callback("SUCCESS");
      }, e => {
        OnPlayFabError(e);
      });
    }, error => {
      if (error.ErrorDetails != null) {
        List<string> message;
        if(error.ErrorDetails.TryGetValue("Password", out message) || error.ErrorDetails.TryGetValue("Username", out message)) {
           callback(message[0]);
        }
      } else if (error.ToString().Contains(":")){
        callback(error.ToString().Substring(error.ToString().LastIndexOf(':')+1));
      } else {
        Debug.Log(error);
        callback(Language.Field["LOGIN_ERROR"]);
      }
    });
  }

  public static void EmailLogin(string email, string password, Action<string> callback=null){
    PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest() {
      Email = email,
      TitleId = "E206C",
      Password = password
    }, result => {
      PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), r => {
        StorePlayerId(r.AccountInfo.CustomIdInfo.CustomId);
        StoreUsername(r.AccountInfo.Username);
        StoreRegistered(true);
        OnLogin(result);
        callback("SUCCESS");
      }, e => {
        OnPlayFabError(e);
      });
    }, error => {
      if (error.ErrorDetails != null) {
        List<string> message;
        if(error.ErrorDetails.TryGetValue("Password", out message)) callback(message[0]);
      } else if (error.ToString().Contains(":")){
        callback(error.ToString().Substring(error.ToString().LastIndexOf(':')+1));
      } else {
        Debug.Log(error);
        callback(Language.Field["LOGIN_ERROR"]);
      }
    });
  }

  public static void StoredLogin(string storedId){
    PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest {
      CustomId = storedId, 
      CreateAccount = true
    }, OnLogin, e => OnPlayFabError(e, true));
  }

  public static void AnonymousLogin(){
    string guid = controller.getUser().getId();
    StorePlayerId(guid);
    StoredLogin(guid);
  }

  public static void OnLogin(LoginResult login)
  { 
    controller.getUI().showLoadingScreen();
    playFabId = login.PlayFabId;
    sessionTicket = login.SessionTicket;
    List<string> keys = new List<string> { "User", "Buildings", "Village" };
    if (!login.NewlyCreated) {
      GetUserData(keys, result => {
        // Check if data received is valid
        if (result != null && result.Data.ContainsKey("User") && result.Data["User"].Value != "{}")
        { 
          // If yes, de-serialize the data
          User user = JsonConvert.DeserializeObject<User>((string) result.Data["User"].Value);
          Village village = JsonConvert.DeserializeObject<Village>((string) result.Data["Village"].Value);
          List<Building> buildings = JsonConvert.DeserializeObject<List<Building>>((string) result.Data["Buildings"].Value);
          // Populate the objects with the retrieved data
          village.setBuildingsFromList(buildings);
          user.setVillage(village);
          controller.setUser(user);
          // Show the village
          spawner.populateVillage(result.Data["Buildings"].Value);
          spawner.populateFloatingObjects();
          controller.getUI().onLogin();
          GetLeaderboard((leaderboard, type) => controller.setLeaderboard(leaderboard, type));
          UpdateBounty(controller.getUser().getBounty());
        } else {
          if(result.Data["User"].Value != ""){
            SetUserData(keys.ToArray());
            controller.getUI().Invoke("hideLoadingScreen", 0.5f);
          } else {
            Debug.Log("API Error: fetched data is null.");
          }
        }
      });
    } else {
      // Set default data for a new user
      SetUserData(keys.ToArray());
      controller.getUI().Invoke("hideLoadingScreen", 0.5f);
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
      callback(result.Data != null ? result : null);
    }, e => OnPlayFabError(e));
  }

  public static void UpdateBounty(int value){
    if(IsRegistered()){
      PlayFabClientAPI.UpdatePlayerStatistics( new UpdatePlayerStatisticsRequest {
        Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = "bounty", Value = value } }
      },
      result => { SetUserData(new string[]{"User"}); },
      error => { OnPlayFabError(error); });
    } 
  }

  public static void GetLeaderboard(Action<List<PlayerLeaderboardEntry>, string> callback){
    // Get the leaderboard around the user
    PlayFabClientAPI.GetLeaderboardAroundPlayer( new GetLeaderboardAroundPlayerRequest {
      MaxResultsCount = 20,
      StatisticName = "bounty"
    },
    result => {
      callback(result.Leaderboard, "local");
    },
    error => { OnPlayFabError(error); });
    // Get the top positions of the leaderboard
    PlayFabClientAPI.GetLeaderboard( new GetLeaderboardRequest {
      StartPosition = 0,
      MaxResultsCount = 20,
      StatisticName = "bounty"
    },
    result => {
      callback(result.Leaderboard, "absolute");
    },
    error => { OnPlayFabError(error); });
  }

  public static void SendPasswordRecoveryEmail(string emailAddress)
  {
    if (emailAddress == null) {
      controller.getUI().emailRecoveryError(Language.Field["REG_EMAIL"]);
      return;
    }
    PlayFabServerAPI.SendCustomAccountRecoveryEmail(new PlayFab.ServerModels.SendCustomAccountRecoveryEmailRequest
    {
      Email = emailAddress,
      EmailTemplateId = "B93733D3FBBC95CD"
    }, result => {
      controller.getUI().emailRecoverySuccess(Language.Field["EMAIL_SENT"]);
    }, e => { 
      controller.getUI().emailRecoveryError(e.ErrorMessage);
      Debug.LogError(e.GenerateErrorReport()); 
    });
  }

  public static string GetStoredPlayerId(){
    return PlayerPrefs.GetString("PlayerId", "");
  }

  public static bool IsRegistered(){
    return PlayerPrefs.GetInt("Registered", 0) > 0;
  }

  public static void StorePlayerId(string id){
    PlayerPrefs.SetString("PlayerId", id);
  }

  public static void StoreRegistered(bool registered){
    PlayerPrefs.SetInt("Registered", registered ? 1 : 0);
  }

  public static void StoreUsername(string username){
    PlayerPrefs.SetString("Username", username);
  }

  public static string GetUsername(){
    return PlayerPrefs.GetString("Username", "");
  }

  // If something goes wrong with the API
  public static void OnPlayFabError(PlayFabError error, bool login = false)
  {
    Debug.LogWarning("Something went wrong with your API call.");
    Debug.LogError(error.GenerateErrorReport());
    if (!login) controller.getUI().showConnectionError(true);
    // Retry to connect
    if (login) {
      controller.Invoke("login", 3.0f); 
    } else {
      controller.Invoke("retryConnection", 3.0f); 
    }
  }

  public static void EmailRecoveryError(PlayFabError error){
    // Show an error message 
    Debug.LogWarning("Something went wrong with sending the email.");
    Debug.LogError(error.GenerateErrorReport());
  }
}