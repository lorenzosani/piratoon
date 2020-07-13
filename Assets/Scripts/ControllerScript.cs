using UnityEngine;
using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

public class ControllerScript : MonoBehaviour
{
  private User user = new User("newId", "serverId", new Village(Vector3.zero));

  void Awake()
  {
    var request = new LoginWithCustomIDRequest { CustomId = user.getId(), CreateAccount = true};
    PlayFabClientAPI.LoginWithCustomID(request, OnLogin, OnPlayFabError);
    // TODO: add LoginWithAndroidDeviceID, LoginWithIOSDeviceID
  }

  public User getUser()
  {
    return user;
  }

  void OnLogin(LoginResult result)
  {
    // Get user id
    string entityId = result.EntityToken.Entity.Id;
    string entityType = result.EntityToken.Entity.Type;
    // Fetch user data
    var getRequest = new PlayFab.DataModels.GetObjectsRequest {
      Entity = new PlayFab.DataModels.EntityKey {
        Id = entityId, 
        Type = entityType
      }
    };
    // Check if user has already data
    PlayFabDataAPI.GetObjects(getRequest, r => { 
      if(r.Objects.Values.Count > 0) { 
        setUserData(r.Objects["UserData"]);
      } else {
        newUserData(entityId, entityType);
      }
    }, OnPlayFabError);
  }

  void setUserData(PlayFab.DataModels.ObjectResult obj){
    user = JsonConvert.DeserializeObject<User>((string) obj.DataObject);
  }

  void newUserData(string entityId, string entityType){
    var dataList = new List<PlayFab.DataModels.SetObject>()
    {
      new PlayFab.DataModels.SetObject()
      {
        ObjectName = "UserData",
        DataObject = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        })
      }
    };
    PlayFabDataAPI.SetObjects(new PlayFab.DataModels.SetObjectsRequest()
    {
      Entity = new PlayFab.DataModels.EntityKey {Id = entityId, Type = entityType},
      Objects = dataList,
    }, (setResult) => {
      Debug.Log(setResult.ProfileVersion);
    }, OnPlayFabError);
  }

  void OnPlayFabError(PlayFabError error)
  {
    Debug.LogWarning("Something went wrong with your API call.");
    Debug.LogError("Here's some debug information:");
    Debug.LogError(error.GenerateErrorReport());
  }
}