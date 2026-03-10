using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    public class Tile : Behaviours.MoveableObject, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;
        protected Vector3 startingPos = Vector3.zero;
        
        private void Start()
        {
            startingPos = transform.position;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MoveToLocation(startingPos + (Vector3.up * yMoveOnHover), moveTime);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MoveToLocation(startingPos, moveTime);
        }
    }
}