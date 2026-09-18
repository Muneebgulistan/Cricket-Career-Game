using System;
using UnityEngine;

namespace CricketGame.Camera
{
    public class CricketCameraManager : MonoBehaviour
    {
        public static CricketCameraManager Instance { get; private set; }

        [Header("Active Camera Reference (Single Camera)")]
        [SerializeField] private UnityEngine.Camera gameplayCamera;

        [Header("Current Mode")]
        [SerializeField] private CricketCameraMode currentMode = CricketCameraMode.BroadcastCamera;

        [Header("Camera Transform Presets")]
        public Transform broadcastCameraAnchor;
        public Transform battingCameraAnchor;
        public Transform bowlingCameraAnchor;
        public Transform fieldCameraAnchor;
        public Transform wicketCameraAnchor;

        [Header("Transition Settings")]
        [SerializeField] private float transitionSpeed = 8.0f;
        [SerializeField] private bool smoothTransitions = true;

        [Header("Target Following Foundation")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 followOffset = new Vector3(0, 3.5f, -10f);

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private float targetFOV = 60f;

        public CricketCameraMode CurrentMode
        {
            get { return currentMode; }
        }

        public UnityEngine.Camera ActiveCamera
        {
            get { return gameplayCamera; }
        }

        public event Action<CricketCameraMode> OnCameraModeChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = UnityEngine.Camera.main;
            }
        }

        private void Start()
        {
            ApplyCameraModeImmediate(currentMode);
        }

        private void Update()
        {
            HandleEditorKeyboardSwitching();
            UpdateCameraMovement();
        }

        private void HandleEditorKeyboardSwitching()
        {
            // Keyboard controls for rapid testing
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchCameraMode(CricketCameraMode.BroadcastCamera);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchCameraMode(CricketCameraMode.BattingCamera);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                SwitchCameraMode(CricketCameraMode.BowlingCamera);
            }
        }

        public void SwitchCameraMode(CricketCameraMode newMode)
        {
            if (currentMode == newMode && !smoothTransitions) return;

            currentMode = newMode;
            UpdateTargetTransform();
            if (OnCameraModeChanged != null)
            {
                OnCameraModeChanged(newMode);
            }
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        private void UpdateTargetTransform()
        {
            Transform anchor = null;

            switch (currentMode)
            {
                case CricketCameraMode.BroadcastCamera:
                    anchor = broadcastCameraAnchor;
                    targetFOV = 55f;
                    break;
                case CricketCameraMode.BattingCamera:
                    anchor = battingCameraAnchor;
                    targetFOV = 60f;
                    break;
                case CricketCameraMode.BowlingCamera:
                    anchor = bowlingCameraAnchor;
                    targetFOV = 60f;
                    break;
                case CricketCameraMode.FieldCamera:
                    anchor = fieldCameraAnchor;
                    targetFOV = 65f;
                    break;
                case CricketCameraMode.WicketCamera:
                    anchor = wicketCameraAnchor;
                    targetFOV = 50f;
                    break;
            }

            if (anchor != null)
            {
                targetPosition = anchor.position;
                targetRotation = anchor.rotation;
            }
            else
            {
                // Fallback default coordinates if anchor is unset
                SetDefaultPresetCoordinates(currentMode);
            }
        }

        private void SetDefaultPresetCoordinates(CricketCameraMode mode)
        {
            switch (mode)
            {
                case CricketCameraMode.BroadcastCamera:
                    targetPosition = new Vector3(0f, 14f, -28f);
                    targetRotation = Quaternion.Euler(22f, 0f, 0f);
                    break;
                case CricketCameraMode.BattingCamera:
                    targetPosition = new Vector3(0.4f, 2.2f, 14.5f);
                    targetRotation = Quaternion.Euler(8f, 180f, 0f);
                    break;
                case CricketCameraMode.BowlingCamera:
                    targetPosition = new Vector3(0f, 3.0f, -18f);
                    targetRotation = Quaternion.Euler(8f, 0f, 0f);
                    break;
            }
        }

        private void ApplyCameraModeImmediate(CricketCameraMode mode)
        {
            currentMode = mode;
            UpdateTargetTransform();

            if (gameplayCamera != null)
            {
                gameplayCamera.transform.position = targetPosition;
                gameplayCamera.transform.rotation = targetRotation;
                gameplayCamera.fieldOfView = targetFOV;
            }
        }

        private void UpdateCameraMovement()
        {
            if (gameplayCamera == null) return;

            if (followTarget != null)
            {
                Vector3 desiredPos = followTarget.position + followOffset;
                gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, desiredPos, Time.deltaTime * transitionSpeed);
                gameplayCamera.transform.LookAt(followTarget.position);
                return;
            }

            if (smoothTransitions)
            {
                gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, targetPosition, Time.deltaTime * transitionSpeed);
                gameplayCamera.transform.rotation = Quaternion.Slerp(gameplayCamera.transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
                gameplayCamera.fieldOfView = Mathf.Lerp(gameplayCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
            }
            else
            {
                gameplayCamera.transform.position = targetPosition;
                gameplayCamera.transform.rotation = targetRotation;
                gameplayCamera.fieldOfView = targetFOV;
            }
        }
    }
}
