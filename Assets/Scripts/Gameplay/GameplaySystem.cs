using System;
using System.Linq;
using Debug;
using Gameplay.Card;
using Gameplay.Piece;
using Gameplay.Tile;
using JetBrains.Annotations;
using UnityEngine;

namespace Gameplay
{
    //Use this as a function return value for a function which would normally return a bool (success or fail)
    public struct ActionResult
    {
        public readonly bool isSuccess;
        [CanBeNull] readonly string msg;

        public ActionResult(bool isSuccess, [CanBeNull] string msg = null)
        {
            this.isSuccess = isSuccess;
            this.msg = msg;
            //Log message if we have it
            if (msg == null) return;
            if (isSuccess)
            {
                //Success, Log
                DebugSystem.Log(msg);
            }
            else
            {
                //Fail, Warn. We do not need to error as most failures are safe. If it is not safe then we can handle on a need-to basis
                DebugSystem.Warn(msg);
            }
        }
    }

    public static class GameplaySystem
    {
        [CanBeNull] private static CardObject _cardBeingPlayed;

        public static CardObject GetCardBeingPlayed() => _cardBeingPlayed;

        public static void SetLocalCardBeingPlayed(CardObject cardObject)
        {
            _cardBeingPlayed = cardObject;
        }

        public static void ClearCardBeingPlayed()
        {
            _cardBeingPlayed = null;
        }

        public static ActionResult PlayCard(Vector2Int gridLocation, Player.Player playedBy)
        {
            //Make sure we have a card to play, otherwise return fail
            if (!_cardBeingPlayed) return new ActionResult(false, "No card being played according to GameplaySystem.");
            if (!CanPlayPieceAtLocation(_cardBeingPlayed.GetLogic().GetCardData().GetAssociatedPieceData(),
                    gridLocation).isSuccess) return new ActionResult(false);

            //Safe to continue
            //Play
            _cardBeingPlayed.GetLogic().PlayCard(gridLocation, playedBy);

            //Clear
            ClearCardBeingPlayed();

            //Return success
            return new ActionResult(true);
        }

        public static ActionResult CanPlayPieceAtLocation(PieceData pieceData, Vector2Int gridLocation)
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

            //Check if all expected tiles are present
            if (response.isMissingExpectedItems)
                return new ActionResult(false,
                    $"No space to play card at location {gridLocation} as object would go out of bounds.");

            //Check for objects being occupied
            if (response.foundItems.Any(t => t.GetState().GetLogicStateMachine().GetState() ==
                                             ETileLogicState.OccupiedByPiece))
            {
                return new ActionResult(false,
                    $"No space to play card at location {gridLocation} as required tile is occupied.");
            }

            //Success
            return new ActionResult(true);
        }
    }
}