using System.Threading.Tasks;
using Global;
using NUnit.Framework;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileLogic : MonoBehaviour, IStatedItem<ETileLogicState>
    {
        protected TileObject tileObject;

        private void Awake()
        {
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);
            SubscribeToStateChangedEvent();
        }

        private void OnDestroy()
        {
            UnSubscribeFromStateChangedEvent();
        }

        //~IStatedItem
        public async Task Init()
        {
            //..Nothing
        }

        public void SubscribeToStateChangedEvent()
        {
            tileObject.GetState().GetLogicStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            tileObject.GetState().GetLogicStateMachine().UnsubscribeToStateChangedCallback(this);
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
        //~IStatedItem End

        public void OnStartAttackPreview()
        {
            tileObject.GetState().GetViewStateMachine().SetState(ETileViewState.PreviewAttack);
        }

        public void OnStartMovePreview()
        {
            tileObject.GetState().GetViewStateMachine().SetState(ETileViewState.PreviewMove);
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
            TileEvents.InvokeOnTileSelected(this, new TileEvents.OnTileSelectedPayload(tileObject));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ETileViewState state = tileObject.GetState().GetViewStateMachine().GetState();
            if (state is ETileViewState.PreviewAttack or ETileViewState.PreviewMove) return;
            tileObject.GetState().GetViewStateMachine().SetState(ETileViewState.Idle);
        }
    }
}