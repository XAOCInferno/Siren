using System;
using System.Threading.Tasks;
using Debug;
using UnityEngine;
using Utils;

namespace Gameplay.Piece
{
    public class PieceObject : StateViewLogicObject<PieceState, PieceLogic, PieceView>
    {
        [SerializeField] private Transform tileConnectionMkr;
        public Transform GetTileConnectionMkr() => tileConnectionMkr;

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
    }
}