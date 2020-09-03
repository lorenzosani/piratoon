using UnityEngine;

namespace Crystal {
    public class SafeArea : MonoBehaviour {
        RectTransform Panel;
        Rect LastSafeArea = new Rect(0, 0, 0, 0);
        Vector2Int LastScreenSize = new Vector2Int(0, 0);
        ScreenOrientation LastOrientation = ScreenOrientation.AutoRotation;
        [SerializeField] bool ConformX = true; // Conform to screen safe area on X-axis (default true, disable to ignore)
        [SerializeField] bool ConformY = true; // Conform to screen safe area on Y-axis (default true, disable to ignore)

        void Awake() {
            Panel = GetComponent<RectTransform>();
            if (Panel == null) {
                Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
                Destroy(gameObject);
            }
            ApplySafeArea(GetSafeArea());
        }

        void Update() {
            Refresh();
        }

        void Refresh() {
            Rect safeArea = GetSafeArea();

            if (safeArea != LastSafeArea
                || Screen.width != LastScreenSize.x
                || Screen.height != LastScreenSize.y
                || Screen.orientation != LastOrientation) {
                LastScreenSize.x = Screen.width;
                LastScreenSize.y = Screen.height;
                LastOrientation = Screen.orientation;

                ApplySafeArea(safeArea);
            }
        }

        Rect GetSafeArea() {
            return Screen.safeArea;
        }

        void ApplySafeArea(Rect r) {
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
}