using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {

  public GameObject loadingScreen;

  public void showLoadingScreen(bool show = true) {
    loadingScreen.SetActive(show);
  }
}