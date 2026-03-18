using Behaviours;
using Gameplay.Card;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileLogic : MonoBehaviour, IStateObject<ETileState>
    {
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;

        protected Vector3 startingPos = Vector3.zero;

        protected TileObject tileObject;
        protected MoveableObject moveableObject;

        private void Awake()
        {
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);
            moveableObject = GetComponent(typeof(MoveableObject)) as MoveableObject;
            Assert.NotNull(moveableObject);
            ListenToStateChangedEvent();
        }

        private void Start()
        {
            startingPos = transform.position;
        }

        public void ListenToStateChangedEvent()
        {
            tileObject.GetState().GetStateMachine().ListenToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<ETileState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ETileState.OccupiedByPiece:
                    OnTileOccupied();
                    break;
            }

            return 0;
        }

        //Not called directly on this object, requires 1 or more TileInputTrigger that will inform this
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!tileObject.GetState().GetIsOccupiedByPiece())
            {
                moveableObject.MoveToLocation(startingPos + (Vector3.up * yMoveOnHover), moveTime);
            }
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
            MoveToIdlePosition();
        }

        protected void OnTileOccupied()
        {
            MoveToIdlePosition();
        }

        protected void MoveToIdlePosition()
        {
            moveableObject.MoveToLocation(startingPos, moveTime);
        }
    }
}