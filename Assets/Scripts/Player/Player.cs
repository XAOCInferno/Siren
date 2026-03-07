using Debug;
using UnityEngine;

namespace Player
{
    public class PlayerData
    {
        public int playerID { get; set; }
        private bool _isLocallyControlled;

        public void SetIsLocallyControlled(bool isLocallyControlled)
        {
            _isLocallyControlled = isLocallyControlled;
        }
    }

    public class Player : MonoBehaviour
    {
        public PlayerData playerData { get; protected set; }

        public void Init(bool isLocallyControlled)
        {
            //Log
            DebugSystem.Log($"Creating new player which {(isLocallyControlled ? "is" : "isn't")} locally controlled");
            //Setup
            playerData = new PlayerData();
            playerData.SetIsLocallyControlled(isLocallyControlled);
            PlayerSystem.RegisterPlayer(this);
        }
    }
}