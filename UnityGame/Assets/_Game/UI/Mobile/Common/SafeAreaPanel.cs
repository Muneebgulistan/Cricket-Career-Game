using System;
using UnityEngine;

namespace CricketGame.UI.Mobile.Common
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform panelRectTransform;
        private Rect lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2Int lastScreenSize = new Vector2Int(0, 0);
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        [SerializeField] private bool conformX = true;
        [SerializeField] private bool conformY = true;

        private void Awake()
        {
            panelRectTransform = GetComponent<RectTransform>();
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            ScreenOrientation orientation = Screen.orientation;

            if (safeArea != lastSafeArea || screenSize != lastScreenSize || orientation != lastOrientation)
            {
                lastSafeArea = safeArea;
                lastScreenSize = screenSize;
                lastOrientation = orientation;
                ApplySafeArea(safeArea);
            }
        }

        public void ApplySafeArea(Rect safeArea)
        {
            if (panelRectTransform == null)
            {
                panelRectTransform = GetComponent<RectTransform>();
            }

            if (panelRectTransform == null) return;

            if (Screen.width <= 0 || Screen.height <= 0) return;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            if (!conformX)
            {
                anchorMin.x = 0;
                anchorMax.x = 1;
            }

            if (!conformY)
            {
                anchorMin.y = 0;
                anchorMax.y = 1;
            }

            panelRectTransform.anchorMin = anchorMin;
            panelRectTransform.anchorMax = anchorMax;
        }

        public Rect GetLastSafeArea()
        {
            return lastSafeArea;
        }
    }
}
