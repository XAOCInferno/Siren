using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Tile
{
    public class TileInputTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private TileObject tileObject;

        //IPointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            Assert.IsNotNull(tileObject);
            tileObject.GetLogic().OnPointerEnter(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Assert.IsNotNull(tileObject);
            tileObject.GetLogic().OnPointerClick(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Assert.IsNotNull(tileObject);
            tileObject.GetLogic().OnPointerExit(eventData);
        }
    }
}