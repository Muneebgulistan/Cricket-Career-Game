using System.Collections.Generic;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Fielding;

namespace CricketGame.Players
{
    public class PlayerTeamAssignment : MonoBehaviour
    {
        public static PlayerTeamAssignment Instance { get; private set; }

        [Header("Team Kits")]
        public Color teamAColorPrimary = new Color(0.12f, 0.55f, 0.25f); // Career XI Green
        public Color teamAColorSecondary = new Color(0.95f, 0.8f, 0.15f); // Gold
        public Color teamBColorPrimary = new Color(0.15f, 0.25f, 0.65f); // Academy XI Navy
        public Color teamBColorSecondary = new Color(0.85f, 0.2f, 0.2f); // Red

        private List<PlayerProfile> teamARoster = new List<PlayerProfile>();
        private List<PlayerProfile> teamBRoster = new List<PlayerProfile>();

        public List<PlayerProfile> TeamARoster { get { return teamARoster; } }
        public List<PlayerProfile> TeamBRoster { get { return teamBRoster; } }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                GenerateFictionalTeams();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void GenerateFictionalTeams()
        {
            teamARoster.Clear();
            teamBRoster.Clear();

            // Team A: "Career XI"
            string[] teamANames = new string[]
            {
                "Zayn Malik", "Danyal Khan", "Farhan Tariq", "Bilal Ahmed",
                "Usman Riaz", "Haris Qasim", "Kamran Baig", "Saad Afridi",
                "Taimur Shah", "Hamza Javed", "Mustafa Ali"
            };

            PlayingRole[] teamARoles = new PlayingRole[]
            {
                PlayingRole.Batsman, PlayingRole.Batsman, PlayingRole.Batsman, PlayingRole.AllRounder,
                PlayingRole.AllRounder, PlayingRole.WicketKeeper, PlayingRole.Bowler, PlayingRole.Bowler,
                PlayingRole.Bowler, PlayingRole.Bowler, PlayingRole.Bowler
            };

            for (int i = 0; i < 11; i++)
            {
                PlayerProfile p = new PlayerProfile(teamANames[i], 19 + (i % 8), "Pakistan", teamARoles[i], BattingStyle.RightHand, BowlingStyle.RightArmFast);
                p.jerseyNumber = i + 1;
                p.currentTeam = "Career XI";
                if (i == 5) p.isWicketKeeper = true;
                if (i == 0) p.isCaptain = true;
                teamARoster.Add(p);
            }

            // Team B: "Academy XI"
            string[] teamBNames = new string[]
            {
                "Rohan Sethi", "Kiran Verma", "Arjun Nair", "Devansh Patel",
                "Vikram Joshi", "Samir Sen", "Nikhil Rao", "Kabir Roy",
                "Varun Bose", "Aditya Das", "Manish Mehra"
            };

            PlayingRole[] teamBRoles = new PlayingRole[]
            {
                PlayingRole.Batsman, PlayingRole.Batsman, PlayingRole.Batsman, PlayingRole.AllRounder,
                PlayingRole.AllRounder, PlayingRole.WicketKeeper, PlayingRole.Bowler, PlayingRole.Bowler,
                PlayingRole.Bowler, PlayingRole.Bowler, PlayingRole.Bowler
            };

            for (int i = 0; i < 11; i++)
            {
                PlayerProfile p = new PlayerProfile(teamBNames[i], 18 + (i % 7), "South Asia", teamBRoles[i], BattingStyle.RightHand, BowlingStyle.RightArmMedium);
                p.jerseyNumber = i + 1;
                p.currentTeam = "Academy XI";
                if (i == 5) p.isWicketKeeper = true;
                if (i == 0) p.isCaptain = true;
                teamBRoster.Add(p);
            }
        }

        public List<PlayerController> SpawnMatchPlayers(PlayerFactory factory, CricketPositionManager posManager)
        {
            List<PlayerController> spawned = new List<PlayerController>();
            if (posManager == null) return spawned;

            // 1. Striker (Batsman from Team A)
            Vector3 strikerPos = posManager.strikerPosition != null ? posManager.strikerPosition.position : new Vector3(0, 0, 9.5f);
            Quaternion strikerRot = Quaternion.Euler(0, 180f, 0);
            PlayerProfile strikerProfile = teamARoster.Count > 0 ? teamARoster[0] : new PlayerProfile();
            PlayerController striker = factory.CreatePlayer(strikerProfile, strikerPos, strikerRot, PlayerMatchRole.Batsman, true, false);
            striker.Visual.SetTeamColors(teamAColorPrimary, teamAColorSecondary);
            spawned.Add(striker);

            // 2. Non-Striker (Batsman from Team A)
            Vector3 nonStrikerPos = posManager.nonStrikerPosition != null ? posManager.nonStrikerPosition.position : new Vector3(0.8f, 0, -9.5f);
            Quaternion nonStrikerRot = Quaternion.Euler(0, 0, 0);
            PlayerProfile nonStrikerProfile = teamARoster.Count > 1 ? teamARoster[1] : new PlayerProfile();
            PlayerController nonStriker = factory.CreatePlayer(nonStrikerProfile, nonStrikerPos, nonStrikerRot, PlayerMatchRole.Batsman, false, true);
            nonStriker.Visual.SetTeamColors(teamAColorPrimary, teamAColorSecondary);
            spawned.Add(nonStriker);

            // 3. Bowler (Bowler from Team B)
            Vector3 bowlerPos = posManager.bowlerStartPosition != null ? posManager.bowlerStartPosition.position : new Vector3(0, 0, -22f);
            Quaternion bowlerRot = Quaternion.Euler(0, 0, 0);
            PlayerProfile bowlerProfile = teamBRoster.Count > 6 ? teamBRoster[6] : new PlayerProfile();
            PlayerController bowler = factory.CreatePlayer(bowlerProfile, bowlerPos, bowlerRot, PlayerMatchRole.Bowler, false, true);
            bowler.Visual.SetTeamColors(teamBColorPrimary, teamBColorSecondary);
            spawned.Add(bowler);

            // 4. WicketKeeper (Keeper from Team B)
            Vector3 keeperPos = posManager.wicketKeeperPosition != null ? posManager.wicketKeeperPosition.position : new Vector3(0, 0, 16f);
            Quaternion keeperRot = Quaternion.Euler(0, 180f, 0);
            PlayerProfile keeperProfile = teamBRoster.Count > 5 ? teamBRoster[5] : new PlayerProfile();
            PlayerController keeper = factory.CreatePlayer(keeperProfile, keeperPos, keeperRot, PlayerMatchRole.WicketKeeper, false, true);
            keeper.Visual.SetTeamColors(teamBColorPrimary, teamBColorSecondary);
            spawned.Add(keeper);

            // 5. Fielders from Team B (MidOff, MidOn, Point, Cover, SquareLeg, FineLeg, ThirdMan)
            string[] fielderSlots = new string[] { "MidOff", "MidOn", "Point", "Cover", "SquareLeg", "FineLeg", "ThirdMan" };
            int bIndex = 0;
            for (int i = 0; i < fielderSlots.Length; i++)
            {
                Transform anchor = posManager.GetFielderAnchor(fielderSlots[i]);
                Vector3 pos = anchor != null ? anchor.position : new Vector3(10f * (i - 3), 0, 0);
                Quaternion rot = Quaternion.LookRotation(strikerPos - pos);

                PlayerProfile fProfile = teamBRoster.Count > bIndex ? teamBRoster[bIndex] : new PlayerProfile();
                bIndex++;
                if (bIndex == 5 || bIndex == 6) bIndex += 2; // Skip keeper and bowler

                PlayerController fielder = factory.CreatePlayer(fProfile, pos, rot, PlayerMatchRole.Fielder, false, true);
                fielder.Visual.SetTeamColors(teamBColorPrimary, teamBColorSecondary);
                spawned.Add(fielder);
            }

            return spawned;
        }
    }
}
