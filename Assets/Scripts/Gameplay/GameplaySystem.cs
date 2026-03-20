using Debug;
using Gameplay.Card;
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

        public static void SetCardBeingPlayed(CardObject cardObject)
        {
            _cardBeingPlayed = cardObject;
        }

        public static ActionResult PlayCard(Vector2Int gridLocation, Player.Player playedBy)
        {
            //Make sure we have a card to play, otherwise return fail
            if (!_cardBeingPlayed) return new ActionResult(false, "No card being played according to GameplaySystem.");

            //Play
            _cardBeingPlayed.GetLogic().PlayCard(gridLocation, playedBy);

            //Clear
            ClearCardBeingPlayed();

            //Return success
            return new ActionResult(true);
        }
    }
}