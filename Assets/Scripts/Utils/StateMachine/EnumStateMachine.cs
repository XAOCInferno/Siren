using System;
using Debug;

namespace Utils.StateMachine
{
    /// <summary>
    /// Generic state machine for Enums
    /// </summary>
    /// <typeparam name="T">The enum this state machine handles</typeparam>
    public class EnumStateMachine<T> where T : Enum
    {
        //Event payload for when a state changes
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

        //Our current state
        private T _state;

        //Callback on state change
        protected Func<StateChangedEventPayload, int> onStateChangeCallback;

        //Changes our state, use this instead of a direct change to _state variable
        public void SetState(T newState)
        {
            //Check if we need to change state, if not return early
            if (_state.Equals(newState)) return;
            
            //Save old state and set new state
            T oldState = _state;
            _state = newState;
            
            //Log
            DebugSystem.Log($"State changed from {oldState} to {newState}");
            
            //Call callback to inform other systems who are subscribed
            onStateChangeCallback?.Invoke(new StateChangedEventPayload(oldState, newState));
        }

        public T GetState() => _state;

        /// <summary>
        /// Subscribe an IStatedItem to listen to our callback
        /// </summary>
        /// <param name="statedItem">The object that wants to listen where T is the enum this state machine is responsible for</param>
        public void SubscribeToStateChangedCallback(IStatedItem<T> statedItem)
        {
            onStateChangeCallback += statedItem.OnStateChanged;
        }
        
        /// <summary>
        /// Unsubscribe an IStatedItem to listen to our callback
        /// </summary>
        /// <param name="statedItem">The object that wants to listen where T is the enum this state machine is responsible for</param>
        public void UnsubscribeToStateChangedCallback(IStatedItem<T> statedItem)
        {
            onStateChangeCallback -= statedItem.OnStateChanged;
        }
    }
}