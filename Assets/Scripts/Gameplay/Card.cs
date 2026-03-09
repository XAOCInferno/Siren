using Debug;

namespace Gameplay
{
    public enum ECardType
    {
        Structure,
        Troop,
        Reaction
    }
    public enum ECardState
    {
        NotInPlay = 0,
        InDeck,
        InHand
    }
    public class CardData
    {
        public ECardType Type;
        public ECardState State = ECardState.NotInPlay;
    }

    public class Card : Utils.PooledObject
    {
        private readonly CardData _cardData = new();

        public CardData GetCardData() => _cardData;
        public override void SetActive()
        {
            //..Does nothing
        }

        public override void SetInActive()
        {
            SetState(ECardState.NotInPlay);
        }

        public void SetState(ECardState newState)
        {
            _cardData.State = newState;
            DebugSystem.Log($"Card {this.gameObject.name} state changed to {newState}");
            switch (newState)
            {
                case ECardState.NotInPlay:
                    gameObject.SetActive(false);
                    break;
                case ECardState.InDeck:
                    gameObject.SetActive(false);
                    break;
                case ECardState.InHand:
                    gameObject.SetActive(true);
                    break;
            }
        }
    }
}