using UnityEngine;

namespace CricketGame.Stadiums
{
    public class AdvertisingBoard : MonoBehaviour
    {
        public static readonly string[] FictionalBrands = new string[]
        {
            "CRICKET CAREER",
            "CC SPORTS",
            "WORLD CRICKET",
            "CAREER CUP"
        };

        [Header("Brand Info")]
        [SerializeField] private string brandName = "CRICKET CAREER";
        [SerializeField] private int boardIndex = 0;

        public string BrandName
        {
            get { return brandName; }
        }

        public void InitializeBoard(int index, Vector3 position, Quaternion rotation)
        {
            boardIndex = index;
            brandName = FictionalBrands[index % FictionalBrands.Length];
            transform.position = position;
            transform.rotation = rotation;
            name = string.Format("AdBoard_{0}_{1}", index, brandName.Replace(" ", "_"));
        }
    }
}
