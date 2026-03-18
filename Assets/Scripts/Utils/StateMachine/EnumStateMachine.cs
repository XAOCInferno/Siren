using System;
using Debug;

namespace Utils.StateMachine
{
    public class EnumStateMachine<T> where T : Enum
    {
        public class StateChangedEventPayload : EventArgs
        {
            public readonly T oldState;
            public readonly T newState;

            public StateChangedEventPayload(T oldState, T newState)
            {
                this.oldState = oldState;
                this.newState = newState;
            }
        }

        private T _state;

        protected Func<StateChangedEventPayload, int> onStateChangeCallback;

        public void SetState(T newState)
        {
            if (_state.Equals(newState)) return;
            //Diff state
            T oldState = _state;
            _state = newState;
            DebugSystem.Log($"State changed from {oldState} to {newState}");
            //Callback
            onStateChangeCallback?.Invoke(new StateChangedEventPayload(oldState, newState));
        }

        public T GetState()
        {
            return _state;
        }

        public void ListenToStateChangedCallback(IStateObject<T> stateObject)
        {
            onStateChangeCallback += stateObject.OnStateChanged;
        }
    }
}