using Gameplay.Piece;
using JetBrains.Annotations;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public enum ETileLogicState
    {
        None = 0,
        Idle,
        OccupiedByPiece
    }
    
    public enum ETileViewState
    {
        None = 0,
        Idle,
        Hovered,
        PreviewMove,
        PreviewAttack
    }

    public class TileState : MonoBehaviour
    {
        private readonly EnumStateMachine<ETileLogicState> _logicStateMachine = new();
        public EnumStateMachine<ETileLogicState> GetLogicStateMachine() => _logicStateMachine;
        private readonly EnumStateMachine<ETileViewState> _viewStateMachine = new();
        public EnumStateMachine<ETileViewState> GetViewStateMachine() => _viewStateMachine;
        
        
        [CanBeNull] protected PieceLogic occupiedByPieceLogic = null;
        private Vector2Int _gridLocation;

        public void SetOccupier(PieceLogic pieceLogic)
        {
            occupiedByPieceLogic = pieceLogic;
            _logicStateMachine.SetState(ETileLogicState.OccupiedByPiece);
        }

        public void ClearOccupier()
        {
            occupiedByPieceLogic = null;
            _logicStateMachine.SetState(ETileLogicState.Idle);
        }

        public bool GetIsOccupiedByPiece() => _logicStateMachine.GetState() == ETileLogicState.OccupiedByPiece;

        public void SetGridLocation(Vector2Int location)
        {
            _gridLocation = location;
        }

        public Vector2Int GetGridLocation() => _gridLocation;
    }
}