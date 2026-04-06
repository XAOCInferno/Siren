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
        private PieceState _state;

        private Mesh _currentMesh;
        private Material _currentMaterial;
        private readonly Dictionary<EPieceViewState, Material> _materialMap = new();

        protected List<TileObject> currentPreviewedTiles = new();
        private PieceObject _pieceObject;
        private Vector3 _lastPosition = Vector3.zero;
        private Transform _cachedDynamicTransform;

        private InstancedMeshRendererSingleton.MeshInstancingTransformDetails _cachedMeshInstancingTransformDetails;
        private KeyValuePair<Mesh, Material> _cachedMeshMaterialPair;

        private int _batchIdx = -1;

        private void Awake()
        {
            //Get our object
            _pieceObject = GetComponent<PieceObject>();
            Assert.NotNull(_pieceObject);

            _cachedDynamicTransform = _pieceObject.GetMoveableObject().transform;

            _pieceObject.GetMoveableObject().BindToOnMovementCallback(gameObject, () =>
            {
                UpdateMeshInstancing();
                return 0;
            });

            //Subscribe to state machine
            SubscribeToStateChangedEvent();
        }

        private void Update()
        {
            // Update instancing if we move
            // TODO: once we make this moveable then we can bind to that callback instead of in update
            if (_cachedDynamicTransform.position == _lastPosition) return;
            _lastPosition = _cachedDynamicTransform.position;
            UpdateMeshInstancing();
        }

        private void OnDestroy()
        {
            _pieceObject.GetMoveableObject().UnBindToOnMovementCallback(gameObject);
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
                    if (_batchIdx > -1)
                    {
                        InstancedMeshRendererSingleton.instance.TryRemoveMeshInstancing(_batchIdx,
                            gameObject.GetInstanceID());
                    }

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

                //Default material
                SetMaterial(loadIdleMaterialTask.Result);
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to load addressables due to {e.Message}");
                throw;
            }
        }

        public void SetMeshScale(Vector3 scale)
        {
            _cachedDynamicTransform.localScale = scale;
            UpdateMeshInstancing();
        }

        public void SetMesh(Mesh newMesh)
        {
            _currentMesh = newMesh;
            _cachedMeshMaterialPair = new KeyValuePair<Mesh, Material>(_currentMesh, _currentMaterial);
            UpdateMeshInstancing();
        }

        public void SetMaterial(Material newMat)
        {
            _currentMaterial = newMat;
            _cachedMeshMaterialPair = new KeyValuePair<Mesh, Material>(_currentMesh, _currentMaterial);
            UpdateMeshInstancing();
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

            //Update material
            if (!_materialMap.TryGetValue(EPieceViewState.Hovered, out var mat)) return;
            SetMaterial(mat);
        }

        public void SetSelected()
        {
            //Change to selected material
            if (!_materialMap.TryGetValue(EPieceViewState.Selected, out var mat)) return;
            SetMaterial(mat);

            UpdateSelectionPreview();
        }

        public void UpdateSelectionPreview()
        {
            // Only update if we're selected
            if(_pieceObject.GetState().GetViewStateMachine().GetState() != EPieceViewState.Selected) return;
            
            // Clear
            ClearAnyPreviewedTiles();
            
            // Get tiles we are in range of
            Vector2Int[] validMoveableLocations = _pieceObject.GetState().GetPossibleMovementLocations();
            TileObject[] tilesInMovementRange = new TileObject[validMoveableLocations.Length];
            for (int a = 0; a < validMoveableLocations.Length; a++)
            {
                tilesInMovementRange[a] = BoardSystem<TileObject>.GetItemOnGrid(validMoveableLocations[a]).Key;
            }

            // Preview movement on all tiles in range, if any
            for (int i = 0; i < tilesInMovementRange.Length; i++)
            {
                // Check is not null
                if (!tilesInMovementRange[i]) continue;

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
        }

        public void ClearAnyPreviewedTiles()
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

            //Update the material
            if (!_materialMap.TryGetValue(EPieceViewState.Idle, out var mat)) return;
            SetMaterial(mat);
        }

        public void UpdateMeshInstancing()
        {
            //Check we have required mesh and mat
            if (!_currentMaterial || !_currentMesh) return;

            //Update cached data
            _cachedMeshInstancingTransformDetails.location = _cachedDynamicTransform.position;
            _cachedMeshInstancingTransformDetails.rotation = _cachedDynamicTransform.rotation;
            _cachedMeshInstancingTransformDetails.scale = _cachedDynamicTransform.lossyScale;

            //Now update instancing
            _batchIdx = InstancedMeshRendererSingleton.instance.AddMeshInstancing(_batchIdx,
                gameObject.GetInstanceID(), _cachedMeshMaterialPair, _cachedMeshInstancingTransformDetails);
        }
    }
}