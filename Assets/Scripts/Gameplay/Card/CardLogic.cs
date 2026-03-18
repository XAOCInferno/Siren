using System;
using Behaviours;
using Debug;
using Gameplay.Tile;
using Global;
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
        DeckToHand,
        HandToBoard,
        HandToHand
    }

    public class CardLogic : PooledObject, IStateObject<ECardState>, IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        //TODO: Possibly move this to the view
        [SerializeField] protected float yMoveOnHover = 0.5f;

        [SerializeField] protected float moveTimeHandToHand = 0.2f;
        [SerializeField] protected float moveTimeDeckToHand = 0.3f;
        [SerializeField] protected float moveTimeHandToBoard = 0.3f;
        [SerializeField] protected float noContextMoveToTime = 0.2f;

        private CardData _cardData;
        private CardObject _cardObject;

        protected Vector2Int desiredGridLocation = Vector2Int.zero;

        private void Awake()
        {
            _cardObject = gameObject.GetComponent<CardObject>();
            Assert.NotNull(_cardObject);
            ListenToStateChangedEvent();
        }

        public void ListenToStateChangedEvent()
        {
            _cardObject.GetState().GetStateMachine().ListenToStateChangedCallback(this);
        }

        public void SetCardData(CardData newCardData)
        {
            //Our Data
            _cardData = newCardData;
            //Set on VM
            _cardObject.GetView().SetViewModelData(newCardData.GetViewData());
        }

        public CardData GetCardData() => _cardData;


        public override void SetActive()
        {
            //..Does nothing
        }

        public override void SetInActive()
        {
            _cardObject.GetState().GetStateMachine().SetState(ECardState.NotInPlay);
        }

        public int OnStateChanged(EnumStateMachine<ECardState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ECardState.NotInPlay:
                case ECardState.InDeck:
                    gameObject.SetActive(false);
                    break;
                case ECardState.InHand:
                case ECardState.SelectedInHand:
                case ECardState.PlayedToBoard:
                    gameObject.SetActive(true);
                    break;
            }

            return 0;
        }

        public void MoveToPosition(Vector3 position)
        {
            _cardObject.GetView().SetDesiredPosition(position, noContextMoveToTime);
        }

        public void MoveToPosition(Vector3 position, ECardMoveContext context)
        {
            _cardObject.GetView().SetDesiredPosition(position, GetMoveSpeedFromContext(context));
        }

        public void MoveToPosition(Vector3 position, ECardMoveContext context,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            _cardObject.GetView().SetDesiredPosition(position, GetMoveSpeedFromContext(context), callback);
        }

        protected float GetMoveSpeedFromContext(ECardMoveContext context)
        {
            float moveTime = noContextMoveToTime;
            switch (context)
            {
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

        //TODO: moveTime dynamic?
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_cardObject.GetState().GetStateMachine().GetState() == ECardState.InHand)
            {
                _cardObject.GetView().SetDesiredOffset(Vector3.up * yMoveOnHover, moveTimeDeckToHand);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_cardObject.GetState().GetStateMachine().GetState() == ECardState.InHand)
            {
                _cardObject.GetView().SetDesiredOffset(Vector3.zero, moveTimeDeckToHand);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //TODO: Can activate calculation here
            bool canActivate = true;
            canActivate &= _cardObject.GetState().GetStateMachine().GetState() == ECardState.InHand;
            canActivate &= CardService.localCardLogicBeingPlayed != this;
            if (!canActivate)
            {
                return;
            }

            DebugSystem.Log($"Card {gameObject.name} is being played from hand");
            _cardObject.GetState().GetStateMachine().SetState(ECardState.SelectedInHand);
            CardService.SetCardBeingPlayed(this);
        }

        public void CancelClick()
        {
            _cardObject.GetState().GetStateMachine().SetState(ECardState.InHand);
            _cardObject.GetView().SetDesiredOffset(Vector3.zero, moveTimeHandToHand);
        }

        public void PlayCard(Vector2Int atGridLocation)
        {
            DebugSystem.Log($"Card {gameObject.name} is about to be played to the board");

            //Where we will play to
            desiredGridLocation = atGridLocation;

            //Change state to being played to the board
            _cardObject.GetState().GetStateMachine().SetState(ECardState.PlayedToBoard);

            //Try get tile
            TileObject tile = BoardSystem<TileObject>.GetItemOnGrid(atGridLocation);
            if (!tile)
            {
                //No tile, so just try to play (though this shouldn't happen)
                DebugSystem.Error($"No tile found at location {atGridLocation} so cannot move card to it!");
                FinishPlayCard(EMoveCompleteCallbackType.Completed);
                return;
            }

            //Move to, on completion play card
            MoveToPosition(tile.transform.position, ECardMoveContext.HandToBoard, FinishPlayCard);
        }

        protected int FinishPlayCard(EMoveCompleteCallbackType callbackType)
        {
            //Play
            BoardEvents.InvokeOnOrderPlacePieceOnBoard(this,
                new BoardEvents.OrderPlacePieceOnBoardPayload(_cardData.GetAssociatedPieceData(), desiredGridLocation));

            //Reset
            desiredGridLocation = Vector2Int.zero;

            //Remove from hand
            HandEvents.InvokeOnRemoveCardFromHand(this, new HandEvents.RemoveCardFromHandPayload(this));

            //End
            return 0;
        }
    }
}