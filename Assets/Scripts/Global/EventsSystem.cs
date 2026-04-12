using System;
using CustomCamera;
using Gameplay;
using Gameplay.Card;
using Gameplay.Piece;
using Gameplay.Resources;
using Gameplay.Tile;
using Input;
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
            public readonly Player.Player spawnedByPlayer;

            public OrderPlacePieceOnBoardPayload(PieceData pieceData, Vector2Int gridCoordinates,
                Player.Player spawnedByPlayer)
            {
                this.pieceData = pieceData;
                this.gridCoordinates = gridCoordinates;
                this.spawnedByPlayer = spawnedByPlayer;
            }
        }

        public static event EventHandler<OrderPlacePieceOnBoardPayload> OnOrderPlacePieceOnBoard;

        public static void InvokeOnOrderPlacePieceOnBoard([CanBeNull] object sender,
            OrderPlacePieceOnBoardPayload payload)
        {
            OnOrderPlacePieceOnBoard?.Invoke(sender, payload);
        }

        //Move a piece
        public class OrderMovePieceOnBoardPayload : EventArgs
        {
            public readonly PieceObject pieceObject;
            public readonly Vector2Int toGridCoordinates;
            public readonly Vector2Int fromGridCoordinates;

            public OrderMovePieceOnBoardPayload(PieceObject pieceObject, Vector2Int toGridCoordinates,
                Vector2Int fromGridCoordinates)
            {
                this.pieceObject = pieceObject;
                this.toGridCoordinates = toGridCoordinates;
                this.fromGridCoordinates = fromGridCoordinates;
            }
        }

        public static event EventHandler<OrderMovePieceOnBoardPayload> OnOrderMovePieceOnBoard;

        public static void InvokeOnOrderMovePieceOnBoard([CanBeNull] object sender,
            OrderMovePieceOnBoardPayload payload)
        {
            OnOrderMovePieceOnBoard?.Invoke(sender, payload);
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
            public readonly CardLogic cardLogic;
            public readonly Vector3 fromPosition;

            public AddCardToHandPayload(Player.Player player, CardLogic cardLogic, Vector3 fromPosition)
            {
                this.player = player;
                this.cardLogic = cardLogic;
                this.fromPosition = fromPosition;
            }
        }

        public static event EventHandler<AddCardToHandPayload> OnAddCardToHand;

        public static void InvokeOnAddCardToHand([CanBeNull] object sender, AddCardToHandPayload payload)
        {
            OnAddCardToHand?.Invoke(sender, payload);
        }

        //Remove Card
        public class RemoveCardFromHandPayload : EventArgs
        {
            public readonly CardLogic cardLogic;

            public RemoveCardFromHandPayload(CardLogic cardLogic)
            {
                this.cardLogic = cardLogic;
            }
        }

        public static event EventHandler<RemoveCardFromHandPayload> OnRemoveCardFromHand;

        public static void InvokeOnRemoveCardFromHand([CanBeNull] object sender, RemoveCardFromHandPayload payload)
        {
            OnRemoveCardFromHand?.Invoke(sender, payload);
        }
    }

    public static class InputEvents
    {
        //Camera Move Input Event
        public class InputMoveCameraEventPayload : EventArgs
        {
            public readonly EMoveCameraDirection direction;
            public readonly bool fromInput;

            public InputMoveCameraEventPayload(EMoveCameraDirection direction, bool fromInput)
            {
                this.direction = direction;
                this.fromInput = fromInput;
            }
        }

        public static event EventHandler<InputMoveCameraEventPayload> OnInputMoveCamera;

        public static void InvokeOnInputMoveCamera([CanBeNull] object sender, InputMoveCameraEventPayload payload)
        {
            OnInputMoveCamera?.Invoke(sender, payload);
        }
    }

    public static class CameraEvents
    {
        //Input Events
        public class CameraMovedEventPayload : EventArgs
        {
            public readonly ECameraViewMode oldMode;
            public readonly ECameraViewMode newMode;

            public CameraMovedEventPayload(ECameraViewMode oldMode, ECameraViewMode newMode)
            {
                this.oldMode = oldMode;
                this.newMode = newMode;
            }
        }

        public static event EventHandler<CameraMovedEventPayload> OnCameraMoved;

        public static void InvokeOnCameraMoved([CanBeNull] object sender, CameraMovedEventPayload payload)
        {
            OnCameraMoved?.Invoke(sender, payload);
        }
    }

    public static class TileEvents
    {
        //Input Events
        public class OnTileSelectedPayload : EventArgs
        {
            public readonly TileObject tileObject;

            public OnTileSelectedPayload(TileObject tileObject)
            {
                this.tileObject = tileObject;
            }
        }

        public static event EventHandler<OnTileSelectedPayload> OnTileSelected;

        public static void InvokeOnTileSelected([CanBeNull] object sender, OnTileSelectedPayload payload)
        {
            OnTileSelected?.Invoke(sender, payload);
        }
    }

    public static class GameplayEvents
    {
        //Broadcast gameplay phase changed. This is a result of the phase changing, not to change it. To change it, call the function in the GameplaySystem
        public class GameplayPhaseStateChangedPayload : EventArgs
        {
            public readonly EGameplayPhaseState newGameplayPhaseState;
            public readonly EGameplayPhaseState oldGameplayPhaseState;

            public GameplayPhaseStateChangedPayload(EGameplayPhaseState newGameplayPhaseState,
                EGameplayPhaseState oldGameplayPhaseState)
            {
                this.newGameplayPhaseState = newGameplayPhaseState;
                this.oldGameplayPhaseState = oldGameplayPhaseState;
            }
        }

        public static event EventHandler<GameplayPhaseStateChangedPayload> OnGameplayPhaseStateChanged;

        public static void InvokeOnGameplayPhaseStateChanged([CanBeNull] object sender,
            GameplayPhaseStateChangedPayload payload)
        {
            OnGameplayPhaseStateChanged?.Invoke(sender, payload);
        }
    }

    public static class ResourceEvents
    {
        //Broadcast resource changes (either current resources or previewed resources;
        public class ResourcePreviewChanged : EventArgs
        {
            public readonly EResourceType resourceType;
            public readonly int costsPreviewed;
            public readonly int resourcesRemaining;
            public readonly int resourcesGained;
            public readonly bool isLocalPlayer;

            public ResourcePreviewChanged(EResourceType resourceType, int costsPreviewed, int resourcesRemaining, int resourcesGained,
                bool isLocalPlayer)
            {
                this.resourceType = resourceType;
                this.costsPreviewed = costsPreviewed;
                this.resourcesRemaining = resourcesRemaining;
                this.resourcesGained = resourcesGained;
                this.isLocalPlayer = isLocalPlayer;
            }
        }

        public static event EventHandler<ResourcePreviewChanged> OnResourcePreviewChanged;

        public static void InvokeOnResourcePreviewChanged([CanBeNull] object sender,
            ResourcePreviewChanged payload)
        {
            OnResourcePreviewChanged?.Invoke(sender, payload);
        }
    }
}