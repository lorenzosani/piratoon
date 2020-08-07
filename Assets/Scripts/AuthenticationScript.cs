using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class AuthenticationScript : MonoBehaviour
{
  public Text description;
  public Text loginDescription;
  public GameObject usernameField;
  public GameObject emailField;
  public GameObject passwordField;
  public GameObject repeatPasswordField;
  public GameObject loginUsernameField;
  public GameObject loginPasswordField;
  public GameObject registerButton;
  public GameObject loginButton;
  public GameObject registrationSpinner;
  public GameObject loginSpinner;
  public GameObject notLoggedInPage;
  public GameObject registrationPage;
  public GameObject loginPage;
  public GameObject onLoginPage;
  public GameObject recoveryPage;

  string username = null;
  string email = null;
  string password = null;
  string repeatPassword = null;

  void Start()
  {
    description.text = Language.Field["REGISTRATION"];
    loginDescription.text = Language.Field["LOGIN"];
    resetAccountMenu();
  }

  public void resetAccountMenu(){
    if (API.IsRegistered() && !isEmpty(API.GetStoredPlayerId())) {
      notLoggedInPage.SetActive(false);
      registrationPage.SetActive(false);
      loginPage.SetActive(false);
      onLoginPage.SetActive(true);
      recoveryPage.SetActive(false);
    } else {
      onLoginPage.SetActive(false);
      registrationPage.SetActive(false);
      loginPage.SetActive(false);
      notLoggedInPage.SetActive(true);
      recoveryPage.SetActive(false);
    }
  }

  public void register(){
    clearErrors();
    if (!fieldsAreValid(true,true,true,true)) return;
    registerButton.SetActive(false);
    registrationSpinner.SetActive(true);
    API.RegisterUser(username, email, password, (message, field) => registrationResult(message, field));
  }

  public void login(){
    clearErrors();
    if (!fieldsAreValid(true,false,true,false)) return;
    loginButton.SetActive(false);
    loginSpinner.SetActive(true);
    API.UsernameLogin(username, password, message => loginResult(message));
  }

  public void logout(){
    API.StorePlayerId("");
    API.StoreRegistered(false);
    API.StoreUsername("");
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  public void recoverPassword() {
    if (email == null) {
      GetComponent<ControllerScript>().getUI().setEmailRecoveryText(Language.Field["REG_EMAIL"]);
      return;
    }
    API.SendPasswordRecoveryEmail(email);
  }

  public void setUsername(string u){
    username = u;
  }

  public void setEmail(string e){
    email = e;
  }

  public void setPassword(string p){
    password = p;
  }

  public void setRepeatPassword(string r){
    repeatPassword = r;
  }

  void setError(string message, GameObject field=null, bool login=false){
    Text descr = login ? loginDescription : description;
    descr.text = message;
    Color red = Color.red;
    red.r = 1.0f;
    red.g = 0.66f;
    red.b = 0.66f;
    descr.color = red;
    if (!field) return;
    Outline outline = field.GetComponent<Outline>();
    outline.enabled = true;
  }

  void clearErrors(){
    description.text = Language.Field["REGISTRATION"];
    description.color = Color.white;
    Outline usernameOutline = usernameField.GetComponent<Outline>();
    Outline emailOutline = emailField.GetComponent<Outline>();
    Outline passwordOutline = passwordField.GetComponent<Outline>();
    Outline repeatPasswordOutline = repeatPasswordField.GetComponent<Outline>();
    usernameOutline.enabled = false;
    emailOutline.enabled = false;
    passwordOutline.enabled = false;
    repeatPasswordOutline.enabled = false;
  }

  bool fieldsAreValid(bool u, bool e, bool p, bool r){
    if (u && isEmpty(username)) {
      setError(Language.Field["REG_USERNAME"], usernameField);
      return false;
    }
    if (u && (username.Length<3 || username.Length>13)){
      setError(Language.Field["REG_USERLENGTH"], usernameField);
      return false;
    }
    if (e && isEmpty(email)) {
      setError(Language.Field["REG_EMAIL"], emailField);
      return false;
    }
    if (p && isEmpty(password)) {
      setError(Language.Field["REG_PASSWORD"], passwordField);
      return false;
    }
    if (r && isEmpty(repeatPassword)) {
      setError(Language.Field["REG_REPEAT"], repeatPasswordField);
      return false;
    }
    if (p && r && password != repeatPassword) {
      setError(Language.Field["REG_MATCH"], repeatPasswordField);
      return false;
    }
    return true;
  }

  void registrationResult(string message, string inputField){
    registrationSpinner.SetActive(false);
    registerButton.SetActive(true);
    if (message == "SUCCESS") {
      registrationPage.SetActive(false);
      populateAccountInfo();
      onLoginPage.SetActive(true);
      return;
    } else {
      GameObject field = null;
      if (inputField == "Username") field = usernameField;
      if (inputField == "Email") field = emailField;
      if (inputField == "Password") field = passwordField;
      setError(message, field);
    }
  }

  void loginResult(string message){
    loginSpinner.SetActive(false);
    loginButton.SetActive(true);
    if (message == "SUCCESS") {
      loginPage.SetActive(false);
      populateAccountInfo();
      onLoginPage.SetActive(true);
    } else {
      setError(message, null, true);
    }
  }

  void populateAccountInfo(){
    Text onLoginText = onLoginPage.GetComponentInChildren(typeof(Text), true) as Text;
    onLoginText.text = "Hello " + API.GetUsername() + "!";
  }

  bool isEmpty(string value){
    return value == "" || value == " " || value == null;
  }
}