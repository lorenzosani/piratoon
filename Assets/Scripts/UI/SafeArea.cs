using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SafeArea : MonoBehaviour {
    RectTransform Panel = null;
    Rect LastSafeArea = new Rect(0, 0, 0, 0);

    void Start() {
        Panel = GetComponent<RectTransform>();
        applySafeArea(Screen.safeArea);
    }

    void Update() {
        if (Panel == null || Screen.safeArea != LastSafeArea) {
            Panel = GetComponent<RectTransform>();
            applySafeArea(Screen.safeArea);
        }
    }

    void applySafeArea(Rect r) {
        LastSafeArea = r;
        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        Panel.anchorMin = anchorMin;
        Panel.anchorMax = anchorMax;
    }
}