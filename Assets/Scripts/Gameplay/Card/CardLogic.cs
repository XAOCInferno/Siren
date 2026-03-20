using System;
using System.Threading.Tasks;
using Behaviours;
using Debug;
using Gameplay.Tile;
using Global;
using Interaction;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Card
{
    public enum ECardType
    {
        Structure,
        Troop,
        Reaction
    }

    public enum ECardMoveContext
    {
        None,
        DeckToHand,
        HandToBoard,
        HandToHand
    }

    public class CardLogic : MonoBehaviour, IPooledItem, IStatedItem<ECardLogicState>,
        IInteractable, IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        private CardData _cardData;
        private CardObject _cardObject;

        protected Vector2Int desiredGridLocation = Vector2Int.zero;

        protected Player.Player lastPlayedBy;

        private void Awake()
        {
            _cardObject = gameObject.GetComponent<CardObject>();
            Assert.NotNull(_cardObject);
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
            _cardObject.GetState().GetLogicStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            _cardObject.GetState().GetLogicStateMachine().UnsubscribeToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<ECardLogicState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ECardLogicState.NotInPlay:
                case ECardLogicState.InDeck:
                    gameObject.SetActive(false);
                    break;
                case ECardLogicState.InHand:
                    gameObject.SetActive(true);
                    InteractionSystem.SetInteractable(this, true);
                    InteractionSystem.SetIdle(this);
                    break;
            }

            return 0;
        }
        //~IStatedItem End

        //~IPooledItem
        public void SetActive()
        {
            //..Does nothing
        }

        public void SetInActive()
        {
            _cardObject.GetState().GetLogicStateMachine().SetState(ECardLogicState.NotInPlay);
        }
        //~IPooledItem End

        //~IInteractable, Do not call directly instead let service call this
        public void SetIdle()
        {
            CardView view = _cardObject.GetView();
            //Set idle state
            _cardObject.GetState().GetInteractionStateMachine().SetState(EInteractionState.Idle);
            _cardObject.GetState().GetLogicStateMachine().SetState(ECardLogicState.InHand);
            //Apply idle offset
            view.SetDesiredOffset(Vector3.zero, view.GetMoveSpeedFromContext(ECardMoveContext.DeckToHand));

            //If we're the card being played, then clear
            if (GameplaySystem.localCardLogicBeingPlayed == this)
            {
                GameplaySystem.ClearLocalCardBeingPlayed();
            }
        }

        public void SetHovered()
        {
            _cardObject.GetState().GetInteractionStateMachine().SetState(EInteractionState.Hovered);
        }

        public void SetSelected()
        {
            CardState state = _cardObject.GetState();
            state.GetInteractionStateMachine().SetState(EInteractionState.Selected);
            GameplaySystem.SetLocalCardBeingPlayed(this);
        }

        public void SetInteractable(bool interactable)
        {
            CardState state = _cardObject.GetState();
            EnumStateMachine<EInteractionState> stateMachine = state.GetInteractionStateMachine();
            if (!interactable || stateMachine.GetState() == EInteractionState.UnInteractable)
            {
                state.GetInteractionStateMachine().SetState(EInteractionState.Idle);
            }

            //If we're the card being played, then clear
            if (GameplaySystem.localCardLogicBeingPlayed == this)
            {
                GameplaySystem.ClearLocalCardBeingPlayed();
            }
        }
        //~IInteractable End

        public void SetCardData(CardData newCardData)
        {
            //Our Data
            _cardData = newCardData;
            //Set on VM
            _cardObject.GetView().SetViewModelData(newCardData.GetViewData());
        }

        public CardData GetCardData() => _cardData;


        public void MoveToPosition(Vector3 position)
        {
            CardView view = _cardObject.GetView();
            view.SetDesiredPosition(position, view.GetMoveSpeedFromContext(ECardMoveContext.None));
        }

        public void MoveToPosition(Vector3 position, ECardMoveContext context)
        {
            CardView view = _cardObject.GetView();
            view.SetDesiredPosition(position, view.GetMoveSpeedFromContext(context));
        }

        public void MoveToPosition(Vector3 position, ECardMoveContext context,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            CardView view = _cardObject.GetView();
            view.SetDesiredPosition(position, view.GetMoveSpeedFromContext(context), callback);
        }

        public void PlayCard(Vector2Int atGridLocation, Player.Player player)
        {
            DebugSystem.Log($"Card {gameObject.name} is about to be played to the board");

            //Save who played this
            lastPlayedBy = player;

            //Where we will play to
            desiredGridLocation = atGridLocation;

            //Try get tile
            TileObject tile = BoardSystem<TileObject>.GetItemOnGrid(atGridLocation);
            Assert.NotNull(tile);

            //End interaction
            InteractionSystem.SetInteractable(this, false);

            //Change state to being played to the board
            _cardObject.GetState().GetLogicStateMachine().SetState(ECardLogicState.PlayedToBoard);

            //Move to, on completion play card
            MoveToPosition(tile.transform.position, ECardMoveContext.HandToBoard, FinishPlayCard);
        }

        protected int FinishPlayCard(EMoveCompleteCallbackType callbackType)
        {
            //Play
            BoardEvents.InvokeOnOrderPlacePieceOnBoard(this,
                new BoardEvents.OrderPlacePieceOnBoardPayload(_cardData.GetAssociatedPieceData(), desiredGridLocation,
                    lastPlayedBy));
            lastPlayedBy = null;

            //Reset
            desiredGridLocation = Vector2Int.zero;

            //Remove from hand
            HandEvents.InvokeOnRemoveCardFromHand(this, new HandEvents.RemoveCardFromHandPayload(this));
            PoolSystem<CardLogic>.GetPool().ReturnToPool(this);

            //End
            return 0;
        }

        //TODO: moveTime dynamic?
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_cardObject.GetState().GetLogicStateMachine().GetState() == ECardLogicState.InHand)
            {
                InteractionSystem.SetHovered(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_cardObject.GetState().GetLogicStateMachine().GetState() == ECardLogicState.InHand)
            {
                InteractionSystem.SetIdle(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //TODO: Can activate calculation here
            bool canActivate = true;
            canActivate &= _cardObject.GetState().GetLogicStateMachine().GetState() == ECardLogicState.InHand;
            canActivate &= GameplaySystem.localCardLogicBeingPlayed != this;
            if (!canActivate)
            {
                return;
            }

            DebugSystem.Log($"Card {gameObject.name} is being played from hand");
            _cardObject.GetState().GetLogicStateMachine().SetState(ECardLogicState.SelectedInHand);
            InteractionSystem.SetSelected(this);
        }
    }
}