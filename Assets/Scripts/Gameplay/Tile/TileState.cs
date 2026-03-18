using Gameplay.Piece;
using JetBrains.Annotations;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public enum ETileState
    {
        Idle,
        OccupiedByPiece
    }

    public class TileState : MonoBehaviour
    {
        [CanBeNull] protected PieceLogic occupiedByPieceLogic = null;
        private Vector2Int _gridLocation;

        private readonly EnumStateMachine<ETileState> _stateMachine = new();
        public EnumStateMachine<ETileState> GetStateMachine() => _stateMachine;

        public void SetOccupier(PieceLogic pieceLogic)
        {
            occupiedByPieceLogic = pieceLogic;
            _stateMachine.SetState(ETileState.OccupiedByPiece);
        }

        public void ClearOccupier()
        {
            occupiedByPieceLogic = null;
            _stateMachine.SetState(ETileState.Idle);
        }

        public bool GetIsOccupiedByPiece() => _stateMachine.GetState() == ETileState.OccupiedByPiece;

        public void SetGridLocation(Vector2Int location)
        {
            _gridLocation = location;
        }

        public Vector2Int GetGridLocation() => _gridLocation;
    }
}