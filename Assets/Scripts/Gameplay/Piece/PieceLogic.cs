using Interaction;
using NUnit.Framework;
using UnityEngine.EventSystems;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    //TODO: Grid location inheritable? Maybe an interface for something that can be placed on the board
    public class PieceLogic : PooledObject, IStateObject<EPieceLogicState>, IInteractable, IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        private PieceObject _pieceObject;
        private readonly PieceData _pieceData;

        private void Awake()
        {
            _pieceObject = GetComponent<PieceObject>();
            Assert.NotNull(_pieceObject);
            ListenToStateChangedEvent();
        }

        public void ListenToStateChangedEvent()
        {
            _pieceObject.GetState().GetLogicStateMachine().ListenToStateChangedCallback(this);
        }

        public override void SetActive()
        {
            InteractionSystem.SetInteractable(this, true);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }

        public override void SetInActive()
        {
            InteractionSystem.SetInteractable(this, false);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.NotInPlay);
        }

        public int OnStateChanged(EnumStateMachine<EPieceLogicState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case EPieceLogicState.NotInPlay:
                    gameObject.SetActive(false);
                    break;
                case EPieceLogicState.IdleOnBoard:
                    gameObject.SetActive(true);
                    break;
            }

            return 0;
        }

        //IPointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            //TODO: If can select...
            if (_pieceObject.GetState().GetLogicStateMachine().GetState() != EPieceLogicState.SelectedOnBoard &&
                _pieceObject.GetState().interactable)
            {
                print("TRYING TO HOVER");
                InteractionSystem.SetHovered(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //TODO: If can select...
            if (_pieceObject.GetState().interactable)
            {
                InteractionSystem.SetSelected(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_pieceObject.GetState().GetLogicStateMachine().GetState() != EPieceLogicState.SelectedOnBoard &&
                _pieceObject.GetState().interactable)
            {
                InteractionSystem.SetIdle(this);
            }
        }

        //IInteractable, Do not call directly instead let service call this
        public void SetIdle()
        {
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Idle);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }

        public void SetHovered()
        {
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Hovered);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }

        public void SetSelected()
        {
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.SelectedOnBoard);
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Selected);
        }

        public void SetInteractable(bool interactable)
        {
            _pieceObject.GetState().interactable = interactable;
        }
    }
}