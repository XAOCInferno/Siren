using Gameplay.Card;
using Player;
using UnityEngine;

namespace Gameplay
{
    public static class GameplaySystem
    {
        public static CardLogic AICardLogicBeingPlayed { get; private set; }
        public static CardLogic localCardLogicBeingPlayed { get; private set; }

        public static void SetLocalCardBeingPlayed(CardLogic cardLogic)
        {
            localCardLogicBeingPlayed = cardLogic;
        }

        public static void ClearLocalCardBeingPlayed()
        {
            localCardLogicBeingPlayed = null;
        }

        public static void SetAICardBeingPlayed(CardLogic cardLogic)
        {
            AICardLogicBeingPlayed = cardLogic;
        }

        public static void ClearAICardBeingPlayed()
        {
            AICardLogicBeingPlayed = null;
        }

        public static void PlayCard(Vector2Int gridLocation, bool playedByLocalPlayer)
        {
            if (playedByLocalPlayer)
            {
                //Play
                localCardLogicBeingPlayed.PlayCard(gridLocation, PlayerSystem.GetLocalPlayer());
                //Clear
                ClearLocalCardBeingPlayed();
            }
            else
            {
                //Play
                AICardLogicBeingPlayed.PlayCard(gridLocation, PlayerSystem.GetAIPlayer());
                //Clear
                ClearAICardBeingPlayed();
            }
        }
    }
}