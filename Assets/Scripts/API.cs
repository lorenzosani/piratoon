using UnityEngine;
using System;
using System.Linq;
using System.Globalization;
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

  static ControllerScript controller;
  static string playFabId;

  public static void RegisterScripts(ControllerScript c){
    controller = c;
  }

  public static void Login(){
    if (GetDeviceId()){
      if (!string.IsNullOrEmpty(androidId))
      {
        PlayFabClientAPI.LoginWithAndroidDeviceID(
          new LoginWithAndroidDeviceIDRequest
          {
            AndroidDeviceId = androidId,
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true
          }, 
          OnLogin, 
          OnPlayFabError
        );
      } else if (!string.IsNullOrEmpty(iosId))
      {
        PlayFabClientAPI.LoginWithIOSDeviceID(
          new LoginWithIOSDeviceIDRequest
          {
            DeviceId = iosId,
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true
          }, 
          OnLogin,
          OnPlayFabError
        );
      }
    } else {
      var newApiRequest = new LoginWithCustomIDRequest {
        CustomId = controller.getUser().getId(), 
        CreateAccount = true
      };
      PlayFabClientAPI.LoginWithCustomID(newApiRequest, OnLogin, OnPlayFabError);
    }
  }

  public static void OnLogin(LoginResult login)
  {
    playFabId = login.PlayFabId;
    List<string> keys = new List<string> { "User", "Buildings", "Village" };  
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
        Debug.Log("BUILDINGS: "+ result.Data["Buildings"].Value);
        Debug.Log("USER: "+ result.Data["User"].Value);
      } else {
        Debug.Log("API Error: fetched data is null.");
      }
    });
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
    }, error => OnPlayFabError(error));
  }

  public static void GetUserData(List<string> keys, Action<GetUserDataResult> callback){
    // Fetch user data
    PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
      PlayFabId = playFabId,
      Keys = keys
    }, result => { 
      // Check if user has already data
      callback(result.Data != null && result.Data.ContainsKey("User") ? result : null);
    }, OnPlayFabError);
  }

  // Get info about the current operating system
  public static bool GetDeviceId(bool silent = false) // silent suppresses the error
  {
    if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
    {
#if UNITY_ANDROID
      //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
      AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
      AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
      AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
      AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
      androidId = clsSecure.CallStatic<string>("getString", objResolver, "android_id");
#endif
#if UNITY_IPHONE
      iosId = UnityEngine.iOS.Device.vendorIdentifier;
#endif
      return true;
    }
    else
    {
      customId = controller.getUser().getId();
      return false;
    }
  }

  // If something goes wrong with the API
  public static void OnPlayFabError(PlayFabError error)
  {
    Debug.LogWarning("Something went wrong with your API call.");
    Debug.LogError("Here's some debug information:");
    Debug.LogError(error.GenerateErrorReport());
  }
}