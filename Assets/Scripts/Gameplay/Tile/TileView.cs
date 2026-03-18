using NUnit.Framework;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileView : MonoBehaviour, IStateObject<ETileState>
    {
        protected TileObject tileObject;

        private void Awake()
        {
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);
            ListenToStateChangedEvent();
        }

        public void ListenToStateChangedEvent()
        {
            tileObject.GetState().GetStateMachine().ListenToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<ETileState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
            }

            return 0;
        }
    }
}