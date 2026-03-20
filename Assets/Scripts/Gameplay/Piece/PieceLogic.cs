using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Debug;
using Gameplay.Tile;
using Interaction;
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

        protected List<TileObject> currentPreviewedTiles = new();

        private void Awake()
        {
            _pieceObject = GetComponent<PieceObject>();
            Assert.NotNull(_pieceObject);
            _pieceObject.GetState().GetLogicStateMachine().SubscribeToStateChangedCallback(this);
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

            //Clear previewed if we had any
            ClearAnyPreviewedTiles();
        }

        public void SetHovered()
        {
            //State
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Hovered);
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);

            //Clear previewed if we had any
            ClearAnyPreviewedTiles();
        }

        public void SetSelected()
        {
            //Get our movement settings
            TileObject[] tilesInMovementRange;
            Vector2Int gridLocation = _pieceObject.GetState().GetGridLocation();
            EPieceMovementType movementType = _pieceData.GetMovementType();
            int movementSpeed = _pieceData.GetBaseMovement();

            //Select the piece
            _pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.SelectedOnBoard);
            _pieceObject.GetState().GetViewStateMachine().SetState(EPieceViewState.Selected);

            //If piece cannot move, we can return early now
            if (movementType == EPieceMovementType.None || movementSpeed == 0) return;

            //Preview move logic
            //Get tiles that are in our range
            switch (movementType)
            {
                case EPieceMovementType.Cross:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInCross(gridLocation, movementSpeed);
                    break;
                case EPieceMovementType.Diagonal:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInDiagonalCross(gridLocation, movementSpeed);
                    break;
                case EPieceMovementType.Star:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInStar(gridLocation, movementSpeed);
                    break;
                case EPieceMovementType.Circle:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInCircle(gridLocation, movementSpeed);
                    break;
                case EPieceMovementType.Square:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInSquare(gridLocation, movementSpeed);
                    break;
                case EPieceMovementType.LShaped:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInLShapeCross(gridLocation, movementSpeed);
                    break;
                default:
                    DebugSystem.Warn(
                        $"Unexpected movement type {movementType}, this is not supported. Piece will be treated as immovable, though code should have returned early before thie point.");
                    return;
            }

            //Preview movement on all tiles in range, if any
            for (int i = 0; i < tilesInMovementRange.Length; i++)
            {
                //Check if the tiles are occupied by an enemy, if they are, then we preview for attack, otherwise for move
                var occupier = tilesInMovementRange[i].GetState().GetOccupier();
                if (occupier && occupier._pieceObject.GetState().GetOwnerPlayer() !=
                    _pieceObject.GetState().GetOwnerPlayer())
                {
                    tilesInMovementRange[i].GetLogic().OnStartAttackPreview();
                }
                else
                {
                    tilesInMovementRange[i].GetLogic().OnStartMovePreview();
                }
            }

            //Save a copy so we can deactivate them later
            currentPreviewedTiles = tilesInMovementRange.ToList();
        }

        public void SetInteractable(bool interactable)
        {
            _pieceObject.GetState().interactable = interactable;
        }
        //~IInteractable End

        protected void ClearAnyPreviewedTiles()
        {
            //Return early if we have no tiles to prevent null ref in foreach
            if (currentPreviewedTiles.Count == 0) return;

            //Iterate over all tiels and reset them to idle state
            foreach (TileObject previewedTile in currentPreviewedTiles)
            {
                previewedTile.GetState().GetViewStateMachine().SetState(ETileViewState.Idle);
            }

            //Clear our list
            currentPreviewedTiles.Clear();
        }

        public void SetCardData(PieceData newPieceData)
        {
            //Our Data
            _pieceData = newPieceData;
        }
    }
}