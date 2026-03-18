using NUnit.Framework;
using UnityEngine;

namespace Utils
{
    public class StateViewLogicObject<TState, TLogic, TView> : MonoBehaviour
    {
        private TState _state;
        private TView _view;
        private TLogic _tileLogic;

        public void Awake()
        {
            _view = GetComponent<TView>();
            _tileLogic = GetComponent<TLogic>();
            _state = GetComponent<TState>();

            Assert.NotNull(_state);
            Assert.NotNull(_view);
            Assert.NotNull(_tileLogic);
        }

        public TState GetState() => _state;
        public TLogic GetLogic() => _tileLogic;
        public TView GetView() => _view;
    }
}