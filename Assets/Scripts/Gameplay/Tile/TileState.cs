using System;
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

        private const float OccupierMoveDurationPerMeter = 0.1f;


        [CanBeNull] private PieceObject _occupiedByPieceObject = null;

        protected TileObject tileObject;

        private void Awake()
        {
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);
        }

        public void SetOccupier(PieceObject pieceObject)
        {
            // Get state and view
            PieceState pieceState = pieceObject.GetState();
            PieceView pieceView = pieceObject.GetView();

            // Cache the occupied by piece logic
            _occupiedByPieceObject = pieceObject;

            // Set position of tile so that the connection markers are touching
            pieceObject.transform.parent = tileObject.GetMoveableObject().transform;
            Vector3 currentLocalPos = pieceObject.transform.localPosition;
            Vector3 desiredLocalPos = tileObject.GetPieceConnectionMkr().transform.localPosition +
                                      (pieceObject.GetTileConnectionMkr().transform.localPosition * -1);

            // Get our move duration
            float distance = Vector3.Distance(currentLocalPos, desiredLocalPos);
            float moveDuration = OccupierMoveDurationPerMeter * distance;

            // Move
            pieceView.ClearAnyPreviewedTiles();
            pieceObject.GetMoveableObject().MoveTo(desiredLocalPos,
                pieceState.isOnTile ? moveDuration : 0, false, (v) =>
                {
                    pieceView.UpdateSelectionPreview();
                    return 0;
                });

            // Inform piece it is actively on the tile
            pieceState.isOnTile = true;

            // Update state
            _logicStateMachine.SetState(ETileLogicState.OccupiedByPiece);
        }

        public PieceObject GetOccupier() => _occupiedByPieceObject;

        public void ClearOccupier()
        {
            // Clear the occupier
            if (_occupiedByPieceObject)
            {
                _occupiedByPieceObject.transform.parent = null;
                _occupiedByPieceObject = null;
            }

            // Set state
            _logicStateMachine.SetState(ETileLogicState.Idle);
        }

        public bool GetIsOccupiedByPiece() => _logicStateMachine.GetState() == ETileLogicState.OccupiedByPiece;
    }
}