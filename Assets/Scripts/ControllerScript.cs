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
  ObserverScript observer;
  BuildingSpawner spawner;
  string userId;

  void Awake()
  {
    observer = GetComponent<ObserverScript>();
    spawner = GetComponent<BuildingSpawner>();
    user = new User("loren", "serverId", new Village(Vector3.zero, observer), observer);
    var newApiRequest = new LoginWithCustomIDRequest { CustomId = user.getId(), CreateAccount = true};
    PlayFabClientAPI.LoginWithCustomID(newApiRequest, OnLogin, OnPlayFabError);
    // TODO: add LoginWithAndroidDeviceID, LoginWithIOSDeviceID
  }

  //*****************************************************************
  // API communication methods
  //*****************************************************************
  void OnLogin(LoginResult login)
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
        user = JsonConvert.DeserializeObject<User>((string) result.Data["User"].Value);
        Village village = JsonConvert.DeserializeObject<Village>((string) result.Data["Village"].Value);
        user.attachObserver(observer);
        village.attachObserver(observer);
        user.setVillage(village);
        populateVillage(result.Data["Buildings"].Value);
      } else {
        // If no, update server with new default user data
        updateRemoteUserData();
      }
    }, OnPlayFabError);
  }

  // Update the user data on server
  public void updateRemoteUserData(){
    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
      Data = new Dictionary<string, string>() {
        {"Buildings", JsonConvert.SerializeObject(
          user.getVillage().getBuildings(),
          new JsonSerializerSettings
          {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
          })
        },
        {"Village", JsonConvert.SerializeObject(
          user.getVillage(),
          new JsonSerializerSettings
          {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
          })
        },
        {"User", JsonConvert.SerializeObject(
          user,
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

  void populateVillage(string buildingsJson){
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
      DateTime dateValue;
      DateTime.TryParseExact(
        (string) buildingsObject[i]["completionTime"],
        "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
        new CultureInfo("en-US"),
        DateTimeStyles.None,
        out dateValue
      );
      b.setCompletionTime(dateValue);
      b.setValue((int) buildingsObject[i]["value"]);
      b.setLocalStorage((int) buildingsObject[i]["localStorage"]);
      b.attachObserver(observer);
      buildingsList.Add(b);
      spawner.addBuildingFromServer(b);
    }
    village.setBuildingsFromList(buildingsList);
  }

  // If something goes wrong with the API
  void OnPlayFabError(PlayFabError error)
  {
    Debug.LogWarning("Something went wrong with your API call.");
    Debug.LogError("Here's some debug information:");
    Debug.LogError(error.GenerateErrorReport());
  }

  //*****************************************************************
  // GETTERS and SETTERS methods
  //*****************************************************************
  public User getUser()
  {
    return user;
  }
}