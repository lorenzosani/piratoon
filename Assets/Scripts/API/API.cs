using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using PlayFab.GroupsModels;
using PlayFab.Internal;
using UnityEngine;
using UnityEngine.Networking;

public static class API {
  public static string androidId = string.Empty;
  public static string iosId = string.Empty;
  public static string customId = string.Empty;

  public static string entityId = string.Empty;
  public static string entityType = string.Empty;
  public static string mapEntityId = string.Empty;

  public static string playFabId = string.Empty;
  static string sessionTicket = string.Empty;

  static ControllerScript controller;
  static Buildings spawner;
  public static void RegisterScripts(ControllerScript c, Buildings s) {
    controller = c;
    spawner = s;
  }

  static City[] citiesUpdated = null;
  static bool updateLastCollected = false;
  static Debouncer debouncer = new Debouncer(600, () => updateUserData(playFabId));
  static Debouncer bountyDebouncer = new Debouncer(1000, () => debouncedUpdateBounty());
  static Debouncer citiesDebouncer = new Debouncer(600, () => {
    if (updateLastCollected) {
      updateCitiesLastCollected();
      updateLastCollected = false;
    }
    updateCitiesData();
  });

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
      OnLogin(result);
      callback("SUCCESS");
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
      OnLogin(result);
      callback("SUCCESS");
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
        if (result != null && result.Data.ContainsKey("User")) {
          // If yes, de-serialize and set the data objects
          User user = JsonConvert.DeserializeObject<User>((string)result.Data["User"].Value);
          Village village = JsonConvert.DeserializeObject<Village>((string)result.Data["Village"].Value);
          user.setVillage(village);
          controller.setUser(user);
          // If the user has built buildings, add them too
          if (result.Data.ContainsKey("Buildings") && result.Data["Buildings"].Value != "{}" && result.Data["Buildings"].Value != "") {
            List<Building> buildings = JsonConvert.DeserializeObject<List<Building>>((string)result.Data["Buildings"].Value);
            village.setBuildingsFromList(buildings);
            spawner.populateVillage(result.Data["Buildings"].Value);
          }
          PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), r => {
            StorePlayerId(r.AccountInfo.CustomIdInfo.CustomId);
            StoreUsername(r.AccountInfo.Username);
            StoreRegistered(true);
            UpdateBounty(controller.getUser().getBounty());
          }, e => {
            OnPlayFabError(e);
          });
          // Show the village
          spawner.populateFloatingObjects();
          spawner.populateShips();
          controller.getUI().onLogin();
          // Check if the user has been attacked while online
          if (user.getLatestAttacks().Count > 0) {
            controller.getUI().showAttacksSuffered(user.getLatestAttacks());
            controller.getUser().resetAttacks();
          }
        } else {
          OnPlayFabError(null, true);
        }
      });
    } else {
      // Set default data for a new user
      User user = new User(Guid.NewGuid().ToString(), new Village(0));
      controller.setUser(user);
      SetUserData(new string[] {
        "User",
        "Village"
      });
      controller.getUI().Invoke("hideLoadingScreen", 0.5f);
    }
  }

  //*****************************************************************
  // SAVE user data on the server
  //*****************************************************************
  static Dictionary<string, string> request = new Dictionary<string, string>();
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
    foreach (string key in keys) {
      if (data.ContainsKey(key) && data[key].Length > 3) {
        request[key] = data[key];
      }
    }
    // Debounce API call
    debouncer.onChange();
  }

  async static void updateUserData(string userId) {
    bool updatedSuccessfully = false;
    if (request.Count > 0 && userId == playFabId) {
      PlayFabAdminAPI.UpdateUserData(new PlayFab.AdminModels.UpdateUserDataRequest() {
        Data = request, Permission = PlayFab.AdminModels.UserDataPermission.Public, PlayFabId = userId
      }, result => {
        Debug.Log("API UPDATE SUCCESSFUL");
        updatedSuccessfully = true;
        request = new Dictionary<string, string>();
      }, e => OnPlayFabError(e));
      // If response not received, try again
      await Task.Delay(1000);
      if (!updatedSuccessfully)updateUserData(userId);
    }
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
    if (mapEntityId == string.Empty) {
      GetMapEntityId(() => GetMapData(mapId));
      return;
    }
    PlayFabDataAPI.GetObjects(new GetObjectsRequest() {
      Entity = new PlayFab.DataModels.EntityKey() {
          Id = mapEntityId, Type = "group"
        },
        EscapeObject = true
    }, r => {
      // Get the players' and cities' information
      MapUser[] players = JsonConvert.DeserializeObject<MapUser[]>(r.Objects["players"].EscapedDataObject);
      GetCities(mapId, players);
    }, e => OnPlayFabError(e));
  }

  //*****************************************************************
  // GET the entity ID of the map, this must be used in every call that changes the map
  //*****************************************************************
  static void GetMapEntityId(Action callback) {
    PlayFabGroupsAPI.GetGroup(new GetGroupRequest {
      GroupName = controller.getUser().getMapId()
    }, result => {
      mapEntityId = result.Group.Id;
      callback();
    }, error => OnPlayFabError(error));
  }

  //*****************************************************************
  // UPDATE information about cities on a map
  //*****************************************************************
  public static async void CreateNewCitiesOnServer() {
    citiesUpdated = new Map("", new MapUser[0]).getCities();
    updateCitiesData();
    await Task.Delay(1000);
    updateCitiesLastCollected();
  }

  //*****************************************************************
  // UPDATE information about cities on a map
  //*****************************************************************
  public static void UpdateCities(bool lastCollected = false) {
    if (controller.getMap() != null) {
      citiesUpdated = controller.getMap().getCities();
      updateLastCollected = lastCollected;
      citiesDebouncer.onChange();
    } else {
      Debug.Log("Can't update the cities: no Map has been found.");
    }
  }
  static void updateCitiesData() {
    City[] cities = citiesUpdated;
    string mapId = controller.getUser().getMapId();
    // Get entity info
    if (mapEntityId == string.Empty) {
      GetMapEntityId(() => updateCitiesData());
      return;
    }
    // Initialise file upload
    PlayFabDataAPI.InitiateFileUploads(
      new PlayFab.DataModels.InitiateFileUploadsRequest {
        Entity = new PlayFab.DataModels.EntityKey { Id = mapEntityId, Type = "group" },
          FileNames = new List<string> { "citiesData" }
      }, r => {
        // Serialize cities
        string citiesSerialized = JsonConvert.SerializeObject(cities, new JsonSerializerSettings {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        // Transform cities into bytes
        byte[] citiesBytes = Encoding.UTF8.GetBytes(citiesSerialized);
        // Upload bytes as payload
        PlayFabHttp.SimplePutCall(r.UploadDetails[0].UploadUrl, citiesBytes,
          risposta => {
            PlayFabDataAPI.FinalizeFileUploads(new PlayFab.DataModels.FinalizeFileUploadsRequest {
              Entity = new PlayFab.DataModels.EntityKey { Id = mapEntityId, Type = "group" },
                FileNames = new List<string> { "citiesData" },
            }, yes => {
              // Confirm completion of file uploadupdateCitiesLastCollected();
              Debug.Log("Cities updated successfully");
            }, no => API.OnPlayFabError(no));
          }, errore => Debug.Log(errore));
      }, e => {
        // Abort and retry if something goes wrong
        if (e.Error == PlayFabErrorCode.EntityFileOperationPending) {
          PlayFabDataAPI.AbortFileUploads(new PlayFab.DataModels.AbortFileUploadsRequest {
            Entity = new PlayFab.DataModels.EntityKey { Id = mapEntityId, Type = "group" },
              FileNames = new List<string> { "citiesData" },
          }, res => UpdateCities(), err => API.OnPlayFabError(err));
        } else API.OnPlayFabError(e);
      });
  }
  static void updateCitiesLastCollected() {
    City[] cities = citiesUpdated;
    DateTime[, ] lastCollectedData = new DateTime[cities.Length, 3];
    for (int i = 0; i < cities.Length; i++) {
      DateTime[] lc = cities[i].getLastCollected();
      lastCollectedData[i, 0] = lc[0];
      lastCollectedData[i, 1] = lc[1];
      lastCollectedData[i, 2] = lc[2];
    }
    string mapId = controller.getUser().getMapId();
    // Get entity info
    if (mapEntityId == string.Empty) {
      GetMapEntityId(() => updateCitiesLastCollected());
      return;
    }
    // Initialise file upload
    PlayFabDataAPI.InitiateFileUploads(
      new PlayFab.DataModels.InitiateFileUploadsRequest {
        Entity = new PlayFab.DataModels.EntityKey { Id = mapEntityId, Type = "group" },
          FileNames = new List<string> { "lastCollectedData" }
      }, r => {
        // Serialize cities
        string dataSerialized = JsonConvert.SerializeObject(lastCollectedData, new JsonSerializerSettings {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        // Upload bytes as payload
        PlayFabHttp.SimplePutCall(r.UploadDetails[0].UploadUrl, Encoding.UTF8.GetBytes(dataSerialized),
          risposta => {
            PlayFabDataAPI.FinalizeFileUploads(new PlayFab.DataModels.FinalizeFileUploadsRequest {
              Entity = new PlayFab.DataModels.EntityKey { Id = mapEntityId, Type = "group" },
                FileNames = new List<string> { "lastCollectedData" },
            }, yes => {
              // Confirm completion of file upload
              Debug.Log("LastCollected data updated successfully");
            }, no => API.OnPlayFabError(no));
          }, errore => Debug.Log(errore));
      }, e => {
        // Abort and retry if something goes wrong
        if (e.Error == PlayFabErrorCode.EntityFileOperationPending) {
          PlayFabDataAPI.AbortFileUploads(new PlayFab.DataModels.AbortFileUploadsRequest {
            Entity = new PlayFab.DataModels.EntityKey { Id = mapEntityId, Type = "group" },
              FileNames = new List<string> { "lastCollectedData" },
          }, res => UpdateCities(true), err => API.OnPlayFabError(err));
        } else API.OnPlayFabError(e);
      });
  }

  //*****************************************************************
  // GET up to date information about cities on the map
  //*****************************************************************
  static string[] cities = null;
  static string lastCollected = null;
  public static void GetCities(string mapId, MapUser[] players) {
    if (mapEntityId == string.Empty) {
      GetMapEntityId(() => GetCities(mapId, players));
      return;
    }
    // Get the url needed to download the file
    PlayFabDataAPI.GetFiles(new PlayFab.DataModels.GetFilesRequest {
      Entity = new PlayFab.DataModels.EntityKey { Id = mapEntityId, Type = "group" }
    }, async r => {
      if (r.Metadata.Keys.Contains("citiesData") && r.Metadata.Keys.Contains("lastCollectedData")) {
        // Get cities data
        fetchCitiesFile(r.Metadata["citiesData"].DownloadUrl);
        // Get resources' last collected data
        fetchLastCollectedFile(r.Metadata["lastCollectedData"].DownloadUrl);
        // Check if both datasets are retrieved, if not wait.
        while (cities == null || lastCollected == null) {
          await Task.Delay(100);
        }
        // If both are received check their integrity
        if (cities.Length != 103 || lastCollected.Length < 9400) {
          // If integrity is compromised, retry
          GetCities(mapId, players);
          return;
        }
        // Otherwise, deserialize both datasets
        City[] citiesData = new City[cities.Length];
        List<string> errors = new List<string>();
        DateTime[, ] lastCollectedData = JsonConvert.DeserializeObject<DateTime[, ]>(lastCollected, new JsonSerializerSettings {
          Error = delegate(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) {
              errors.Add(args.ErrorContext.Error.Message);
              args.ErrorContext.Handled = true;
            },
            Converters = { new Newtonsoft.Json.Converters.IsoDateTimeConverter() }
        });
        foreach (string error in errors)Debug.Log(error);
        // Compile and store data
        char[] charsToTrim = new char[5] { '[', ']', '}', '{', '"' };
        for (int i = 0; i < citiesData.Length; i++) {
          // Then, for each city we separate all the info we have about it
          string[] city = cities[i].Split(',');
          // And we manually populate a City object with that info
          City cityObject = new City(city[0].Split(':')[1].Trim(charsToTrim));
          cityObject.setLevel(Int32.Parse(city[1].Split(':')[1].Trim(charsToTrim)));
          cityObject.setResources(new int[3] {
            Int32.Parse(city[2].Split(':')[1].Trim(charsToTrim)),
              Int32.Parse(city[3].Trim(charsToTrim)),
              Int32.Parse(city[4].Trim(charsToTrim))
          }, false);
          cityObject.setOwner(city[5].Split(':')[1].Trim(charsToTrim), false);
          cityObject.setCooldownEnd(DateTime.Parse(city[6].Split(new char[1] { ':' }, 2)[1].Trim(charsToTrim)), false);
          cityObject.setLastCollected(new DateTime[] { lastCollectedData[i, 0], lastCollectedData[i, 1], lastCollectedData[i, 2] });
          cityObject.sethpw();
          // And then store the newly created city in an array containing all cities
          citiesData[i] = cityObject;
        }
        // All this is then saved in the controller
        controller.setMap(new Map(mapId, players, citiesData));
      } else {
        Debug.Log("Cities can't be retrieved for this map. Creating new ones.");
        CreateNewCitiesOnServer();
      }
    }, e => API.OnPlayFabError(e));
  }
  public static async void fetchCitiesFile(string url) {
    UnityWebRequest www = UnityWebRequest.Get(url);
    www.SendWebRequest();
    int timeout = 10000;
    while (!www.isDone && timeout > 0) {
      if (www.isNetworkError || www.isHttpError) {
        OnPlayFabError(null);
        Debug.Log(www.error);
        return;
      }
      await Task.Delay(100);
      timeout = timeout - 100;
    }
    if (timeout > 0)cities = Encoding.UTF8.GetString(www.downloadHandler.data).Split(new [] { "}," }, StringSplitOptions.None);
  }
  public static async void fetchLastCollectedFile(string url) {
    UnityWebRequest www = UnityWebRequest.Get(url);
    www.SendWebRequest();
    int timeout = 10000;
    while (!www.isDone && timeout > 0) {
      if (www.isNetworkError || www.result == UnityWebRequest.Result.ProtocolError) {
        OnPlayFabError(null);
        Debug.Log(www.error);
        return;
      }
      await Task.Delay(100);
      timeout = timeout - 100;
    }
    if (timeout > 0)lastCollected = Encoding.UTF8.GetString(www.downloadHandler.data);
  }

  //*****************************************************************
  // SAVE the player's bounty, which is taken care separately (because of leaderboard)
  //*****************************************************************
  static int latestBountyValue = -1;
  public static void UpdateBounty(int value) {
    // Make the api call
    latestBountyValue = value;
    bountyDebouncer.onChange();
  }

  public static void debouncedUpdateBounty() {
    if (IsRegistered() && controller.getUser().getUsername() != null && latestBountyValue != -1) {
      PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest {
          Statistics = new List<StatisticUpdate> {
            new StatisticUpdate {
              StatisticName = "bounty", Value = latestBountyValue
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
    Debug.LogWarning("Something went wrong with your API call. " + error.GenerateErrorReport());
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