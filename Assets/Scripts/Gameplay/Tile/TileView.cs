using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Debug;
using Gameplay.Card;
using Gameplay.Piece;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileView : MonoBehaviour, IStatedItem<ETileViewState>
    {
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;

        protected Vector3 startingPos = Vector3.zero;

        private readonly List<TileObject> _currentlyPreviewedTiles = new();

        protected TileObject tileObject;

        private Transform _cachedDynamicTransform;

        private readonly Dictionary<ETileViewState, Material> _materialMap = new();

        private InstancedMeshRendererSingleton.MeshInstancingTransformDetails _cachedMeshInstancingTransformDetails;
        private KeyValuePair<Mesh, Material> _cachedMeshMaterialPair;
        private int _cachedInstanceID;

        private int _batchIdx = -1;

        private void Awake()
        {
            //Get our parent object
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);

            //Get Start pos
            startingPos = transform.position;

            //Caching
            _cachedDynamicTransform = tileObject.GetMoveableObject().transform;
            _cachedInstanceID = gameObject.GetInstanceID();

            //Now subscribe to our State Machine
            SubscribeToStateChangedEvent();
        }

        private void OnDestroy()
        {
            UnSubscribeFromStateChangedEvent();
            UnSubscribeFromCallbacks();
        }

        //~IStatedItem
        public async Task Init()
        {
            try
            {
                //Addressables
                await LoadAddressables();
                tileObject.GetMoveableObject().BindToOnMovementCallback(gameObject, () =>
                {
                    UpdateMeshInstancing();
                    return 0;
                });
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to Init due to {e.Message}");
                throw;
            }
        }

        public void SubscribeToStateChangedEvent()
        {
            tileObject.GetState().GetViewStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            tileObject.GetMoveableObject().UnBindToOnMovementCallback(gameObject);
        }

        public void UnSubscribeFromCallbacks()
        {
            tileObject.GetState().GetViewStateMachine().UnsubscribeToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<ETileViewState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ETileViewState.Idle:
                    //Hide preview if we were previously previewing piece
                    if (payload.oldState == ETileViewState.Hovered && !GameplaySystem.GetCardBeingPlayed())
                    {
                        PiecePreviewSingleton.instance.Hide();
                    }

                    //Hide any other tiles we're previewing
                    for (int i = 0; i < _currentlyPreviewedTiles.Count; i++)
                    {
                        _currentlyPreviewedTiles[i].GetState().GetViewStateMachine().SetState(ETileViewState.Idle);
                    }

                    _currentlyPreviewedTiles.Clear();

                    //Set idle state
                    MoveToIdlePosition();
                    SetIdleMaterial();
                    break;
                case ETileViewState.Hovered:
                    CardObject card = GameplaySystem.GetCardBeingPlayed();
                    if (card)
                    {
                        HandleCardPlayPreview(card);
                    }
                    else
                    {
                        MoveToActivePosition();
                        SetIdleMaterial();
                    }

                    break;
                case ETileViewState.PreviewAttack:
                    PiecePreviewSingleton.instance.Hide();
                    MoveToActivePosition();
                    SetPreviewAttackMaterial();
                    break;
                case ETileViewState.PreviewMove:
                    PiecePreviewSingleton.instance.Hide();
                    MoveToActivePosition();
                    SetPreviewMoveMaterial();
                    break;
                case ETileViewState.PreviewPlayCard:
                    MoveToActivePosition();
                    SetIdleMaterial();
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
                var loadIdleMaterialTask = AddressablesSystem<Material>.GetOrLoadAddressable("M_TileIdle.mat");
                var loadPreviewAttackMaterialTask =
                    AddressablesSystem<Material>.GetOrLoadAddressable("M_TilePreviewAttack.mat");
                var loadPreviewMoveMaterialTask =
                    AddressablesSystem<Material>.GetOrLoadAddressable("M_TilePreviewMove.mat");
                var loadDefaultMesh =
                    AddressablesSystem<Mesh>.GetOrLoadAddressable("DefaultTileBase.fbx");

                //Results
                await loadIdleMaterialTask;
                Assert.NotNull(loadIdleMaterialTask.Result);
                await loadPreviewAttackMaterialTask;
                Assert.NotNull(loadPreviewAttackMaterialTask.Result);
                await loadPreviewMoveMaterialTask;
                Assert.NotNull(loadPreviewMoveMaterialTask.Result);
                await loadDefaultMesh;
                Assert.NotNull(loadDefaultMesh.Result);

                //Set in data
                _materialMap.Add(ETileViewState.Idle, loadIdleMaterialTask.Result);
                _materialMap.Add(ETileViewState.PreviewMove, loadPreviewMoveMaterialTask.Result);
                _materialMap.Add(ETileViewState.PreviewAttack, loadPreviewAttackMaterialTask.Result);

                //Set mesh
                SetMesh(loadDefaultMesh.Result);
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to load addressables due to {e.Message}");
                throw;
            }
        }

        public void SetScale(Vector3 scale)
        {
            _cachedDynamicTransform.localScale = scale;
            UpdateMeshInstancing();
        }

        public void SetMesh(Mesh newMesh)
        {
            // Update mesh if we are not already assigned it
            if (_cachedMeshMaterialPair.Key == newMesh) return;
            _cachedMeshMaterialPair = new KeyValuePair<Mesh, Material>(newMesh, _cachedMeshMaterialPair.Value);
            UpdateMeshInstancing();
        }

        public void SetMaterial(Material newMat)
        {
            // Update material if we are not already assigned it
            if (_cachedMeshMaterialPair.Value == newMat) return;
            _cachedMeshMaterialPair = new KeyValuePair<Mesh, Material>(_cachedMeshMaterialPair.Key, newMat);
            UpdateMeshInstancing();
        }


        protected void MoveToActivePosition()
        {
            tileObject.GetMoveableObject().MoveTo(startingPos + (Vector3.up * yMoveOnHover), moveTime, true);
        }

        protected void MoveToIdlePosition()
        {
            tileObject.GetMoveableObject().MoveTo(startingPos, moveTime, true);
        }

        protected void SetIdleMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.Idle, out var mat)) return;
            SetMaterial(mat);
        }

        protected void SetPreviewAttackMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.PreviewAttack, out var mat)) return;
            SetMaterial(mat);
        }

        protected void SetPreviewMoveMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.PreviewMove, out var mat)) return;
            SetMaterial(mat);
        }

        protected void HandleCardPlayPreview(CardObject card)
        {
            var response = GameplaySystem.GetTilesPieceWouldOccupy(
                card.GetLogic().GetCardData().GetAssociatedPieceData(),
                BoardSystem<TileObject>.GetItemLocationOnGrid(tileObject));

            for (int i = 0; i < response.foundItems.Length; i++)
            {
                response.foundItems[i].Key.GetState().GetViewStateMachine().SetState(ETileViewState.PreviewPlayCard);
                _currentlyPreviewedTiles.Add(response.foundItems[i].Key);
            }

            PiecePreviewSingleton.instance.PreviewPieceOnTile(
                card.GetLogic().GetCardData().GetAssociatedPieceData(), tileObject);
        }


        protected void UpdateMeshInstancing()
        {
            //Check we have required mesh and mat
            if (!_cachedMeshMaterialPair.Key || !_cachedMeshMaterialPair.Value) return;

            //Update cached data
            _cachedMeshInstancingTransformDetails.location = _cachedDynamicTransform.position;
            _cachedMeshInstancingTransformDetails.rotation = _cachedDynamicTransform.rotation;
            _cachedMeshInstancingTransformDetails.scale = GetVisualHalfScale();

            //Now update instancing
            _batchIdx = InstancedMeshRendererSingleton.instance.AddMeshInstancing(_batchIdx, _cachedInstanceID,
                _cachedMeshMaterialPair, _cachedMeshInstancingTransformDetails);
        }

        protected Vector3 GetVisualHalfScale()
        {
            return _cachedDynamicTransform.lossyScale / 2;
        }
    }
}