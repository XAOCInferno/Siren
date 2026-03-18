using NUnit.Framework;
using UnityEngine;
using Utils;


namespace Gameplay.Tile
{
    public class TileObject : StateViewLogicObject<TileState, TileLogic, TileView>
    {
        [SerializeField] protected Transform pieceConnectionMkr;
        
        public Transform GetPieceConnectionMkr() => pieceConnectionMkr;
    }
}