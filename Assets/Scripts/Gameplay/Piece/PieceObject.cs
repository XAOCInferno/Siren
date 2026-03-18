using UnityEngine;
using Utils;

namespace Gameplay.Piece
{
    public class PieceObject : StateViewLogicObject<PieceState, PieceLogic, PieceView>
    {
        [SerializeField] private Transform tileConnectionMkr;
        public Transform GetTileConnectionMkr() => tileConnectionMkr;
    }
}