using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    public enum EPieceLogicState
    {
        NotInPlay = 0,
        IdleOnBoard,
        SelectedOnBoard
    }
    public enum EPieceViewState
    {
        Idle,
        Hovered,
        Selected
    }


    public class PieceState : MonoBehaviour
    {
        private Vector2Int _gridLocation;
        
        //Logic
        private readonly EnumStateMachine<EPieceLogicState> _logicStateMachine = new();
        public EnumStateMachine<EPieceLogicState> GetLogicStateMachine() => _logicStateMachine;
        //View
        private readonly EnumStateMachine<EPieceViewState> _viewStateMachine = new();
        public EnumStateMachine<EPieceViewState> GetViewStateMachine() => _viewStateMachine;

        public void SetGridLocation(Vector2Int location)
        {
            _gridLocation = location;
        }
    }
}