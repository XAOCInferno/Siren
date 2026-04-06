using System;
using Gameplay.Tile;
using NUnit.Framework;
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
        //Logic
        private readonly EnumStateMachine<EPieceLogicState> _logicStateMachine = new();
        public EnumStateMachine<EPieceLogicState> GetLogicStateMachine() => _logicStateMachine;

        //View
        private readonly EnumStateMachine<EPieceViewState> _viewStateMachine = new();
        public EnumStateMachine<EPieceViewState> GetViewStateMachine() => _viewStateMachine;

        //Grid
        private Vector2Int[] _possibleMovementLocations = Array.Empty<Vector2Int>();

        public bool interactable;
        protected Player.Player ownerPlayer;

        private PieceObject _pieceObject;

        private void Awake()
        {
            _pieceObject = GetComponent<PieceObject>();

            Assert.NotNull(_pieceObject);
        }

        public void SetOwnerPlayer(Player.Player player)
        {
            ownerPlayer = player;
        }

        public Player.Player GetOwnerPlayer() => ownerPlayer;

        public void SetPossibleMovementLocations(Vector2Int[] locations)
        {
            _possibleMovementLocations = locations;
        }

        public Vector2Int[] GetPossibleMovementLocations() => _possibleMovementLocations;
    }
}