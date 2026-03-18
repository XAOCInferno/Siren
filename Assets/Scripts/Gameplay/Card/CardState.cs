using Interaction;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Card
{
    public enum ECardLogicState
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
        //Logic
        private readonly EnumStateMachine<ECardLogicState> _logicStateMachine = new();
        public EnumStateMachine<ECardLogicState> GetLogicStateMachine() => _logicStateMachine;
        
        //View
        private readonly EnumStateMachine<EInteractionState> _interactionStateMachine = new();
        public EnumStateMachine<EInteractionState> GetInteractionStateMachine() => _interactionStateMachine;
    }
}