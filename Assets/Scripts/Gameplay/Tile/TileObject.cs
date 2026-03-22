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

        protected DynamicObject dynamicObject;

        public override void Awake()
        {
            base.Awake();
            dynamicObject = GetComponent(typeof(DynamicObject)) as DynamicObject;
            Assert.NotNull(dynamicObject);
        }

        public async Task Init()
        {
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