using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomCamera;
using Debug;
using Gameplay.Tile;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    public class PieceView : MonoBehaviour, IStatedItem<EPieceLogicState>, IStatedItem<EPieceViewState>
    {
        [SerializeField] private GameObject pieceMeshObject;

        private MeshRenderer _meshRenderer;

        private PieceState _state;

        private readonly Dictionary<EPieceViewState, Material> _materialMap = new();

        protected List<TileObject> currentPreviewedTiles = new();
        private PieceObject _pieceObject;

        private void Awake()
        {
            //Get our object
            _pieceObject = GetComponent<PieceObject>();
            Assert.NotNull(_pieceObject);

            //Mesh
            _meshRenderer = pieceMeshObject.GetComponent<MeshRenderer>();

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
            try
            {
                //Addressables
                await LoadAddressables();
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to Init due to {e.Message}");
                throw;
            }
        }

        public void SubscribeToStateChangedEvent()
        {
            _state = GetComponent<PieceState>();
            if (!_state)
            {
                DebugSystem.Error("CardState not found");
                return;
            }

            _state.GetLogicStateMachine().SubscribeToStateChangedCallback(this);
            _state.GetViewStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            _state.GetLogicStateMachine().UnsubscribeToStateChangedCallback(this);
            _state.GetViewStateMachine().UnsubscribeToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<EPieceLogicState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case EPieceLogicState.NotInPlay:
                    pieceMeshObject.SetActive(false);
                    break;
                case EPieceLogicState.IdleOnBoard:
                    pieceMeshObject.SetActive(true);
                    break;
            }

            return 0;
        }

        public int OnStateChanged(EnumStateMachine<EPieceViewState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case EPieceViewState.Idle:
                    //Clear selection specific if we are no longer selected
                    if (payload.oldState == EPieceViewState.Selected)
                    {
                        ClearSelection();
                    }

                    //Set idle state
                    SetIdle();
                    break;
                case EPieceViewState.Hovered:
                    //Clear selection specific if we are no longer selected
                    if (payload.oldState == EPieceViewState.Selected)
                    {
                        ClearSelection();
                    }

                    //Set hovered
                    SetHovered();
                    break;
                case EPieceViewState.Selected:
                    //Change view
                    CameraSubsystem.GetMainCamera().ChangeCameraViewMode(ECameraViewMode.Board);

                    //Set selected state
                    SetSelected();
                    break;
            }

            return 0;
        }
        //~IStatedItem End

        private async Task LoadAddressables()
        {
            try
            {
                //Load addressables
                var loadIdleMaterialTask = AddressablesSystem<Material>.GetOrLoadAddressable("M_PieceIdle.mat");
                var loadHoverMaterialTask = AddressablesSystem<Material>.GetOrLoadAddressable("M_PieceHover.mat");
                var loadSelectedMaterialTask = AddressablesSystem<Material>.GetOrLoadAddressable("M_PieceSelected.mat");

                //Results
                await loadIdleMaterialTask;
                Assert.NotNull(loadIdleMaterialTask.Result);
                await loadHoverMaterialTask;
                Assert.NotNull(loadHoverMaterialTask.Result);
                await loadSelectedMaterialTask;
                Assert.NotNull(loadSelectedMaterialTask.Result);

                //Set in data
                _materialMap.Add(EPieceViewState.Idle, loadIdleMaterialTask.Result);
                _materialMap.Add(EPieceViewState.Hovered, loadHoverMaterialTask.Result);
                _materialMap.Add(EPieceViewState.Selected, loadSelectedMaterialTask.Result);
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to load addressables due to {e.Message}");
                throw;
            }
        }

        public void SetMeshScale(Vector3 scale)
        {
            _pieceObject.GetTileScaleMkr().localScale = scale;
        }

        public void SetMesh(Mesh newMesh)
        {
            MeshFilter meshFilter = pieceMeshObject.GetComponent<MeshFilter>();
            Assert.IsNotNull(meshFilter);
            meshFilter.mesh = newMesh;
        }

        protected void ClearSelection()
        {
            //Clear previewed if we had any
            ClearAnyPreviewedTiles();

            //Return to hand
            CameraSubsystem.GetMainCamera().ChangeCameraViewMode(ECameraViewMode.Hand);
        }

        public void SetHovered()
        {
            //Clear previewed if we had any
            ClearAnyPreviewedTiles();

            if (!_materialMap.TryGetValue(EPieceViewState.Hovered, out var mat)) return;
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }

        public void SetSelected()
        {
            //Get our movement settings
            TileObject[] tilesInMovementRange;
            Vector2Int gridLocation = _pieceObject.GetState().GetGridLocation();
            PieceData pieceData = _pieceObject.GetLogic().GetPieceData();
            EPieceMovementType movementType = pieceData.GetMovementType();
            int movementSpeed = pieceData.GetBaseMovement();

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
                    tilesInMovementRange =
                        BoardSystem<TileObject>.GetItemsInCross(gridLocation, movementSpeed).foundItems;
                    break;
                case EPieceMovementType.Diagonal:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInDiagonalCross(gridLocation, movementSpeed)
                        .foundItems;
                    break;
                case EPieceMovementType.Star:
                    tilesInMovementRange =
                        BoardSystem<TileObject>.GetItemsInStar(gridLocation, movementSpeed).foundItems;
                    break;
                case EPieceMovementType.Circle:
                    tilesInMovementRange =
                        BoardSystem<TileObject>.GetItemsInCircle(gridLocation, movementSpeed).foundItems;
                    break;
                case EPieceMovementType.Square:
                    tilesInMovementRange =
                        BoardSystem<TileObject>.GetItemsInSquare(gridLocation, movementSpeed).foundItems;
                    break;
                case EPieceMovementType.LShaped:
                    tilesInMovementRange = BoardSystem<TileObject>.GetItemsInLShapeCross(gridLocation, movementSpeed)
                        .foundItems;
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
                if (occupier && occupier.GetComponent<PieceState>().GetOwnerPlayer() !=
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

            //Change to selected material
            if (!_materialMap.TryGetValue(EPieceViewState.Selected, out var mat)) return;
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }

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

        public void SetIdle()
        {
            //Clear previewed if we had any
            ClearAnyPreviewedTiles();

            if (!_materialMap.TryGetValue(EPieceViewState.Idle, out var mat)) return;
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }
    }
}