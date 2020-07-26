using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class AuthenticationScript : MonoBehaviour
{
  public Text description;
  public GameObject usernameField;
  public GameObject emailField;
  public GameObject passwordField;
  public GameObject repeatPasswordField;
  public GameObject registerButton;
  public GameObject spinner;
  public GameObject notLoggedInPage;
  public GameObject registrationPage;
  public GameObject onLoginPage;

  string username = null;
  string email = null;
  string password = null;
  string repeatPassword = null;

  void Start()
  {
    description.text = Language.Field["REGISTRATION"];
    if (API.IsRegistered() && !isEmpty(API.GetStoredPlayerId())) {
      notLoggedInPage.SetActive(false);
      registrationPage.SetActive(false);
      onLoginPage.SetActive(true);
    } else {
      onLoginPage.SetActive(false);
      registrationPage.SetActive(false);
      notLoggedInPage.SetActive(true);
    }
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

  public void register(){
    clearErrors();
    if (!fieldsAreValid()) return;
    registerButton.SetActive(false);
    spinner.SetActive(true);
    API.RegisterUser(username, email, password, (message, field) => registrationResult(message, field));
  }

  void setError(string message, GameObject field=null){
    description.text = message;
    Color red = Color.red;
    red.r = 1.0f;
    red.g = 0.66f;
    red.b = 0.66f;
    description.color = red;
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

  bool fieldsAreValid(){
    if (isEmpty(username)) {
      setError(Language.Field["REG_USERNAME"], usernameField);
      return false;
    }
    if (isEmpty(email)) {
      setError(Language.Field["REG_EMAIL"], emailField);
      return false;
    }
    if (isEmpty(password)) {
      setError(Language.Field["REG_PASSWORD"], passwordField);
      return false;
    }
    if (isEmpty(repeatPassword)) {
      setError(Language.Field["REG_REPEAT"], repeatPasswordField);
      return false;
    }
    if (password != repeatPassword) {
      setError(Language.Field["REG_MATCH"], repeatPasswordField);
      return false;
    }
    return true;
  }

  void registrationResult(string message, string inputField){
    spinner.SetActive(false);
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

  void populateAccountInfo(){
    Text onLoginText = onLoginPage.GetComponentInChildren(typeof(Text), true) as Text;
    onLoginText.text = "Hello " + API.GetUsername() + "!\nYou are now officially a pirate!";
  }

  bool isEmpty(string value){
    return value == "" || value == " " || value == null;
  }
}