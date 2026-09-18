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
                Debug.Log($"[SaveManager] Career saved successfully to: {SaveFilePath}");
                OnCareerSaved?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Error saving career: {ex.Message}");
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
                Debug.Log($"[SaveManager] Career loaded successfully for player: {profile.player?.name}");
                OnCareerLoaded?.Invoke();
                return profile;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to load career save file: {ex.Message}");
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
                Debug.LogError($"[SaveManager] Error deleting save: {ex.Message}");
                return false;
            }
        }
    }
}
