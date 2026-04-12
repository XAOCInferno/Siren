using System;
using System.Threading.Tasks;
using Debug;
using Utils;

namespace Gameplay.Resources
{
    public class ResourcePipeObject : StateViewLogicObject<ResourcePipeState, ResourcePipeLogic, ResourcePipeView>
    {
        public override void Awake()
        {
            base.Awake();
            Init();
        }

        public async Task Init()
        {
            try
            {
                await GetView().Init();
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to Init due to {e.Message}");
                throw;
            }
        }
    }
}