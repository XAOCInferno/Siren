using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Card
{
    public enum ECardState
    {
        None = 0,
        NotInPlay,
        InDeck,
        InHand,
        SelectedInHand,
        PlayedToBoard,
    }

    public class CardState : MonoBehaviour
    {
        private readonly EnumStateMachine<ECardState> _stateMachine = new();

        public EnumStateMachine<ECardState> GetStateMachine() => _stateMachine;
    }
}