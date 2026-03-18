using System;
using Behaviours;
using Debug;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Card
{
    public class CardView : MoveableObject, IStateObject<ECardState>
    {
        [SerializeField] protected CardViewModel cardViewModel;

        private CardState _state;

        protected Vector3 desiredPosition = Vector3.zero;
        protected Vector3 desiredOffset = Vector3.zero;

        private void Awake()
        {
            ListenToStateChangedEvent();
        }

        public void ListenToStateChangedEvent()
        {
            _state = GetComponent<CardState>();
            if (!_state)
            {
                DebugSystem.Error("CardState not found");
                return;
            }

            _state.GetStateMachine().ListenToStateChangedCallback(this);
        }

        public void SetViewModelData(CardViewModelData data)
        {
            cardViewModel.SetViewModelData(data);
        }

        public void SetDesiredPosition(Vector3 position, float moveTime)
        {
            desiredPosition = position;
            MoveToLocation(desiredPosition + desiredOffset, moveTime);
        }

        public void SetDesiredPosition(Vector3 position, float moveTime,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            desiredPosition = position;
            MoveToLocation(desiredPosition + desiredOffset, moveTime, callback);
        }

        public void SetDesiredOffset(Vector3 offset, float moveTime)
        {
            desiredOffset = offset;
            MoveToLocation(desiredPosition + desiredOffset, moveTime);
        }

        public int OnStateChanged(EnumStateMachine<ECardState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ECardState.NotInPlay:
                case ECardState.InDeck:
                case ECardState.InHand:
                case ECardState.PlayedToBoard:
                    cardViewModel.SetBorderVisibility(false);
                    break;
                case ECardState.SelectedInHand:
                    cardViewModel.SetBorderVisibility(true);
                    break;
            }

            return 0;
        }
    }
}