using NUnit.Framework;
using UnityEngine;


namespace Gameplay.Tile
{
    public class TileObject : MonoBehaviour
    {
        [SerializeField] protected Transform pieceConnectionMkr;
        
        private TileState _state;
        private TileView _view;
        private TileLogic _tileLogic;

        public void Awake()
        {
            _view = GetComponent<TileView>();
            _tileLogic = GetComponent<TileLogic>();
            _state = GetComponent<TileState>();

            Assert.NotNull(_state);
            Assert.NotNull(_view);
            Assert.NotNull(_tileLogic);
        }

        public TileState GetState() => _state;
        public TileLogic GetLogic() => _tileLogic;
        public TileView GetView() => _view;
        
        public Transform GetPieceConnectionMkr() => pieceConnectionMkr;
    }
}