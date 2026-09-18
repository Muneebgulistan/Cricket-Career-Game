using System;
using UnityEngine;

namespace CricketGame.Core
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Booting;
        public GameState PreviousState { get; private set; } = GameState.Booting;

        public event Action<GameState, GameState> OnStateChanged;

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
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            PreviousState = CurrentState;
            CurrentState = newState;

            Debug.Log($"[GameStateManager] Transition: {PreviousState} -> {CurrentState}");
            OnStateChanged?.Invoke(PreviousState, CurrentState);
        }

        public bool IsInMatch()
        {
            return CurrentState == GameState.PlayingMatch || 
                   CurrentState == GameState.MatchPaused || 
                   CurrentState == GameState.MatchLoading;
        }
    }
}
