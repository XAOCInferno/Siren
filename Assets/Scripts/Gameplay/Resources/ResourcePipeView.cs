using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Debug;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Resources
{
    public class ResourcePipeView : MonoBehaviour, IStatedItem<EResourcePipeState>
    {
        private ResourcePipeObject _resourcePipeObject;

        private readonly Dictionary<EResourcePipeState, Material> _materialMap = new();

        private MeshRenderer _meshRenderer;

        private void OnDestroy()
        {
            UnSubscribeFromStateChangedEvent();
        }

        //~IStatedItem
        public async Task Init()
        {
            try
            {
                //Addressables
                _resourcePipeObject = GetComponent<ResourcePipeObject>();
                _meshRenderer = GetComponent<MeshRenderer>();
                SubscribeToStateChangedEvent();
                await LoadAddressables();
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to Init due to {e.Message}");
                throw;
            }
        }

        public void SubscribeToStateChangedEvent()
        {
            _resourcePipeObject.GetState().GetStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            _resourcePipeObject.GetState().GetStateMachine().UnsubscribeToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<EResourcePipeState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                //..Nothing right now
            }

            // Set material
            _materialMap.TryGetValue(payload.newState, out Material newMaterial);
            if (newMaterial)
            {
                SetMaterial(newMaterial);
            }
            else
            {
                DebugSystem.Warn($"Cannot change ResourcePipeMaterial as none found for enum {payload.newState}");
            }

            return 0;
        }
        //~IStatedItem End

        private async Task LoadAddressables()
        {
            try
            {
                //Get mat type
                string offlineMaterialName;
                string poweredMaterialName;
                string costPreviewMaterialName;
                string gainPreviewMaterialName;
                if (_resourcePipeObject.GetState().GetRepresentsLocalPlayer())
                {
                    offlineMaterialName = "M_PipeEnergyOwnOffline";
                    poweredMaterialName = "M_PipeEnergyOwnPowered";
                    costPreviewMaterialName = "M_PipeEnergyOwnPreview";
                    gainPreviewMaterialName = "M_PipeEnergyOwnGainPreview";
                }
                else
                {
                    offlineMaterialName = "M_PipeEnergyOpponentOffline";
                    poweredMaterialName = "M_PipeEnergyOpponentPowered";
                    costPreviewMaterialName = "M_PipeEnergyOpponentCostPreview";
                    gainPreviewMaterialName = "M_PipeEnergyOpponentGainPreview";
                }

                //Load addressables
                var loadOfflineMaterialTask = AddressablesSystem<Material>.GetOrLoadAddressable(offlineMaterialName);
                var loadPoweredMaterialTask = AddressablesSystem<Material>.GetOrLoadAddressable(poweredMaterialName);
                var loadPreviewCostMaterialTask =
                    AddressablesSystem<Material>.GetOrLoadAddressable(costPreviewMaterialName);
                var loadPreviewGainMaterialTask =
                    AddressablesSystem<Material>.GetOrLoadAddressable(gainPreviewMaterialName);

                //Results
                await loadOfflineMaterialTask;
                Assert.NotNull(loadOfflineMaterialTask.Result);
                await loadPoweredMaterialTask;
                Assert.NotNull(loadPoweredMaterialTask.Result);
                await loadPreviewCostMaterialTask;
                Assert.NotNull(loadPreviewCostMaterialTask.Result);
                await loadPreviewGainMaterialTask;
                Assert.NotNull(loadPreviewGainMaterialTask.Result);

                //Set in data
                _materialMap.Add(EResourcePipeState.Offline, loadOfflineMaterialTask.Result);
                _materialMap.Add(EResourcePipeState.Powered, loadPoweredMaterialTask.Result);
                _materialMap.Add(EResourcePipeState.PreviewingCost, loadPreviewCostMaterialTask.Result);
                _materialMap.Add(EResourcePipeState.PreviewingGain, loadPreviewGainMaterialTask.Result);

                //Default material
                SetMaterial(loadOfflineMaterialTask.Result);
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to load addressables due to {e.Message}");
                throw;
            }
        }

        protected void SetMaterial(Material material)
        {
            Assert.IsNotNull(_meshRenderer);
            _meshRenderer.material = material;
        }
    }
}