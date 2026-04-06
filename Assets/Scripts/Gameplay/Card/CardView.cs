using System;
using System.Threading.Tasks;
using Behaviours;
using CustomCamera;
using Interaction;
using JetBrains.Annotations;
using NUnit.Framework;
using UI;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Card
{
    public class CardView : MonoBehaviour, IStatedItem<EInteractionState>
    {
        [SerializeField] protected CardViewModel cardViewModel;

        [SerializeField] protected float scale = 0.5f;

        //TODO: Possibly move this to the view
        [SerializeField] protected float yMoveOnHover = 0.5f;

        [SerializeField] protected float moveTimeHandToHand = 0.2f;
        [SerializeField] protected float moveTimeDeckToHand = 0.3f;
        [SerializeField] protected float moveTimeHandToBoard = 0.3f;
        [SerializeField] protected float noContextMoveToTime = 0.2f;

        private CardObject _object;

        protected Vector3 desiredPosition = Vector3.zero;
        protected Vector3 desiredOffset = Vector3.zero;

        private void Awake()
        {
            // Cache
            _object = GetComponent<CardObject>();
            Assert.NotNull(_object);
            
            // Set Scale
            transform.localScale = Vector3.one * scale;

            // Sub to state machine
            SubscribeToStateChangedEvent();
        }

        private void OnDestroy()
        {
            UnSubscribeFromStateChangedEvent();
        }

        //~IStatedItem
        public async Task Init()
        {
            //..Does nothing
        }

        public void SubscribeToStateChangedEvent()
        {
            _object.GetState().GetInteractionStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            _object.GetState().GetInteractionStateMachine().UnsubscribeToStateChangedCallback(this);
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

                    //If we were previously selected then reset selected related logic
                    if (payload.oldState == EInteractionState.Selected)
                    {
                        //Clear preview
                        PlayingCardPreviewSingleton.instance.ClearFocusedCardData();

                        //Return to hand
                        CameraSubsystem.GetMainCamera().ChangeCameraViewMode(ECameraViewMode.Hand);
                    }

                    break;
                case EInteractionState.Selected:
                    //Enable border
                    cardViewModel.SetBorderVisibility(true);

                    //Move camera to focus the board
                    CameraSubsystem.GetMainCamera().ChangeCameraViewMode(ECameraViewMode.Board);

                    //Set preview
                    PlayingCardPreviewSingleton.instance.SetFocusedCardData(cardViewModel.GetViewModelData());
                    break;
            }

            return 0;
        }
        //~IStatedItem End

        public void SetViewModelData(CardViewModelData data)
        {
            cardViewModel.SetViewModelData(data);
        }

        public void SetDesiredPosition(Vector3 position, float moveTime)
        {
            desiredPosition = position;
            _object.GetMoveableObject().MoveTo(desiredPosition + desiredOffset, moveTime, true);
        }

        public void SetDesiredPosition(Vector3 position, float moveTime,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            desiredPosition = position;
            _object.GetMoveableObject().MoveTo(desiredPosition + desiredOffset, moveTime, true, callback);
        }

        public void SetDesiredOffset(Vector3 offset, float moveTime)
        {
            desiredOffset = offset;
            _object.GetMoveableObject().MoveTo(desiredPosition + desiredOffset, moveTime, true);
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