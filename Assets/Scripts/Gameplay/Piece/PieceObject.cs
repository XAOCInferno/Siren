using System;
using System.Threading.Tasks;
using Behaviours;
using Debug;
using NUnit.Framework;
using UnityEngine;
using Utils;

namespace Gameplay.Piece
{
    public class PieceObject : StateViewLogicObject<PieceState, PieceLogic, PieceView>, IBoardObject
    {
        [SerializeField] private Transform tileConnectionMkr;
        public Transform GetTileConnectionMkr() => tileConnectionMkr;
        [SerializeField] private Transform tileScaleMkr;
        public Transform GetTileScaleMkr() => tileScaleMkr;

        [SerializeField] protected DynamicObject dynamicObject;

        //~IBoardObject
        public void OnGridLocationSet()
        {
            GetLogic().UpdateTilesInMovementRange();
            GetView().UpdateSelectionPreview();
        }
        //~IBoardObject End

        public async Task Init()
        {
            try
            {
                Assert.NotNull(dynamicObject);
                await GetLogic().Init();
                await GetView().Init();
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to Init due to {e.Message}");
                throw;
            }
        }

        public DynamicObject GetMoveableObject() => dynamicObject;
    }
}