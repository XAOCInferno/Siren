namespace Gameplay.Card
{
    public static class CardService
    {
        public static CardLogic localCardLogicBeingPlayed { get; private set; }

        public static void SetCardBeingPlayed(CardLogic cardLogic)
        {
            if (localCardLogicBeingPlayed != null)
            {
                localCardLogicBeingPlayed.CancelClick();
            }

            localCardLogicBeingPlayed = cardLogic;
        }

        public static void ClearCardBeingPlayed()
        {
            localCardLogicBeingPlayed = null;
        }
    }
}