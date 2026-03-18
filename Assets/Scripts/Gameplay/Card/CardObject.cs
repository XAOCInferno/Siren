using NUnit.Framework;
using UnityEngine;

namespace Gameplay.Card
{
    public class CardObject : MonoBehaviour
    {
        private CardState _state;
        private CardView _view;
        private CardLogic _tileLogic;

        public void Awake()
        {
            _view = GetComponent<CardView>();
            _tileLogic = GetComponent<CardLogic>();
            _state = GetComponent<CardState>();

            Assert.NotNull(_state);
            Assert.NotNull(_view);
            Assert.NotNull(_tileLogic);
        }

        public CardState GetState() => _state;
        public CardLogic GetLogic() => _tileLogic;
        public CardView GetView() => _view;
    }
}