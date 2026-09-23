using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Running
{
    public class StopRunButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        public event Action OnStopClicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        public void HandleClicked()
        {
            if (OnStopClicked != null)
            {
                OnStopClicked();
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendCancelRunRequest();
            }
        }
    }
}
