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
        Square,
        LShaped,
    }

    //Theoretically we can support all shapes that are defined in grid types, but will require 3D mesh for them
    //TODO: Design what else we may want
    public enum EPieceSizeShapeTypes
    {
        Circle = 0,
        Square,
    }

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Piece", order = 2)]
    public class PieceData : ScriptableObject
    {
        [SerializeField] private int baseMovement;
        [SerializeField] private EPieceMovementType movementType;
        [SerializeField] private int baseSize;
        [SerializeField] private EPieceSizeShapeTypes sizeShapeType;

        public PieceData(int baseMovement, EPieceMovementType movementType, int baseSize, EPieceSizeShapeTypes sizeShapeType)
        {
            this.baseMovement = baseMovement;
            this.movementType = movementType;
            this.baseSize = baseSize;
            this.sizeShapeType = sizeShapeType;
        }

        // Movement
        public int GetBaseMovement() => baseMovement;
        public EPieceMovementType GetMovementType() => movementType;
        // Size
        public int GetBaseSize() => baseSize;
        public EPieceSizeShapeTypes GetSizeShape() => sizeShapeType;
    }
}