using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Global
{
    public class PlayerRegisteredPayload : EventArgs
    {
        public readonly Player.Player player;

        public PlayerRegisteredPayload(Player.Player player)
        {
            this.player = player;
        }
    }

    public static class EventsSystem
    {
    }


    public static class PlayerEvents
    {
        public static event EventHandler<PlayerRegisteredPayload> OnPlayerRegistered;

        public static void InvokeOnPlayerRegistered([CanBeNull] object sender, PlayerRegisteredPayload payload)
        {
            OnPlayerRegistered?.Invoke(sender, payload);
        }
    }
}