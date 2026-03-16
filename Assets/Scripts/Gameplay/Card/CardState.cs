using Debug;
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
        private CardView _view;
        private CardLogic _logic;

        private readonly EnumStateMachine<ECardState> _stateMachine = new();

        private void Awake()
        {
            _view = GetComponent<CardView>();
            _logic = GetComponent<CardLogic>();
        }

        public CardView GetView() => _view;

        public EnumStateMachine<ECardState> GetStateMachine() => _stateMachine;
    }
}