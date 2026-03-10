using System;
using Debug;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public class Card : Utils.PooledObject, IPointerEnterHandler, IPointerExitHandler
    {
        //TODO: Possibly move this to the view
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;

        private readonly CardData _cardData = new();
        protected CardView view = null;

        public CardData GetCardData() => _cardData;

        private void Awake()
        {
            view = GetComponent<CardView>();
            if (!view)
            {
                DebugSystem.Error("CardView not found");
            }
        }

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

        public void SetDesiredPosition(Vector3 position)
        {
            if (view)
            {
                view.SetDesiredPosition(position, moveTime);
            }
        }

        //TODO: moveTime dynamic?
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (view)
            {
                view.SetDesiredOffset(Vector3.up * yMoveOnHover, moveTime);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (view)
            {
                view.SetDesiredOffset(Vector3.zero, moveTime);
            }
        }
    }
}