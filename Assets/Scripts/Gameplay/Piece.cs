using Debug;
using UnityEngine;
using Utils;

namespace Gameplay
{
    public enum EPieceState
    {
        NotInPlay = 0,
        OnBoard,
    }

    //TODO: Grid location inheritable? Maybe an interface for something that can be placed on the board
    public class Piece : PooledObject
    {
        [SerializeField] private Transform tileConnectionMkr;
        private EPieceState _state;
        private readonly PieceData _pieceData;

        public Transform GetTileConnectionMkr() => tileConnectionMkr;

        private Vector2Int _gridLocation;

        public override void SetActive()
        {
            SetState(EPieceState.OnBoard);
        }

        public override void SetInActive()
        {
            SetState(EPieceState.NotInPlay);
        }

        public void SetState(EPieceState newState)
        {
            _state = newState;
            DebugSystem.Log($"Piece {gameObject.name} state changed to {newState}");
            switch (newState)
            {
                case EPieceState.NotInPlay:
                    gameObject.SetActive(false);
                    break;
                case EPieceState.OnBoard:
                    gameObject.SetActive(true);
                    break;
            }
        }

        public void SetGridLocation(Vector2Int location)
        {
            _gridLocation = location;
        }
    }
}