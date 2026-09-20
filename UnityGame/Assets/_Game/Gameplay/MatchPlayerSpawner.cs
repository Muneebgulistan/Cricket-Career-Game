using System.Collections.Generic;
using UnityEngine;
using CricketGame.Fielding;
using CricketGame.Players;

namespace CricketGame.Gameplay
{
    public class MatchPlayerSpawner : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private CricketPositionManager positionManager;
        [SerializeField] private PlayerFactory playerFactory;
        [SerializeField] private PlayerTeamAssignment teamAssignment;

        [Header("Spawn Configuration")]
        [SerializeField] private bool autoSpawnOnStart = true;

        private List<PlayerController> activeMatchPlayers = new List<PlayerController>();

        public List<PlayerController> ActiveMatchPlayers
        {
            get { return activeMatchPlayers; }
        }

        private void Start()
        {
            if (autoSpawnOnStart)
            {
                SpawnMatchPlayers();
            }
        }

        public void SpawnMatchPlayers()
        {
            if (positionManager == null) positionManager = FindObjectOfType<CricketPositionManager>();
            if (playerFactory == null) playerFactory = FindObjectOfType<PlayerFactory>();
            if (teamAssignment == null) teamAssignment = FindObjectOfType<PlayerTeamAssignment>();

            if (playerFactory == null)
            {
                GameObject factoryObj = new GameObject("PlayerFactory");
                playerFactory = factoryObj.AddComponent<PlayerFactory>();
            }

            if (teamAssignment == null)
            {
                GameObject teamObj = new GameObject("PlayerTeamAssignment");
                teamAssignment = teamObj.AddComponent<PlayerTeamAssignment>();
                teamAssignment.GenerateFictionalTeams();
            }

            if (positionManager != null && playerFactory != null && teamAssignment != null)
            {
                activeMatchPlayers = teamAssignment.SpawnMatchPlayers(playerFactory, positionManager);
                Debug.Log(string.Format("[MatchPlayerSpawner] Successfully spawned {0} players into the stadium environment.", activeMatchPlayers.Count));
            }
        }
    }
}
