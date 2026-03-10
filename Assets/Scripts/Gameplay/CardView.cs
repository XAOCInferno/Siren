using UnityEngine;

namespace Gameplay
{
    public class CardView : Behaviours.MoveableObject
    {
        protected Vector3 desiredPosition = Vector3.zero;
        protected Vector3 desiredOffset = Vector3.zero;

        public void SetDesiredPosition(Vector3 position, float moveTime)
        {
            desiredPosition = position;
            MoveToLocation(desiredPosition + desiredOffset, moveTime);
        }
        
        public void SetDesiredOffset(Vector3 offset, float moveTime)
        {
            desiredOffset = offset;
            MoveToLocation(desiredPosition + desiredOffset, moveTime);
        }
    }
}