using System;
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

    // Valid shapes an object supports
    // Theoretically we can support all shapes that are defined in grid types, but will require 3D mesh for them
    // TODO: Design what else we may want
    public enum EPieceSizeShapeTypes
    {
        Circle = 0,
        Square,
    }

    // The valid sizes we currently support. This can be increased
    // TODO: Perhaps this can be exposed somewhere for design?
    public enum EPieceSizes
    {
        Small = 0,
        Medium,
        Large,
        ExtraLarge
    }

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Piece", order = 2)]
    public class PieceData : ScriptableObject
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private int baseMovement;
        [SerializeField] private EPieceMovementType movementType;
        [SerializeField] private EPieceSizes baseSizeType;
        [SerializeField] private EPieceSizeShapeTypes sizeShapeType;

        // Graphics
        public Mesh GetMesh() => mesh;

        // Movement
        public int GetBaseMovement() => baseMovement;

        public EPieceMovementType GetMovementType() => movementType;

        // Size
        public int GetBaseSize()
        {
            return baseSizeType switch
            {
                EPieceSizes.Small => 1,
                EPieceSizes.Medium => 2,
                EPieceSizes.Large => 3,
                EPieceSizes.ExtraLarge => 4,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public int GetMeshScale() => GetBaseSize() * 2;
        public EPieceSizeShapeTypes GetSizeShape() => sizeShapeType;
    }
}