using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomCamera;
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

namespace Gameplay.Piece
{
    //TODO: Grid location inheritable? Maybe an interface for something that can be placed on the board
    public class PieceLogic : MonoBehaviour, IPooledItem, IStatedItem<EPieceLogicState>, IInteractable
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
            gameObject.SetActive(true);
        }

        public void SetInActive()
        {
            InteractionSystem.SetInteractable(this, false);
            gameObject.SetActive(false);
        }
        //~IPooledItem End

        //~IInteractable, Do not call directly instead let service call this
        public void SetIdle()
        {
            //State
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Idle);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);

            //Clear if we're currently selected
            if (GameplaySystem.GetPieceBeingControlled() == _pieceObject)
            {
                GameplaySystem.ClearPieceBeingControlled();
            }
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

            //Save the new local card
            GameplaySystem.SetPieceBeingControlled(_pieceObject);
        }

        public void SetInteractable(bool interactable)
        {
            PieceState state =  _pieceObject.GetState();
            state.interactable = interactable;
            
            if (!interactable && state.GetLogicStateMachine().GetState() != EPieceLogicState.NotInPlay)
            {
                state.GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
            }
        }
        //~IInteractable End

        public PieceData GetPieceData() => _pieceData;

        public void SetPieceData(PieceData newPieceData)
        {
            // Our Data
            _pieceData = newPieceData;

            // Set Mesh
            _pieceObject.GetView().SetMesh(_pieceData.GetMesh());
            _pieceObject.GetView().SetMeshScale(Vector3.one * _pieceData.GetMeshScale());
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

        //Pointer events, called by PieceInputTrigger
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

        public void ClearTilesInMovementRange()
        {
            _pieceObject.GetState().SetPossibleMovementLocations(Array.Empty<Vector2Int>());
        }
        
        public void UpdateTilesInMovementRange()
        {
            // Ensure we have data set
            if(!_pieceData) return;
            
            // Get state
            PieceState state = _pieceObject.GetState();

            // Get our movement settings
            List<KeyValuePair<TileObject, Vector2Int>> responsesInMovementRange = new();
            Vector2Int gridLocation = BoardSystem<PieceObject>.GetItemLocationOnGrid(_pieceObject);
            PieceData pieceData = GetPieceData();
            EPieceMovementType movementType = pieceData.GetMovementType();
            int movementSpeed = pieceData.GetBaseMovement();

            // If piece cannot move, we can return early now
            if (movementType == EPieceMovementType.None || movementSpeed == 0) return;

            // Preview move logic
            // Get tiles that are in our range
            switch (movementType)
            {
                case EPieceMovementType.Cross:
                    responsesInMovementRange.AddRange(BoardSystem<TileObject>
                        .GetItemsInCross(gridLocation, movementSpeed)
                        .foundItems);
                    break;
                case EPieceMovementType.Diagonal:
                    responsesInMovementRange.AddRange(BoardSystem<TileObject>
                        .GetItemsInDiagonalCross(gridLocation, movementSpeed)
                        .foundItems);
                    break;
                case EPieceMovementType.Star:
                    responsesInMovementRange.AddRange(
                        BoardSystem<TileObject>.GetItemsInStar(gridLocation, movementSpeed).foundItems);
                    break;
                case EPieceMovementType.Circle:
                    responsesInMovementRange.AddRange(
                        BoardSystem<TileObject>.GetItemsInCircle(gridLocation, movementSpeed).foundItems);
                    break;
                case EPieceMovementType.Square:
                    responsesInMovementRange.AddRange(
                        BoardSystem<TileObject>.GetItemsInSquare(gridLocation, movementSpeed).foundItems);
                    break;
                case EPieceMovementType.LShaped:
                    responsesInMovementRange.AddRange(BoardSystem<TileObject>
                        .GetItemsInLShapeCross(gridLocation, movementSpeed)
                        .foundItems);
                    break;
                default:
                    DebugSystem.Warn(
                        $"Unexpected movement type {movementType}, this is not supported. Piece will be treated as immovable, though code should have returned early before thie point.");
                    return;
            }

            // Extract values
            Vector2Int[] tilesInMovementRange = new Vector2Int[responsesInMovementRange.Count];
            for (int i = 0; i < responsesInMovementRange.Count; i++)
            {
                tilesInMovementRange[i] = responsesInMovementRange[i].Value;
            }

            // Set in state
            state.SetPossibleMovementLocations(tilesInMovementRange);
        }
    }
}