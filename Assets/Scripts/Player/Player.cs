using Debug;
using UnityEngine;

namespace Player
{
    public class PlayerData
    {
        public int playerID { get; set; }
        public bool isLocallyControlled { get; set; }
    }

    public class Player : MonoBehaviour
    {
        public PlayerData playerData { get; protected set; }

        public void Init(bool isLocallyControlled)
        {
            //Log
            DebugSystem.Log($"Creating new player which {(isLocallyControlled ? "is" : "isn't")} locally controlled");
            //Setup
            playerData = new PlayerData
            {
                isLocallyControlled = isLocallyControlled
            };
            PlayerSystem.RegisterPlayer(this);
        }
    }
}