using System.Collections.Generic;
using Debug;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    public class PieceView : MonoBehaviour, IStateObject<EPieceLogicState>, IStateObject<EPieceViewState>
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        [SerializeField] private Mesh pieceMesh;
        [SerializeField] private GameObject pieceMeshObject;

        private MeshRenderer _meshRenderer;

        private PieceState _state;

        private readonly Dictionary<EPieceViewState, Material> _materialMap = new();

        private void Awake()
        {
            //All pieces must have a mesh
            Assert.NotNull(pieceMesh);

            //Addressables
            LoadAddressables();

            //Set mesh
            SetMesh(pieceMesh);
            _meshRenderer = pieceMeshObject.GetComponent<MeshRenderer>();

            //Subscribe to state machine
            ListenToStateChangedEvent();
        }

        private void LoadAddressables()
        {
            //Load addressables
            var loadHoverMaterial = Addressables.LoadAssetAsync<Material>(
                "PieceHoverMaterial.mat");
            var idleHoverMaterial = Addressables.LoadAssetAsync<Material>(
                "PieceIdleMaterial.mat");
            var selectedHoverMaterial = Addressables.LoadAssetAsync<Material>(
                "PieceSelectedMaterial.mat");
            //Results
            loadHoverMaterial.Completed += h => { _materialMap.Add(EPieceViewState.Hovered, h.Result); };
            idleHoverMaterial.Completed += h => { _materialMap.Add(EPieceViewState.Idle, h.Result); };
            selectedHoverMaterial.Completed += h => { _materialMap.Add(EPieceViewState.Selected, h.Result); };
        }

        public void ListenToStateChangedEvent()
        {
            _state = GetComponent<PieceState>();
            if (!_state)
            {
                DebugSystem.Error("CardState not found");
                return;
            }

            _state.GetLogicStateMachine().ListenToStateChangedCallback(this);
            _state.GetViewStateMachine().ListenToStateChangedCallback(this);
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
            print(payload.newState);
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
            Assert.NotNull(meshFilter);
            meshFilter.mesh = newMesh;
        }

        public void SetHovered()
        {
            Material mat = _materialMap[EPieceViewState.Hovered];
            Assert.NotNull(mat);
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }

        public void SetSelected()
        {
            Material mat = _materialMap[EPieceViewState.Selected];
            Assert.NotNull(mat);
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }

        public void SetIdle()
        {
            Material mat = _materialMap[EPieceViewState.Idle];
            Assert.NotNull(mat);
            print(mat);
            _meshRenderer.SetMaterials(new List<Material> { mat });
        }
    }
}