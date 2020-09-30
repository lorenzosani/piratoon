using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SafeArea : MonoBehaviour {
    RectTransform Panel = null;
    Rect LastSafeArea = new Rect(0, 0, 0, 0);
    [SerializeField] bool ConformX = true; // Conform to screen safe area on X-axis (default true, disable to ignore)
    [SerializeField] bool ConformY = true; // Conform to screen safe area on Y-axis (default true, disable to ignore)

    void Start() {
        Panel = GetComponent<RectTransform>();
        applySafeArea(Screen.safeArea);
    }

    void Update() {
        if (Panel == null) {
            Panel = GetComponent<RectTransform>();
            applySafeArea(Screen.safeArea);
        }
    }

    void applySafeArea(Rect r) {
        LastSafeArea = r;
        // Ignore x-axis?
        if (!ConformX) {
            r.x = 0;
            r.width = Screen.width;
        }
        // Ignore y-axis?
        if (!ConformY) {
            r.y = 0;
            r.height = Screen.height;
        }
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