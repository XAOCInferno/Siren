using JetBrains.Annotations;
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Card", order = 1)]
    public class CardData : ScriptableObject
    {
        [SerializeField] private int cardID;
        [SerializeField] private ECardType type;
        [CanBeNull] [SerializeField] private PieceData associatedPieceData;

        public int GetCardID() => cardID;
        public ECardType GetCardType() => type;
        public PieceData GetAssociatedPieceData() => associatedPieceData;
    }
}