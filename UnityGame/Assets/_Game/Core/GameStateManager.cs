using System;
using UnityEngine;

namespace CricketGame.Core
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        private GameState currentState = GameState.Booting;
        private GameState previousState = GameState.Booting;

        public GameState CurrentState { get { return currentState; } }
        public GameState PreviousState { get { return previousState; } }

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
            if (currentState == newState) return;

            previousState = currentState;
            currentState = newState;

            Debug.Log(string.Format("[GameStateManager] Transition: {0} -> {1}", previousState, currentState));
            if (OnStateChanged != null)
            {
                OnStateChanged(previousState, currentState);
            }
        }

        public bool IsInMatch()
        {
            return currentState == GameState.PlayingMatch || 
                   currentState == GameState.MatchPaused || 
                   currentState == GameState.MatchLoading;
        }
    }
}
