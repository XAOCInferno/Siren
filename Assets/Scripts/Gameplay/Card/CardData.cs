using JetBrains.Annotations;
using UI;
using UnityEngine;

namespace Gameplay.Card
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Card", order = 1)]
    public class CardData : ScriptableObject
    {
        [SerializeField] private int cardID;
        [SerializeField] private ECardType type;

        [CanBeNull] [SerializeField] private PieceData associatedPieceData;

        //View
        [SerializeField] private CardViewModelData viewData;


        public int GetCardID() => cardID;
        public ECardType GetCardType() => type;
        public PieceData GetAssociatedPieceData() => associatedPieceData;
        public CardViewModelData GetViewData() => viewData;
    }
}