using Debug;
using Utils;

namespace Gameplay
{
    public enum EPieceState
    {
        NotInPlay = 0,
        OnBoard,
    }

    public class Piece : PooledObject
    {
        private EPieceState _state;
        private readonly PieceData _pieceData;

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
    }
}