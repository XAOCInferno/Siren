using System;
using System.Threading.Tasks;

namespace Utils.StateMachine
{
    public interface IStateObject<T> where T : Enum
    {
        public Task Init();
        public void SubscribeToStateChangedEvent();
        public void UnSubscribeFromStateChangedEvent();
        public int OnStateChanged(EnumStateMachine<T>.StateChangedEventPayload payload);
    }
}