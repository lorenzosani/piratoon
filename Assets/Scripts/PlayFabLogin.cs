using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabLogin : MonoBehaviour
{
    public void Start()
    {
        var request = new LoginWithCustomIDRequest { CustomId = "loginId", CreateAccount = true};
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        // TODO: add LoginWithAndroidDeviceID, LoginWithIOSDeviceID
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successful login API call.");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your login API call.");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }
}