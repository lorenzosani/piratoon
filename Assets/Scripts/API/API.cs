using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using PlayFab.GroupsModels;
using UnityEngine;

public static class API {
  public static string androidId = string.Empty;
  public static string iosId = string.Empty;
  public static string customId = string.Empty;

  public static string entityId = string.Empty;
  public static string entityType = string.Empty;

  public static string playFabId = string.Empty;
  static string sessionTicket = string.Empty;

  static ControllerScript controller;
  static Buildings spawner;
  public static void RegisterScripts(ControllerScript c, Buildings s) {
    controller = c;
    spawner = s;
  }

  //*****************************************************************
  // REGISTER a new user
  //*****************************************************************
  public static void RegisterUser(string username, string email, string password, Action<string, string> callback) {
    // Add to the database the username and password for the user
    PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest() {
      Email = email,
        Password = password,
        Username = username
    }, result => {
      // Update the display name, which is anot the same thing as the username
      PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest() {
        DisplayName = username
      }, a => {
        // Update the contact email, which is needed to recover the password
        PlayFabClientAPI.AddOrUpdateContactEmail(new AddOrUpdateContactEmailRequest() {
          EmailAddress = email
        }, r => {
          controller.getUser().setUsername(a.DisplayName);
          StoreUsername(result.Username);
          StoreRegistered(true);
          callback("SUCCESS", "");
        }, e => {
          OnPlayFabError(e);
        });
      }, b => OnPlayFabError(b));
    }, error => {
      if (error.ErrorDetails != null) {
        List<string> message;
        if (error.ErrorDetails.TryGetValue("Username", out message))callback(message[0], "Username");
        else if (error.ErrorDetails.TryGetValue("Email", out message))callback(message[0], "Email");
        else if (error.ErrorDetails.TryGetValue("Password", out message))callback(message[0], "Password");
      } else {
        Debug.Log(error);
        callback(Language.Field["REG_ALREADY"], "");
      }
    });
  }

  //*****************************************************************
  // LOGIN a registered player using their username
  //*****************************************************************
  public static void UsernameLogin(string username, string password, Action<string> callback = null) {
    // Login to the game with the details provided
    PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest() {
      Username = username,
        TitleId = "E206C",
        Password = password
    }, result => {
      // Once logged in, retrieve all the user's info
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
        if (error.ErrorDetails.TryGetValue("Password", out message) || error.ErrorDetails.TryGetValue("Username", out message)) {
          callback(message[0]);
        }
      } else if (error.ToString().Contains(":")) {
        callback(error.ToString().Substring(error.ToString().LastIndexOf(':') + 1));
      } else {
        Debug.Log(error);
        callback(Language.Field["LOGIN_ERROR"]);
      }
    });
  }

  //*****************************************************************
  // LOGIN a registered player using their email
  //*****************************************************************
  public static void EmailLogin(string email, string password, Action<string> callback = null) {
    // Login to the game with the details provided
    PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest() {
      Email = email,
        TitleId = "E206C",
        Password = password
    }, result => {
      // Once logged in, retrieve all the user's info
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
        if (error.ErrorDetails.TryGetValue("Password", out message))callback(message[0]);
      } else if (error.ToString().Contains(":")) {
        callback(error.ToString().Substring(error.ToString().LastIndexOf(':') + 1));
      } else {
        Debug.Log(error);
        callback(Language.Field["LOGIN_ERROR"]);
      }
    });
  }

  //*****************************************************************
  // LOGIN players that are not registered, using locally stored ids
  //*****************************************************************
  public static void NewPlayerLogin(string userId = null) {
    PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest {
      CustomId = userId ?? controller.getUser().getId(),
        CreateAccount = true
    }, OnLogin, e => OnPlayFabError(e, true));
  }

  //*****************************************************************
  // LOGIN SUCCESSFUL, this is called after
  //*****************************************************************
  static void OnLogin(LoginResult login) {
    // Store the user info
    controller.getUI().showLoadingScreen();
    playFabId = login.PlayFabId;
    entityId = login.EntityToken.Entity.Id;
    entityType = login.EntityToken.Entity.Type;
    sessionTicket = login.SessionTicket;
    List<string> keys = new List<string> {
      "User",
      "Buildings",
      "Village"
    };
    if (!login.NewlyCreated) {
      // Retrieve all the data (village, buildings etc.) and show it in the game
      GetUserData(keys, result => {
        // Check if data received is valid
        if (result != null && result.Data.ContainsKey("User") && result.Data["User"].Value != "{}") {
          // If yes, de-serialize the data
          User user = JsonConvert.DeserializeObject<User>((string)result.Data["User"].Value);
          Village village = JsonConvert.DeserializeObject<Village>((string)result.Data["Village"].Value);
          List<Building> buildings = JsonConvert.DeserializeObject<List<Building>>((string)result.Data["Buildings"].Value);
          // Populate the objects with the retrieved data
          village.setBuildingsFromList(buildings);
          user.setVillage(village);
          controller.setUser(user);
          // Download info about the map the user is in
          if (controller.getUser().getMapId() != null && controller.getUser().getMapId() != "") {
            GetMapData(controller.getUser().getMapId());
          }
          // Show the village
          spawner.populateVillage(result.Data["Buildings"].Value);
          spawner.populateFloatingObjects();
          controller.getUI().onLogin();
          UpdateBounty(controller.getUser().getBounty());
        } else {
          if (result.Data["User"].Value != "") {
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

  //*****************************************************************
  // SAVE user data on the server
  //*****************************************************************
  public static void SetUserData(string[] keys) {
    Dictionary<string, string> data = new Dictionary<string, string>() {
      {
      "Buildings",
      JsonConvert.SerializeObject(
      controller.getUser().getVillage().getBuildings(),
      new JsonSerializerSettings {
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      })
      }, {
      "Village",
      JsonConvert.SerializeObject(
      controller.getUser().getVillage(),
      new JsonSerializerSettings {
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      })
      }, {
      "User",
      JsonConvert.SerializeObject(
      controller.getUser(),
      new JsonSerializerSettings {
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      })
      }
    };
    Dictionary<string, string> request = new Dictionary<string, string>();
    foreach (string key in keys) {
      if (data.ContainsKey(key)) {
        request[key] = data[key];
      }
    }
    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
      Data = request, Permission = UserDataPermission.Public
    }, result => {
      Debug.Log("Server-side data updated successfully.");
    }, e => OnPlayFabError(e));
  }

  //*****************************************************************
  // RETRIEVE user data on the server
  //*****************************************************************
  public static void GetUserData(List<string> keys, Action<GetUserDataResult> callback, string userId = null) {
    // Fetch user data
    PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
      PlayFabId = userId ?? playFabId, Keys = keys
    }, result => {
      // Check if user has already data
      callback(result.Data != null ? result : null);
    }, e => OnPlayFabError(e));
  }

  //*****************************************************************
  // RETRIEVE data about the map the user is in
  //*****************************************************************
  public static void GetMapData(string mapId) {
    PlayFabGroupsAPI.GetGroup(new GetGroupRequest {
      GroupName = mapId
    }, result => {
      PlayFabDataAPI.GetObjects(new GetObjectsRequest() {
        Entity = new PlayFab.DataModels.EntityKey() {
            Id = result.Group.Id, Type = "group"
          },
          EscapeObject = true
      }, r => {
        MapUser[] players = JsonConvert.DeserializeObject<MapUser[]>(r.Objects["players"].EscapedDataObject);
        controller.setMap(new Map(mapId, players));
      }, e => OnPlayFabError(e));
    }, error => OnPlayFabError(error));
  }

  //*****************************************************************
  // SAVE the player's bounty, which is taken care separately (because of leaderboard)
  //*****************************************************************
  public static void UpdateBounty(int value) {
    if (IsRegistered() && controller.getUser().getUsername() != null) {
      PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest {
          Statistics = new List<StatisticUpdate> {
            new StatisticUpdate {
              StatisticName = "bounty", Value = value
            }
          }
        },
        result => {
          SetUserData(new string[] {
            "User"
          });
        },
        error => {
          OnPlayFabError(error);
        });
    }
  }

  //*****************************************************************
  // RETRIEVE the players' leaderboard
  //*****************************************************************
  public static void GetLeaderboard(Action<List<PlayerLeaderboardEntry>, string> callback) {
    // Get the leaderboard around the user
    PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest {
        MaxResultsCount = 20,
          StatisticName = "bounty"
      },
      result => {
        callback(result.Leaderboard, "local");
      },
      error => {
        OnPlayFabError(error);
      });
    // Get the top positions of the leaderboard
    PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest {
        StartPosition = 0,
          MaxResultsCount = 20,
          StatisticName = "bounty"
      },
      result => {
        callback(result.Leaderboard, "absolute");
      },
      error => {
        OnPlayFabError(error);
      });
  }

  //*****************************************************************
  // SEND an email to recover a forgotten password
  //*****************************************************************
  public static void SendPasswordRecoveryEmail(string emailAddress) {
    if (emailAddress == null) {
      controller.getUI().emailRecoveryError(Language.Field["REG_EMAIL"]);
      return;
    }
    PlayFabServerAPI.SendCustomAccountRecoveryEmail(new PlayFab.ServerModels.SendCustomAccountRecoveryEmailRequest {
      Email = emailAddress,
        EmailTemplateId = "B93733D3FBBC95CD"
    }, result => {
      controller.getUI().emailRecoverySuccess(Language.Field["EMAIL_SENT"]);
    }, e => {
      controller.getUI().emailRecoveryError(e.ErrorMessage);
      Debug.LogError(e.GenerateErrorReport());
    });
  }

  //*****************************************************************
  // GETTER and SETTER methods
  //*****************************************************************
  public static string GetStoredPlayerId() {
    return PlayerPrefs.GetString("PlayerId", "");
  }
  public static bool IsRegistered() {
    return PlayerPrefs.GetInt("Registered", 0) > 0;
  }
  public static void StorePlayerId(string id) {
    PlayerPrefs.SetString("PlayerId", id);
  }
  public static void StoreRegistered(bool registered) {
    PlayerPrefs.SetInt("Registered", registered ? 1 : 0);
  }
  public static void StoreUsername(string username) {
    PlayerPrefs.SetString("Username", username);
  }
  public static string GetUsername() {
    return PlayerPrefs.GetString("Username", "");
  }

  //*****************************************************************
  // ERROR handlers: these are called when something goes wrong
  //*****************************************************************
  public static void OnPlayFabError(PlayFabError error, bool login = false) {
    Debug.LogWarning("Something went wrong with your API call.");
    Debug.LogError(error.GenerateErrorReport());
    if (!login)controller.getUI().showConnectionError(true);
    // Retry to connect
    if (login) {
      controller.Invoke("login", 3.0f);
    } else {
      controller.Invoke("retryConnection", 3.0f);
    }
  }
  public static void EmailRecoveryError(PlayFabError error) {
    Debug.LogWarning("Something went wrong with sending the email.");
    Debug.LogError(error.GenerateErrorReport());
  }
}