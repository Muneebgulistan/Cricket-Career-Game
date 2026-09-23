using System;
using UnityEngine;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Running
{
    public class MobileRunningHUD : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject hudContainer;

        [Header("Controls")]
        [SerializeField] private RunButton runButton;
        [SerializeField] private StopRunButton stopRunButton;
        [SerializeField] private DiveButton diveButton;
        [SerializeField] private RunDecisionIndicator decisionIndicator;

        public RunButton RunButton { get { return runButton; } }
        public StopRunButton StopRunButton { get { return stopRunButton; } }
        public DiveButton DiveButton { get { return diveButton; } }
        public RunDecisionIndicator DecisionIndicator { get { return decisionIndicator; } }

        public void SetVisible(bool visible)
        {
            if (hudContainer != null)
            {
                hudContainer.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
