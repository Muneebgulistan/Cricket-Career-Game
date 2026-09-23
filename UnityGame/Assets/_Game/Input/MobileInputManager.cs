using System;
using UnityEngine;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Fielding;
using CricketGame.Gameplay.Running;
using CricketGame.Gameplay.Match;

namespace CricketGame.MobileInput
{
    public class MobileInputManager : MonoBehaviour
    {
        public static MobileInputManager Instance { get; private set; }

        [Header("Mode & Settings")]
        [SerializeField] private MobileInputMode inputMode = MobileInputMode.Auto;
        [SerializeField] private MobileInputSettings settings;

        [Header("Component References")]
        [SerializeField] private BattingInput battingInput;
        [SerializeField] private BowlingInput bowlingInput;
        [SerializeField] private FieldingInput fieldingInput;
        [SerializeField] private RunningInput runningInput;
        [SerializeField] private MobileTouchInput touchInput;

        private MobileInputRouter router;

        public MobileInputMode CurrentMode { get { return inputMode; } }
        public MobileInputSettings Settings { get { return settings; } }
        public MobileInputRouter Router { get { return router; } }
        public MobileTouchInput TouchInput { get { return touchInput; } }

        public event Action<MobileInputMode> OnInputModeChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (settings == null)
            {
                settings = MobileInputSettings.Default();
            }

            ResolveDependencies();
            InitializeRouter();
        }

        private void Start()
        {
            ApplyEffectiveMode();
            HookTouchGestures();
        }

        public void SetInputMode(MobileInputMode mode)
        {
            inputMode = mode;
            ApplyEffectiveMode();
            if (OnInputModeChanged != null)
            {
                OnInputModeChanged(inputMode);
            }
        }

        public bool IsTouchActive()
        {
            if (inputMode == MobileInputMode.Touch) return true;
            if (inputMode == MobileInputMode.Keyboard) return false;

            // Auto mode: check platform
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        private void ResolveDependencies()
        {
            if (battingInput == null) battingInput = FindObjectOfType<BattingInput>();
            if (bowlingInput == null) bowlingInput = FindObjectOfType<BowlingInput>();
            if (fieldingInput == null) fieldingInput = FindObjectOfType<FieldingInput>();
            if (runningInput == null) runningInput = FindObjectOfType<RunningInput>();

            if (touchInput == null)
            {
                touchInput = GetComponent<MobileTouchInput>();
                if (touchInput == null)
                {
                    touchInput = gameObject.AddComponent<MobileTouchInput>();
                }
            }
            if (touchInput != null)
            {
                touchInput.Initialize(settings);
            }
        }

        private void InitializeRouter()
        {
            router = new MobileInputRouter(battingInput, bowlingInput, fieldingInput, runningInput);
        }

        private void ApplyEffectiveMode()
        {
            // If touch is active, polling keyboard is optional or disabled in HUDs
        }

        private void HookTouchGestures()
        {
            if (touchInput != null)
            {
                touchInput.OnSwipeDetected += HandleSwipeGesture;
                touchInput.OnTapDetected += HandleTapGesture;
            }
        }

        private void HandleSwipeGesture(SwipeDirection direction, Vector2 normalizedDir)
        {
            if (router == null) return;

            bool isLofted;
            bool isDefensive;
            BattingShotType shot = touchInput.MapSwipeToBattingShot(direction, out isLofted, out isDefensive);
            router.SendDirectShot(shot, normalizedDir, isLofted);
        }

        private void HandleTapGesture(Vector2 screenPos)
        {
            if (router == null) return;
            // Tap triggers standard swing with currently selected shot/direction
            router.SendBattingSwing();
        }
    }
}
