using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Debug;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    public class PieceView : MonoBehaviour, IStateObject<EPieceLogicState>, IStateObject<EPieceViewState>
    {
        [SerializeField] private Mesh pieceMesh;
        [SerializeField] private GameObject pieceMeshObject;

        private MeshRenderer _meshRenderer;

        private PieceState _state;

        private readonly Dictionary<EPieceViewState, Material> _materialMap = new();

        private void Awake()
        {
            //All pieces must have a mesh
            Assert.NotNull(pieceMesh);

            //Set mesh
            SetMesh(pieceMesh);
            _meshRenderer = pieceMeshObject.GetComponent<MeshRenderer>();

            //Subscribe to state machine
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
        public void UnSubscribeToStateChangedEvent()
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
                    SetIdle();
                    break;
                case EPieceViewState.Hovered:
                    SetHovered();
                    break;
                case EPieceViewState.Selected:
                    SetSelected();
                    break;
            }

            return 0;
        }

        protected void SetMesh(Mesh newMesh)
        {
            MeshFilter meshFilter = pieceMeshObject.GetComponent<MeshFilter>();
            Assert.IsNotNull(meshFilter);
            meshFilter.mesh = newMesh;
        }

        public void SetHovered()
        {
            if (!_materialMap.TryGetValue(EPieceViewState.Hovered, out var mat)) return;
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }

        public void SetSelected()
        {
            if (!_materialMap.TryGetValue(EPieceViewState.Selected, out var mat)) return;
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }

        public void SetIdle()
        {
            if (!_materialMap.TryGetValue(EPieceViewState.Idle, out var mat)) return;
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }
    }
}