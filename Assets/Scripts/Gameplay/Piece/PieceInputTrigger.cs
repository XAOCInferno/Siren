using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Piece
{
    public class PieceInputTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private PieceObject pieceObject;

        //~IPointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            Assert.IsNotNull(pieceObject);
            pieceObject.GetLogic().OnPointerEnter(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Assert.IsNotNull(pieceObject);
            pieceObject.GetLogic().OnPointerClick(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Assert.IsNotNull(pieceObject);
            pieceObject.GetLogic().OnPointerExit(eventData);
        }
//~IPointer Events End
    }
}