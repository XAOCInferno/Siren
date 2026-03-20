using Gameplay.Card;
using JetBrains.Annotations;
using UnityEngine;

namespace Gameplay
{
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

        public static void PlayCard(Vector2Int gridLocation, Player.Player playedBy)
        {
            if (!_cardBeingPlayed) return;
            //Play
            _cardBeingPlayed.GetLogic().PlayCard(gridLocation, playedBy);
            //Clear
            ClearCardBeingPlayed();
        }
    }
}