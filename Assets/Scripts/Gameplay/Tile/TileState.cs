using Gameplay.Piece;
using JetBrains.Annotations;
using NUnit.Framework;
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
        PreviewAttack,
        PreviewPlayCard
    }

    public class TileState : MonoBehaviour
    {
        private readonly EnumStateMachine<ETileLogicState> _logicStateMachine = new();
        public EnumStateMachine<ETileLogicState> GetLogicStateMachine() => _logicStateMachine;
        private readonly EnumStateMachine<ETileViewState> _viewStateMachine = new();
        public EnumStateMachine<ETileViewState> GetViewStateMachine() => _viewStateMachine;


        [CanBeNull] private PieceLogic _occupiedByPieceLogic = null;
        private Vector2Int _gridLocation;

        protected TileObject tileObject;

        private void Awake()
        {
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);
        }

        public void SetOccupier(PieceObject pieceLogic)
        {
            // Cache the occupied by piece logic
            _occupiedByPieceLogic = pieceLogic.GetLogic();

            // Set position of tile so that the connection markers are touching
            Transform pieceTransform = pieceLogic.transform;
            pieceTransform.parent = tileObject.GetMoveableObject().transform;
            pieceTransform.localPosition = tileObject.GetPieceConnectionMkr().transform.localPosition +
                                           (pieceLogic.GetTileConnectionMkr().transform.localPosition * -1);

            // Update state
            _logicStateMachine.SetState(ETileLogicState.OccupiedByPiece);
        }

        public PieceLogic GetOccupier() => _occupiedByPieceLogic;

        public void ClearOccupier()
        {
            _occupiedByPieceLogic = null;
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