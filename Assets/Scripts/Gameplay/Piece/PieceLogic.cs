using NUnit.Framework;
using UnityEngine.EventSystems;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    //TODO: Grid location inheritable? Maybe an interface for something that can be placed on the board
    public class PieceLogic : PooledObject, IStateObject<EPieceLogicState>, IPointerEnterHandler, IPointerExitHandler,
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
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }

        public override void SetInActive()
        {
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
            if (_pieceObject.GetState().GetLogicStateMachine().GetState() != EPieceLogicState.SelectedOnBoard)
            {
                _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Hovered);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //TODO: If can select...
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.SelectedOnBoard);
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Selected);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_pieceObject.GetState().GetLogicStateMachine().GetState() != EPieceLogicState.SelectedOnBoard)
            {
                _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Idle);
            }
        }
    }
}