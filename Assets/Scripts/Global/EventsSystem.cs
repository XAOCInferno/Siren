using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Global
{

    public static class EventsSystem
    {
    }


    public static class PlayerEvents
    {
        //Player registered
        public class PlayerRegisteredPayload : EventArgs
        {
            public readonly Player.Player player;

            public PlayerRegisteredPayload(Player.Player player)
            {
                this.player = player;
            }
        }
        public static event EventHandler<PlayerRegisteredPayload> OnPlayerRegistered;
        public static void InvokeOnPlayerRegistered([CanBeNull] object sender, PlayerRegisteredPayload payload)
        {
            OnPlayerRegistered?.Invoke(sender, payload);
        }
    }


    public static class DeckEvents
    {
        //Draw Card
        public class DrawCardPayload : EventArgs
        {
            public readonly Player.Player player;
            public readonly Gameplay.ECardType cardToDrawType;

            public DrawCardPayload(Player.Player player, Gameplay.ECardType cardToDrawType)
            {
                this.player = player;
                this.cardToDrawType = cardToDrawType;
            }
        }
        public static event EventHandler<DrawCardPayload> OnDrawCard;
        public static void InvokeOnDrawCard([CanBeNull] object sender, DrawCardPayload payload)
        {
            OnDrawCard?.Invoke(sender, payload);
        }
    }
}