using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Running
{
    public class RunButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text label;

        public event Action OnRunClicked;

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
            if (OnRunClicked != null)
            {
                OnRunClicked();
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendRunRequest();
            }
        }

        public void SetRunCountLabel(int count)
        {
            if (label != null)
            {
                label.text = string.Format("RUN ({0})", count);
            }
        }
    }
}
