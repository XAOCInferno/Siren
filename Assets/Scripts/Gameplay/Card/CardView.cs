using System;
using Behaviours;
using JetBrains.Annotations;
using UI;
using UnityEngine;

namespace Gameplay
{
    public class CardView : MoveableObject
    {
        [SerializeField] protected CardViewModel cardViewModel;

        protected Vector3 desiredPosition = Vector3.zero;
        protected Vector3 desiredOffset = Vector3.zero;

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
            [CanBeNull] Func<MoveCompleteCallbackType, int> callback)
        {
            desiredPosition = position;
            MoveToLocation(desiredPosition + desiredOffset, moveTime, callback);
        }

        public void SetDesiredOffset(Vector3 offset, float moveTime)
        {
            desiredOffset = offset;
            MoveToLocation(desiredPosition + desiredOffset, moveTime);
        }

        public void OnStateChanged(int newState)
        {
            CardState.ECardState state = (CardState.ECardState)newState;
            switch (state)
            {
                case CardState.ECardState.NotInPlay:
                case CardState.ECardState.InDeck:
                case CardState.ECardState.InHand:
                case CardState.ECardState.PlayedToBoard:
                    cardViewModel.SetBorderVisibility(false);
                    break;
                case CardState.ECardState.SelectedInHand:
                    cardViewModel.SetBorderVisibility(true);
                    break;
            }
        }
    }
}