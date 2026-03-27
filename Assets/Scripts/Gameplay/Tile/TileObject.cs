using System;
using System.Threading.Tasks;
using Behaviours;
using Debug;
using NUnit.Framework;
using UnityEngine;
using Utils;


namespace Gameplay.Tile
{
    public class TileObject : StateViewLogicObject<TileState, TileLogic, TileView>
    {
        [SerializeField] protected Transform pieceConnectionMkr;

        [SerializeField] protected DynamicObject dynamicObject;

        public async Task Init()
        {
            // Assert for required objects
            Assert.NotNull(pieceConnectionMkr);
            Assert.NotNull(dynamicObject);
            
            try
            {
                await GetLogic().Init();
                await GetView().Init();
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to Init due to {e.Message}");
                throw; // TODO handle exception
            }
        }

        public Transform GetPieceConnectionMkr() => pieceConnectionMkr;

        public DynamicObject GetMoveableObject() => dynamicObject;
    }
}