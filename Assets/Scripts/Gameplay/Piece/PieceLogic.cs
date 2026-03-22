using System.Threading.Tasks;
using CustomCamera;
using Global;
using Interaction;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    //TODO: Grid location inheritable? Maybe an interface for something that can be placed on the board
    public class PieceLogic : MonoBehaviour, IPooledItem, IStatedItem<EPieceLogicState>, IInteractable,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        private PieceObject _pieceObject;
        private PieceData _pieceData;

        private void Awake()
        {
            //Get our object
            _pieceObject = GetComponent<PieceObject>();
            Assert.NotNull(_pieceObject);

            //Subscribe to events
            CameraEvents.OnCameraMoved += OnCameraMoved;

            //Subscribe to state machine
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
            _pieceObject.GetState().GetLogicStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            _pieceObject.GetState().GetLogicStateMachine().UnsubscribeToStateChangedCallback(this);
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
        //~IStatedItem End

        //~IPooledItem
        public void SetActive()
        {
            InteractionSystem.SetInteractable(this, true);
            InteractionSystem.SetIdle(this);
        }

        public void SetInActive()
        {
            InteractionSystem.SetInteractable(this, false);
            InteractionSystem.SetIdle(this);
        }
        //~IPooledItem End

        //~IPointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            //TODO: If can select...
            if (_pieceObject.GetState().GetLogicStateMachine().GetState() != EPieceLogicState.SelectedOnBoard &&
                _pieceObject.GetState().interactable)
            {
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
        //~IPointer Events End

        //~IInteractable, Do not call directly instead let service call this
        public void SetIdle()
        {
            //State
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Idle);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }

        public void SetHovered()
        {
            //State
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Hovered);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }

        public void SetSelected()
        {
            //Select the piece
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.SelectedOnBoard);
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Selected);
        }

        public void SetInteractable(bool interactable)
        {
            _pieceObject.GetState().interactable = interactable;
        }
        //~IInteractable End

        public PieceData GetPieceData() => _pieceData;

        public void SetPieceData(PieceData newPieceData)
        {
            //Our Data
            _pieceData = newPieceData;

            //Set Mesh
            _pieceObject.GetView().SetMesh(_pieceData.GetMesh());
            _pieceObject.GetView().SetMeshScale(Vector3.one * _pieceData.GetBaseSize());
        }

        //TODO: Block interaction when not in board state
        protected void OnCameraMoved([CanBeNull] object sender, CameraEvents.CameraMovedEventPayload payload)
        {
            //If we move to anywhere except the board then return
            if (payload.newMode == ECameraViewMode.Board) return;

            //We've moved away from board, so unselect this card
            EnumStateMachine<EPieceViewState> stateMachine = _pieceObject.GetState().GetViewStateMachine();
            if (stateMachine.GetState() == EPieceViewState.Selected)
            {
                InteractionSystem.SetIdle(this);
            }
        }
    }
}