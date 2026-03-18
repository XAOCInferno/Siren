using Debug;
using NUnit.Framework;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    public class PieceView : MonoBehaviour, IStateObject<EPieceState>
    {
        [SerializeField] private Mesh pieceMesh;
        [SerializeField] private GameObject pieceMeshObject;

        private PieceState _state;

        private void Awake()
        {
            //All pieces must have a mesh
            Assert.NotNull(pieceMesh);

            //Set mesh
            SetMesh(pieceMesh);

            //Subscribe to state machine
            ListenToStateChangedEvent();
        }

        public void ListenToStateChangedEvent()
        {
            _state = GetComponent<PieceState>();
            if (!_state)
            {
                DebugSystem.Error("CardState not found");
                return;
            }

            _state.GetStateMachine().ListenToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<EPieceState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case EPieceState.NotInPlay:
                    pieceMeshObject.SetActive(false);
                    break;
                case EPieceState.IdleOnBoard:
                    pieceMeshObject.SetActive(true);
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
    }
}