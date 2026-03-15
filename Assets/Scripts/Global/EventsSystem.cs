using System;
using Gameplay;
using JetBrains.Annotations;
using UnityEngine;

namespace Global
{
    public static class EventsSystem
    {
    }

    public static class DatabaseEvents
    {
        //Loaded card template and set up pool
        public class DatabaseSetupPayload : EventArgs
        {
            public readonly Type DatabaseType;

            public DatabaseSetupPayload(Type type)
            {
                DatabaseType = type;
            }
        }

        public static event EventHandler<DatabaseSetupPayload> OnDatabaseSet;

        public static void InvokeOnPoolSetup([CanBeNull] object sender, DatabaseSetupPayload payload)
        {
            OnDatabaseSet?.Invoke(sender, payload);
        }
    }

    public static class BoardEvents
    {
        //Spawn a piece
        public class OrderPlacePieceOnBoardPayload : EventArgs
        {
            public readonly PieceData pieceData;
            public readonly Vector2Int gridCoordinates;

            public OrderPlacePieceOnBoardPayload(PieceData pieceData, Vector2Int gridCoordinates)
            {
                this.pieceData = pieceData;
                this.gridCoordinates = gridCoordinates;
            }
        }

        public static event EventHandler<OrderPlacePieceOnBoardPayload> OnOrderPlacePieceOnBoard;

        public static void InvokeOnOrderPlacePieceOnBoard([CanBeNull] object sender,
            OrderPlacePieceOnBoardPayload payload)
        {
            OnOrderPlacePieceOnBoard?.Invoke(sender, payload);
        }
    }

    public static class PoolEvents
    {
        //Loaded card template and set up pool
        public class PoolSetupPayload : EventArgs
        {
            public readonly Type PoolType;

            public PoolSetupPayload(Type type)
            {
                PoolType = type;
            }
        }

        public static event EventHandler<PoolSetupPayload> OnPoolSetup;

        public static void InvokeOnPoolSetup([CanBeNull] object sender, PoolSetupPayload payload)
        {
            OnPoolSetup?.Invoke(sender, payload);
        }
    }

    public static class PlayerEvents
    {
        //Player registered
        public class PlayerRegisteredPayload : EventArgs
        {
            public readonly Player.Player RegisteredPlayer;

            public PlayerRegisteredPayload(Player.Player registeredPlayer)
            {
                this.RegisteredPlayer = registeredPlayer;
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
            public readonly Player.Player Player;
            public readonly Gameplay.EDrawFromDeckOption DrawOption;
            public readonly ECardType? CardToDrawType;
            public readonly int? SpecificCardToDrawIndex;

            public DrawCardPayload(Player.Player player, Gameplay.EDrawFromDeckOption drawOption,
                ECardType? cardToDrawType = null, int? specificCardToDrawIndex = null)
            {
                this.Player = player;
                this.DrawOption = drawOption;
                this.SpecificCardToDrawIndex = specificCardToDrawIndex;
                this.CardToDrawType = cardToDrawType;
            }
        }

        public static event EventHandler<DrawCardPayload> OnDrawCard;

        public static void InvokeOnDrawCard([CanBeNull] object sender, DrawCardPayload payload)
        {
            OnDrawCard?.Invoke(sender, payload);
        }
    }

    public static class HandEvents
    {
        //Draw Card
        public class AddCardToHandPayload : EventArgs
        {
            public readonly Player.Player player;
            public readonly Gameplay.Card card;
            public readonly Vector3 fromPosition;

            public AddCardToHandPayload(Player.Player player, Gameplay.Card card, Vector3 fromPosition)
            {
                this.player = player;
                this.card = card;
                this.fromPosition = fromPosition;
            }
        }

        public static event EventHandler<AddCardToHandPayload> OnAddCardToHand;

        public static void InvokeOnAddCardToHand([CanBeNull] object sender, AddCardToHandPayload payload)
        {
            OnAddCardToHand?.Invoke(sender, payload);
        }
    }
}