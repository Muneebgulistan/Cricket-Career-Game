using System;
using System.IO;
using UnityEngine;
using CricketGame.Career;

namespace CricketGame.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SaveFileName = "career_save.json";

        private string SaveFilePath
        {
            get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
        }

        public event Action OnCareerSaved;
        public event Action OnCareerLoaded;

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

        public bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }

        public bool SaveCareer(CareerProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[SaveManager] Cannot save null career profile.");
                return false;
            }

            try
            {
                string json = JsonUtility.ToJson(profile, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log(string.Format("[SaveManager] Career saved successfully to: {0}", SaveFilePath));
                if (OnCareerSaved != null) OnCareerSaved();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[SaveManager] Error saving career: {0}", ex.Message));
                return false;
            }
        }

        public CareerProfile LoadCareer()
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("[SaveManager] No career save file found.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                CareerProfile profile = JsonUtility.FromJson<CareerProfile>(json);
                string pName = (profile != null && profile.player != null) ? profile.player.name : "Unknown";
                Debug.Log(string.Format("[SaveManager] Career loaded successfully for player: {0}", pName));
                if (OnCareerLoaded != null) OnCareerLoaded();
                return profile;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[SaveManager] Failed to load career save file: {0}", ex.Message));
                return null;
            }
        }

        public bool DeleteSaveFile()
        {
            try
            {
                if (HasSaveFile())
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("[SaveManager] Save file deleted.");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[SaveManager] Error deleting save: {0}", ex.Message));
                return false;
            }
        }
    }
}
