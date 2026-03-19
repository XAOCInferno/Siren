using UnityEngine;

namespace Gameplay.Piece
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Piece", order = 2)]
    public class PieceData : ScriptableObject
    {
        [SerializeField] private int baseMovement;

        public int GetBaseMovement() => baseMovement;

        public PieceData(int baseMovement)
        {
            this.baseMovement = baseMovement;
        }
    }
}