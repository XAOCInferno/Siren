using Global;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    public class Tile : Behaviours.MoveableObject, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] protected Transform pieceConnectionMkr;
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;

        private Vector2Int _gridLocation;
        protected Vector3 startingPos = Vector3.zero;

        protected bool isOccupiedByPiece = false;
        [CanBeNull] protected Piece occupiedByPiece = null;

        private void Start()
        {
            startingPos = transform.position;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MoveToLocation(startingPos + (Vector3.up * yMoveOnHover), moveTime);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Ensure this is valid
            if (isOccupiedByPiece || CardService.localCardLogicBeingPlayed == null) return;

            //Valid, get data and spawn piece
            CardService.localCardLogicBeingPlayed.PlayCard(_gridLocation);

            //Clear
            CardService.ClearCardBeingPlayed();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MoveToLocation(startingPos, moveTime);
        }

        public Transform GetPieceConnectionMkr() => pieceConnectionMkr;

        public void SetGridLocation(Vector2Int location)
        {
            _gridLocation = location;
        }

        public void SetOccupier(Piece piece)
        {
            isOccupiedByPiece = true;
            occupiedByPiece = piece;
        }

        public void ClearOccupier()
        {
            isOccupiedByPiece = false;
            occupiedByPiece = null;
        }
    }
}