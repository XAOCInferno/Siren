using Gameplay.Card;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Tile
{
    public class TileLogic : Behaviours.MoveableObject
    {
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;

        protected Vector3 startingPos = Vector3.zero;

        protected TileObject tileObject;

        private void Awake()
        {
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);
        }

        private void Start()
        {
            startingPos = transform.position;
        }

        //Not called directly on this object, requires 1 or more TileInputTrigger that will inform this
        public void OnPointerEnter(PointerEventData eventData)
        {
            MoveToLocation(startingPos + (Vector3.up * yMoveOnHover), moveTime);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //State
            TileState state = tileObject.GetState();

            //Ensure this is valid
            if (state.GetIsOccupiedByPiece() || CardService.localCardLogicBeingPlayed == null) return;

            //Valid, get data and spawn piece
            CardService.localCardLogicBeingPlayed.PlayCard(state.GetGridLocation());

            //Clear
            CardService.ClearCardBeingPlayed();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MoveToLocation(startingPos, moveTime);
        }
    }
}