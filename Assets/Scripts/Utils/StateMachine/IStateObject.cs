using System;

namespace Utils.StateMachine
{
    public interface IStateObject<T> where T : Enum
    {
        public void ListenToStateChangedEvent();
        public int OnStateChanged(EnumStateMachine<T>.StateChangedEventPayload payload);
    }
}