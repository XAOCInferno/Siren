using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Debug;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileView : MonoBehaviour, IStateObject<ETileViewState>
    {
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;
        [SerializeField] private MeshRenderer[] meshRenderers;

        protected Vector3 startingPos = Vector3.zero;

        protected TileObject tileObject;


        private readonly Dictionary<ETileViewState, Material> _materialMap = new();

        private void Awake()
        {
            //Get our parent object
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);

            //Get Start pos
            startingPos = transform.position;

            //Now subscribe to our State Machine
            SubscribeToStateChangedEvent();
        }

        private void OnDestroy()
        {
            UnSubscribeToStateChangedEvent();
        }

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
            tileObject.GetState().GetViewStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeToStateChangedEvent()
        {
            tileObject.GetState().GetViewStateMachine().UnsubscribeToStateChangedCallback(this);
        }

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

                //Results
                await loadIdleMaterialTask;
                Assert.NotNull(loadIdleMaterialTask.Result);
                await loadPreviewAttackMaterialTask;
                Assert.NotNull(loadPreviewAttackMaterialTask.Result);
                await loadPreviewMoveMaterialTask;
                Assert.NotNull(loadPreviewMoveMaterialTask.Result);

                //Set in data
                _materialMap.Add(ETileViewState.Idle, loadIdleMaterialTask.Result);
                _materialMap.Add(ETileViewState.PreviewMove, loadPreviewMoveMaterialTask.Result);
                _materialMap.Add(ETileViewState.PreviewAttack, loadPreviewAttackMaterialTask.Result);
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to load addressables due to {e.Message}");
                throw;
            }
        }

        public int OnStateChanged(EnumStateMachine<ETileViewState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ETileViewState.Idle:
                    MoveToIdlePosition();
                    SetIdleMaterial();
                    break;
                case ETileViewState.Hovered:
                    MoveToActivePosition();
                    SetIdleMaterial();
                    break;
                case ETileViewState.PreviewAttack:
                    MoveToActivePosition();
                    SetPreviewAttackMaterial();
                    break;
                case ETileViewState.PreviewMove:
                    MoveToActivePosition();
                    SetPreviewMoveMaterial();
                    break;
            }

            return 0;
        }

        protected void MoveToActivePosition()
        {
            tileObject.GetMoveableObject().MoveToLocation(startingPos + (Vector3.up * yMoveOnHover), moveTime);
        }

        protected void MoveToIdlePosition()
        {
            tileObject.GetMoveableObject().MoveToLocation(startingPos, moveTime);
        }

        protected void SetIdleMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.Idle, out var mat)) return;
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.SetMaterials(new List<Material> { mat });
            }
        }

        protected void SetPreviewAttackMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.PreviewAttack, out var mat)) return;
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.SetMaterials(new List<Material> { mat });
            }
        }

        protected void SetPreviewMoveMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.PreviewMove, out var mat)) return;
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.SetMaterials(new List<Material> { mat });
            }
        }
    }
}