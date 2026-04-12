using System;
using Global;
using UnityEngine;

namespace Gameplay.Resources
{
    public class ResourcePipeLogic : MonoBehaviour
    {
        private ResourcePipeObject _resourcePipeObject;

        private void Awake()
        {
            _resourcePipeObject = GetComponent<ResourcePipeObject>();
            ResourceEvents.OnResourcePreviewChanged += OnResourcePreviewChanged;
        }

        protected void OnResourcePreviewChanged(object sender, ResourceEvents.ResourcePreviewChanged payload)
        {
            // State
            ResourcePipeState state = _resourcePipeObject.GetState();

            // Check this is affecting the player who we're previewing and the resource we represent
            if (payload.isLocalPlayer != state.GetRepresentsLocalPlayer() ||
                payload.resourceType != state.GetRepresentedResourceType()) return;

            // Get Gained and lost
            int totalGained;
            int totalCost = payload.costsPreviewed - payload.resourcesGained;
            if (totalCost < 0)
            {
                totalGained = Math.Abs(totalCost);
                totalCost = 0;
            }
            else
            {
                totalGained = 0;
            }

            //Get thresholds
            int poweredUpTotal = payload.resourcesRemaining - totalCost;
            int poweredUpAfterCost = poweredUpTotal + totalCost;
            int poweredUpTotalAfterGained = poweredUpAfterCost + totalGained;

            // Calculate new state
            EResourcePipeState newState;
            int representsResource = state.GetRepresentedResourceValue();
            if (representsResource > poweredUpTotalAfterGained)
            {
                // Has enough resources but is previewing cost
                newState = EResourcePipeState.Offline;
            }
            else if (representsResource > poweredUpAfterCost)
            {
                // Has enough resources and isn't previewing cost
                newState = EResourcePipeState.PreviewingGain;
            }
            else if (representsResource > poweredUpTotal)
            {
                // Has enough resources and isn't previewing cost
                newState = EResourcePipeState.PreviewingCost;
            }
            else
            {
                // Has enough resources but is previewing cost
                newState = EResourcePipeState.Powered;
            }

            // Set state
            state.GetStateMachine().SetState(newState);
        }
    }
}