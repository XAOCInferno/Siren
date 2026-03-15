using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Piece", order = 2)]
    public class PieceData : ScriptableObject
    {
        [SerializeField] private int pieceID;

        public PieceData()
        {
            pieceID = 0;
        }
    }
}