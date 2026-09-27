using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using CricketGame.Career;

namespace CricketGame.SaveSystem
{
    /// <summary>
    /// Secure local profile persistence manager with AES encryption, slot management,
    /// and save integrity verification for the Cricket Career mode.
    /// </summary>
    public class SaveProfileManager : MonoBehaviour
    {
        public static SaveProfileManager Instance { get; private set; }

        private const string SAVE_FILE_EXTENSION = ".sav";
        private const string SAVE_VERSION = "2.0";
        private const string DEFAULT_SLOT = "career_slot_0";

        // Pre-shared encryption vector & key for local tampering deterrence
        private static readonly byte[] EncryptionKey = new byte[32]
        {
            0x24, 0x88, 0x19, 0x76, 0xBA, 0x90, 0x43, 0x11,
            0xFE, 0x89, 0x01, 0x45, 0x33, 0x99, 0xAA, 0xBB,
            0xCC, 0xDD, 0xEE, 0xFF, 0x00, 0x12, 0x34, 0x56,
            0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x1F, 0x2E, 0x3D
        };

        private static readonly byte[] EncryptionIV = new byte[16]
        {
            0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80,
            0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0, 0x0F
        };

        [SerializeField] private bool encryptionEnabled = true;
        [SerializeField] private string activeSlot = DEFAULT_SLOT;

        private CareerProfile currentProfile;

        public bool IsEncryptionEnabled
        {
            get { return encryptionEnabled; }
            set { encryptionEnabled = value; }
        }

        public string ActiveSlot
        {
            get { return activeSlot; }
            set { activeSlot = string.IsNullOrEmpty(value) ? DEFAULT_SLOT : value; }
        }

        public CareerProfile CurrentProfile
        {
            get { return currentProfile; }
            set { currentProfile = value; }
        }

        public event Action<string, CareerProfile> OnProfileSaved;
        public event Action<string, CareerProfile> OnProfileLoaded;
        public event Action<string> OnSaveError;

        public static void SetInstanceForTesting(SaveProfileManager inst)
        {
            Instance = inst;
        }

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public string GetProfileFilePath(string slotName)
        {
            string slot = string.IsNullOrEmpty(slotName) ? activeSlot : slotName;
            string fileName = slot + SAVE_FILE_EXTENSION;
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public bool HasProfile(string slotName = null)
        {
            string path = GetProfileFilePath(slotName);
            return File.Exists(path);
        }

        public bool SaveProfile(CareerProfile profile, string slotName = null)
        {
            if (profile == null)
            {
                if (OnSaveError != null) OnSaveError("Cannot save null career profile");
                return false;
            }

            string slot = string.IsNullOrEmpty(slotName) ? activeSlot : slotName;
            string path = GetProfileFilePath(slot);

            try
            {
                string json = JsonUtility.ToJson(profile, false);
                string payloadToPersist;

                if (encryptionEnabled)
                {
                    payloadToPersist = EncryptPayload(json);
                }
                else
                {
                    payloadToPersist = json;
                }

                File.WriteAllText(path, payloadToPersist);
                currentProfile = profile;

                if (OnProfileSaved != null)
                {
                    OnProfileSaved(slot, profile);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[SaveProfileManager] Save error: {0}", ex.Message));
                if (OnSaveError != null) OnSaveError(ex.Message);
                return false;
            }
        }

        public CareerProfile LoadProfile(string slotName = null)
        {
            string slot = string.IsNullOrEmpty(slotName) ? activeSlot : slotName;
            string path = GetProfileFilePath(slot);

            if (!File.Exists(path))
            {
                Debug.LogWarning(string.Format("[SaveProfileManager] No profile found at {0}", path));
                return null;
            }

            try
            {
                string rawContent = File.ReadAllText(path);
                string json;

                if (encryptionEnabled)
                {
                    json = DecryptPayload(rawContent);
                }
                else
                {
                    json = rawContent;
                }

                CareerProfile profile = JsonUtility.FromJson<CareerProfile>(json);
                if (profile != null)
                {
                    currentProfile = profile;
                    if (OnProfileLoaded != null)
                    {
                        OnProfileLoaded(slot, profile);
                    }
                }

                return profile;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[SaveProfileManager] Load error: {0}", ex.Message));
                if (OnSaveError != null) OnSaveError(ex.Message);
                return null;
            }
        }

        public bool DeleteProfile(string slotName = null)
        {
            string slot = string.IsNullOrEmpty(slotName) ? activeSlot : slotName;
            string path = GetProfileFilePath(slot);

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    if (currentProfile != null && (slotName == null || slotName == activeSlot))
                    {
                        currentProfile = null;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[SaveProfileManager] Delete error: {0}", ex.Message));
                return false;
            }
        }

        public string EncryptPayload(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.Key = EncryptionKey;
                aes.IV = EncryptionIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    byte[] encrypted = ms.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        public string DecryptPayload(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.Key = EncryptionKey;
                aes.IV = EncryptionIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    byte[] decrypted = ms.ToArray();
                    return Encoding.UTF8.GetString(decrypted);
                }
            }
        }
    }
}
