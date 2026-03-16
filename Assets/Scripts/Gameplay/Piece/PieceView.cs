using Debug;
using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Piece
{
    public class PieceView : MonoBehaviour, IStateObject<EPieceState>
    {
        private PieceState _state;

        private void Awake()
        {
            ListenToStateChangedEvent();
        }

        public void ListenToStateChangedEvent()
        {
            _state = GetComponent<PieceState>();
            if (!_state)
            {
                DebugSystem.Error("CardState not found");
                return;
            }

            _state.GetStateMachine().ListenToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<EPieceState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                //..Nothing yet
            }

            return 0;
        }
    }
}