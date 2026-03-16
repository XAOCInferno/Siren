using Debug;
using UnityEngine;

namespace Gameplay.Card
{
    public interface IStateObject
    {
        public void OnStateChanged(int newState);
    }

    public class CardState : MonoBehaviour
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

        private ECardState _state = ECardState.None;
        private CardView _view;
        private CardLogic _logic;

        private void Awake()
        {
            _view = GetComponent<CardView>();
            _logic = GetComponent<CardLogic>();
        }

        public void SetState(ECardState newState)
        {
            if (_state == newState) return;
            //Diff state
            _state = newState;
            DebugSystem.Log($"Card {gameObject.name} state changed to {newState}");

            _view.OnStateChanged((int)_state);
            _logic.OnStateChanged((int)_state);
        }

        public ECardState GetState()
        {
            return _state;
        }

        public CardView GetView() => _view;
    }
}