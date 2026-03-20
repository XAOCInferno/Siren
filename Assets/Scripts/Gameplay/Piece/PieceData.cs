using UnityEngine;

namespace Gameplay.Piece
{
    public enum EPieceMovementType
    {
        None = 0,
        Cross,
        Diagonal,
        Star,
        Circle,
        Square
    }

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Piece", order = 2)]
    public class PieceData : ScriptableObject
    {
        [SerializeField] private int baseMovement;
        [SerializeField] private EPieceMovementType movementType;

        public PieceData(int baseMovement, EPieceMovementType movementType)
        {
            this.baseMovement = baseMovement;
            this.movementType = movementType;
        }

        public int GetBaseMovement() => baseMovement;
        public EPieceMovementType GetMovementType() => movementType;
    }
}