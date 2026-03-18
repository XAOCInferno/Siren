using Gameplay.Piece;
using JetBrains.Annotations;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileState : MonoBehaviour
    {
        protected bool isOccupiedByPiece = false;
        [CanBeNull] protected PieceLogic occupiedByPieceLogic = null;
        private Vector2Int _gridLocation;

        private readonly EnumStateMachine<EPieceState> _stateMachine = new();
        public EnumStateMachine<EPieceState> GetStateMachine() => _stateMachine;

        public void SetOccupier(PieceLogic pieceLogic)
        {
            isOccupiedByPiece = true;
            occupiedByPieceLogic = pieceLogic;
        }

        public void ClearOccupier()
        {
            isOccupiedByPiece = false;
            occupiedByPieceLogic = null;
        }
        
        public bool GetIsOccupiedByPiece() => isOccupiedByPiece;

        public void SetGridLocation(Vector2Int location)
        {
            _gridLocation = location;
        }

        public Vector2Int GetGridLocation() => _gridLocation;
    }
}