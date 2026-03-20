using System;
using System.Threading.Tasks;
using Behaviours;
using Debug;
using Interaction;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Card
{
    public class CardView : MoveableObject, IStateObject<EInteractionState>
    {
        [SerializeField] protected CardViewModel cardViewModel;

        //TODO: Possibly move this to the view
        [SerializeField] protected float yMoveOnHover = 0.5f;

        [SerializeField] protected float moveTimeHandToHand = 0.2f;
        [SerializeField] protected float moveTimeDeckToHand = 0.3f;
        [SerializeField] protected float moveTimeHandToBoard = 0.3f;
        [SerializeField] protected float noContextMoveToTime = 0.2f;

        private CardState _state;

        protected Vector3 desiredPosition = Vector3.zero;
        protected Vector3 desiredOffset = Vector3.zero;

        private void Awake()
        {
            SubscribeToStateChangedEvent();
        }

        private void OnDestroy()
        {
            UnSubscribeFromStateChangedEvent();
        }
        
        public async Task Init()
        {
            //..Does nothing
        }

        public void SubscribeToStateChangedEvent()
        {
            _state = GetComponent<CardState>();
            if (!_state)
            {
                DebugSystem.Error("CardState not found");
                return;
            }

            _state.GetInteractionStateMachine().SubscribeToStateChangedCallback(this);
        }
        public void UnSubscribeFromStateChangedEvent()
        {
            _state.GetInteractionStateMachine().UnsubscribeToStateChangedCallback(this);
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

        public int OnStateChanged(EnumStateMachine<EInteractionState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case EInteractionState.Idle:
                case EInteractionState.Hovered:
                    //Apply hovered offset
                    SetDesiredOffset(Vector3.up * yMoveOnHover, moveTimeDeckToHand);
                    cardViewModel.SetBorderVisibility(false);
                    break;
                case EInteractionState.Selected:
                    cardViewModel.SetBorderVisibility(true);
                    break;
            }

            return 0;
        }

        public float GetMoveSpeedFromContext(ECardMoveContext context)
        {
            float moveTime = noContextMoveToTime;
            switch (context)
            {
                case ECardMoveContext.None:
                    moveTime = noContextMoveToTime;
                    break;
                case ECardMoveContext.DeckToHand:
                    moveTime = moveTimeDeckToHand;
                    break;
                case ECardMoveContext.HandToBoard:
                    moveTime = moveTimeHandToBoard;
                    break;
                case ECardMoveContext.HandToHand:
                    moveTime = moveTimeHandToHand;
                    break;
            }

            return moveTime;
        }
    }
}