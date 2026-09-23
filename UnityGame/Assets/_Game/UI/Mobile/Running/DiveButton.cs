using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Running
{
    public class DiveButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        public event Action OnDiveClicked;

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
            if (OnDiveClicked != null)
            {
                OnDiveClicked();
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendDiveRequest();
            }
        }
    }
}
