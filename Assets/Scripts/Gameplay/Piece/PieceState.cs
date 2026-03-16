using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    public enum EPieceState
    {
        NotInPlay = 0,
        OnBoard,
    }

    public class PieceState : MonoBehaviour
    {
        private readonly EnumStateMachine<EPieceState> _stateMachine = new();
        public EnumStateMachine<EPieceState> GetStateMachine() => _stateMachine;
    }
}