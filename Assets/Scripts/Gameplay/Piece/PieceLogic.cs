using Debug;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    //TODO: Grid location inheritable? Maybe an interface for something that can be placed on the board
    public class PieceLogic : PooledObject, IStateObject<EPieceState>
    {
        private PieceState _state;
        private readonly PieceData _pieceData;

        private Vector2Int _gridLocation;

        private void Awake()
        {
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

        public override void SetActive()
        {
            _state.GetStateMachine().SetState(EPieceState.OnBoard);
        }

        public override void SetInActive()
        {
            _state.GetStateMachine().SetState(EPieceState.NotInPlay);
        }

        public int OnStateChanged(EnumStateMachine<EPieceState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case EPieceState.NotInPlay:
                    gameObject.SetActive(false);
                    break;
                case EPieceState.OnBoard:
                    gameObject.SetActive(true);
                    break;
            }

            return 0;
        }

        public void SetGridLocation(Vector2Int location)
        {
            _gridLocation = location;
        }
    }
}