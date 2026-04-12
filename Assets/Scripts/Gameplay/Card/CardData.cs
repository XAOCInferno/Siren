using System;
using Gameplay.Piece;
using Gameplay.Resources;
using JetBrains.Annotations;
using UI;
using UnityEngine;

namespace Gameplay.Card
{
    [Serializable]
    public struct ResourceChange
    {
        public EResourceType resourceType;
        public int decrease;
        public int increase;
    }
    
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Gameplay/Card", order = 1)]
    public class CardData : ScriptableObject
    {
        
        [SerializeField] private ResourceChange[] resourceChangeBy;

        [SerializeField] private ECardType type;

        [CanBeNull] [SerializeField] private PieceData associatedPieceData;

        //View
        [SerializeField] private CardViewModelData viewData;

        public ECardType GetCardType() => type;
        public PieceData GetAssociatedPieceData() => associatedPieceData;
        public CardViewModelData GetViewData() => viewData;
        public ResourceChange[] GetResourceChangeBy() => resourceChangeBy;
    }
}