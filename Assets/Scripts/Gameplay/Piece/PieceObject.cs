using NUnit.Framework;
using UnityEngine;

namespace Gameplay.Piece
{
    public class PieceObject : MonoBehaviour
    {
        [SerializeField] private Transform tileConnectionMkr;
        
        private PieceState _state;
        private PieceView _view;
        private PieceLogic _tileLogic;

        public void Awake()
        {
            _view = GetComponent<PieceView>();
            _tileLogic = GetComponent<PieceLogic>();
            _state = GetComponent<PieceState>();

            Assert.NotNull(_state);
            Assert.NotNull(_view);
            Assert.NotNull(_tileLogic);
        }

        public PieceState GetState() => _state;
        public PieceLogic GetLogic() => _tileLogic;
        public PieceView GetView() => _view;
        
        public Transform GetTileConnectionMkr() => tileConnectionMkr;
    }
}