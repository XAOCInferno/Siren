using System;
using System.Linq;
using Gameplay.Card;
using Gameplay.Piece;
using Gameplay.Resources;
using Gameplay.Tile;
using Global;
using JetBrains.Annotations;
using Player;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay
{
    public enum EGameplayPhaseState
    {
        CardPhase = 0,
        BoardPhase,
        ReactionPhase
    }

    public static class GameplaySystem
    {
        [CanBeNull] private static PieceObject _pieceBeingControlled;
        [CanBeNull] private static CardObject _cardBeingPlayed;
        [CanBeNull] private static CardObject _cardBeingHovered;

        public static PieceObject GetPieceBeingControlled() => _pieceBeingControlled;
        public static CardObject GetCardBeingPlayed() => _cardBeingPlayed;

        private static EnumStateMachine<EGameplayPhaseState> _gameplayPhaseStateMachine = new();

        public static bool HasCardBeingPlayed()
        {
            return _cardBeingPlayed;
        }

        public static void SetCardBeingPlayed(CardObject cardObject)
        {
            _cardBeingPlayed = cardObject;
        }

        public static void ClearCardBeingPlayed()
        {
            _cardBeingPlayed = null;
        }

        public static bool HasPieceBeingControlled()
        {
            return _pieceBeingControlled;
        }

        public static void SetPieceBeingControlled(PieceObject pieceObject)
        {
            _pieceBeingControlled = pieceObject;
        }

        public static void ClearPieceBeingControlled()
        {
            _pieceBeingControlled = null;
        }

        public static Util.ActionResult PlayCard(Vector2Int gridLocation, Player.Player playedBy)
        {
            //Make sure we have a card to play, otherwise return fail
            if (!_cardBeingPlayed)
                return new Util.ActionResult(false, "No card being played according to GameplaySystem.");

            CardData data = _cardBeingPlayed.GetLogic().GetCardData();
            if (!CanPlayCard(data, playedBy).isSuccess ||
                !CanPlayPieceAtLocation(data.GetAssociatedPieceData(), gridLocation).isSuccess)
                return new Util.ActionResult(false);

            //Safe to continue

            //Subtract cost
            ResourceSystem.PlayerChangeResources(playedBy, data.GetResourceChangeBy());

            //Play
            _cardBeingPlayed.GetLogic().PlayCard(gridLocation, playedBy);

            //Clear
            ClearCardBeingPlayed();

            //Return success
            return new Util.ActionResult(true);
        }

        public static Util.ActionResult CanPlayCard(CardData cardData, Player.Player playedBy)
        {
            //Check we have enough resources to play the card
            return ResourceSystem.PlayerHasEnoughResources(playedBy, cardData.GetResourceChangeBy());
        }

        public static BoardSystem<TileObject>.GetItemsInAreaResponse GetTilesPieceWouldOccupy(PieceData pieceData,
            Vector2Int gridLocation)
        {
            //Get items in our shape
            BoardSystem<TileObject>.GetItemsInAreaResponse response = pieceData.GetSizeShape() switch
            {
                EPieceSizeShapeTypes.Circle => BoardSystem<TileObject>.GetItemsInCircle(gridLocation,
                    pieceData.GetBaseSize()),
                EPieceSizeShapeTypes.Square => BoardSystem<TileObject>.GetItemsInSquare(gridLocation,
                    pieceData.GetBaseSize()),
                _ => throw new ArgumentOutOfRangeException()
            };
            return response;
        }

        public static Util.ActionResult CanMovePieceToLocation(PieceObject pieceObject, Vector2Int gridLocation)
        {
            //Check is in range
            if (!pieceObject.GetState().GetPossibleMovementLocations().Contains(gridLocation))
            {
                return new Util.ActionResult(false,
                    "Trying to move piece to a location that is not in movement range.");
            }

            return CanPlayPieceAtLocation(pieceObject.GetLogic().GetPieceData(), gridLocation);
        }

        public static Util.ActionResult CanPlayPieceAtLocation(PieceData pieceData, Vector2Int gridLocation)
        {
            //Get items in our shape
            BoardSystem<TileObject>.GetItemsInAreaResponse response = GetTilesPieceWouldOccupy(pieceData, gridLocation);

            //Check if all expected tiles are present
            if (response.isMissingExpectedItems)
                return new Util.ActionResult(false,
                    $"No space to play card at location {gridLocation} as object would go out of bounds.");

            //Check for objects being occupied
            if (response.foundItems.Any(t => t.Key.GetState().GetLogicStateMachine().GetState() ==
                                             ETileLogicState.OccupiedByPiece))
            {
                return new Util.ActionResult(false,
                    $"No space to play card at location {gridLocation} as required tile is occupied.");
            }

            //Success
            return new Util.ActionResult(true);
        }

        public static Util.ActionResult MovePiece(PieceObject pieceObject, Vector2Int gridLocation)
        {
            //Make sure we can play the piece at the given location
            if (!CanMovePieceToLocation(pieceObject, gridLocation).isSuccess)
                return new Util.ActionResult(false);

            //Safe to continue
            //Play
            BoardEvents.InvokeOnOrderMovePieceOnBoard(pieceObject,
                new BoardEvents.OrderMovePieceOnBoardPayload(pieceObject, gridLocation,
                    BoardSystem<PieceObject>.GetItemLocationOnGrid(pieceObject)));

            //Return success
            return new Util.ActionResult(true);
        }

        /// <summary>
        /// Changes gameplay phase. This will always work, there is no checking if it's a valid state change.
        /// </summary>
        /// <param name="newState">The phase we want to enter.</param>
        public static void SetGameplayPhaseState(EGameplayPhaseState newState)
        {
            // Get old state
            EGameplayPhaseState oldState = _gameplayPhaseStateMachine.GetState();

            // Set new state
            _gameplayPhaseStateMachine.SetState(newState);

            // Broadcast
            GameplayEvents.InvokeOnGameplayPhaseStateChanged(GameplayManagerSingleton.instance,
                new GameplayEvents.GameplayPhaseStateChangedPayload(newState, oldState));
        }

        /// <summary>
        /// Gets the current gameplay phase state.
        /// </summary>
        /// <returns>The current state we're in.</returns>
        public static EGameplayPhaseState GetGameplayPhaseState() => _gameplayPhaseStateMachine.GetState();


        [CanBeNull]
        public static CardObject GetCardBeingHovered() => _cardBeingHovered;

        public static void SetCardBeingHovered(CardObject card)
        {
            // Set
            _cardBeingHovered = card;

            // Don't preview resources if playing a card, as playing a card takes priority
            if (_cardBeingPlayed || !_cardBeingHovered) return;

            // Set cost preview
            ResourceChange[] costs = _cardBeingHovered.GetLogic().GetCardData().GetResourceChangeBy();
            for (int i = 0; i < costs.Length; i++)
            {
                // Get cost
                ResourceChange change = costs[i];

                // Broadcast
                ResourceEvents.InvokeOnResourcePreviewChanged(null,
                    new ResourceEvents.ResourcePreviewChanged(change.resourceType, change.decrease,
                        ResourceSystem.GetPlayerResourceAmount(PlayerSystem.GetLocalPlayer(), change.resourceType),
                        change.increase,
                        true));
            }
        }

        public static void ClearCardBeingHovered()
        {
            // No need to clear if we aren't previewing yet
            if (!_cardBeingHovered) return;

            // Clear cost preview
            ResourceChange[] costs = _cardBeingHovered.GetLogic().GetCardData().GetResourceChangeBy();
            for (int i = 0; i < costs.Length; i++)
            {
                // Get cost
                ResourceChange change = costs[i];

                // Broadcast
                ResourceEvents.InvokeOnResourcePreviewChanged(null,
                    new ResourceEvents.ResourcePreviewChanged(change.resourceType, 0,
                        ResourceSystem.GetPlayerResourceAmount(PlayerSystem.GetLocalPlayer(), change.resourceType), 0,
                        true));
            }

            // Clear hovered
            _cardBeingHovered = null;
        }
    }
}