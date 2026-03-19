using System.Threading.Tasks;
using Gameplay.Card;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileLogic : MonoBehaviour, IStateObject<ETileLogicState>
    {
        protected TileObject tileObject;

        private void Awake()
        {
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);
            ListenToStateChangedEvent();
        }

        public async Task Init()
        {
            //..Nothing
        }

        public void ListenToStateChangedEvent()
        {
            tileObject.GetState().GetLogicStateMachine().ListenToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<ETileLogicState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ETileLogicState.OccupiedByPiece:
                    tileObject.GetState().GetViewStateMachine().SetState(ETileViewState.Idle);
                    break;
            }

            return 0;
        }

        //Not called directly on this object, requires 1 or more TileInputTrigger that will inform this
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!tileObject.GetState().GetIsOccupiedByPiece())
            {
                EnumStateMachine<ETileViewState> viewStateMachine = tileObject.GetState().GetViewStateMachine();
                if (viewStateMachine.GetState() != ETileViewState.Idle)
                {
                    return;
                }

                viewStateMachine.SetState(ETileViewState.Hovered);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //State
            TileState state = tileObject.GetState();

            //Ensure this is valid
            if (state.GetIsOccupiedByPiece() || CardService.localCardLogicBeingPlayed == null) return;

            //Valid, get data and spawn piece
            CardService.localCardLogicBeingPlayed.PlayCard(state.GetGridLocation());

            //Clear
            CardService.ClearCardBeingPlayed();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tileObject.GetState().GetViewStateMachine().SetState(ETileViewState.Idle);
        }
    }
}