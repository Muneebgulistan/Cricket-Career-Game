using UnityEngine;

namespace CricketGame.Players
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public PlayerMatchRole defaultRole = PlayerMatchRole.Fielder;
        public int teamIndex = 0; // 0: Home / Bowling, 1: Away / Batting
        public string positionName = "FieldPosition";

        private void OnDrawGizmos()
        {
            Gizmos.color = defaultRole == PlayerMatchRole.Batsman ? Color.blue :
                           (defaultRole == PlayerMatchRole.Bowler ? Color.red :
                           (defaultRole == PlayerMatchRole.WicketKeeper ? Color.yellow : Color.green));

            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.4f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.0f);
        }
    }
}
