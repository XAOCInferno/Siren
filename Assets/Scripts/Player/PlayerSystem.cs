using System.Collections.Generic;
using Debug;
using Global;
using JetBrains.Annotations;

namespace Player
{
    //A system to handle players, register them and communicate them
    public static class PlayerSystem
    {
        private static readonly List<Player> Players = new();

        public static int RegisterPlayer(Player player)
        {
            //Don't register if we're already registered, log error if we tried
            if (Players.Contains(player))
            {
                DebugSystem.Error($"Attempting to register player {player} who is already registered.");
                return Players.Find((v) => v == player).playerData.playerID;
            }

            //Set data
            Players.Add(player);
            player.playerData.playerID = Players.Count;
            //Log success
            DebugSystem.Log($"Successfully registered player {player}. Broadcasting.");
            //Broadcast
            PlayerEvents.InvokeOnPlayerRegistered(null, new PlayerRegisteredPayload(player));
            return player.playerData.playerID;
        }

        [CanBeNull]
        public static Player TryGetPlayerFromID(int playerID)
        {
            return Players.Find((v) => v.playerData.playerID == playerID);
        }
    }
}