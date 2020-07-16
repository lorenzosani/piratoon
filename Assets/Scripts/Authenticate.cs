using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class Authenticate
{
  static string userId;
  static ControllerScript controller;
  static ObserverScript observer;

  public static void RegisterScripts(ControllerScript c, ObserverScript o){
    controller = c;
    observer = o;
  }

  public static void Login(){
    var newApiRequest = new LoginWithCustomIDRequest { CustomId = controller.getUser().getId(), CreateAccount = true};
    PlayFabClientAPI.LoginWithCustomID(newApiRequest, OnLogin, OnPlayFabError);
    // TODO: add LoginWithAndroidDeviceID, LoginWithIOSDeviceID
  }

  public static void OnLogin(LoginResult login)
  {
    userId = login.PlayFabId;
    // Fetch user data
    PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
        PlayFabId = userId,
        Keys = null
    }, result => { 
      // Check if user has already data
      if (result.Data != null && result.Data.ContainsKey("User"))
      { 
        // If yes, set the local user data to match the remote
        User user = JsonConvert.DeserializeObject<User>((string) result.Data["User"].Value);
        Village village = JsonConvert.DeserializeObject<Village>((string) result.Data["Village"].Value);
        user.attachObserver(observer);
        village.attachObserver(observer);
        user.setVillage(village);
        controller.setUser(user);
        controller.populateVillage(result.Data["Buildings"].Value);
        Debug.Log("BUILDINGS: "+result.Data["Buildings"].Value);
        Debug.Log("USER: "+result.Data["User"].Value);
      } else {
        // If no, update server with new default user data
        SetUserData();
      }
    }, OnPlayFabError);
  }

  public static void SetUserData(){
    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
      Data = new Dictionary<string, string>() {
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
      }
    }, result => {
      Debug.Log("Server-side data updated successfully.");
    }, error => OnPlayFabError(error));
  }

  // If something goes wrong with the API
  public static void OnPlayFabError(PlayFabError error)
  {
    Debug.LogWarning("Something went wrong with your API call.");
    Debug.LogError("Here's some debug information:");
    Debug.LogError(error.GenerateErrorReport());
  }
}