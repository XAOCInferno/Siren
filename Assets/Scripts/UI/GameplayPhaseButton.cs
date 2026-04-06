using System.Linq;
using Gameplay;
using Global;
using UnityEngine;

namespace UI
{
    public class GameplayPhaseButton : SimpleButton
    {
        [SerializeField] private EGameplayPhaseState representedState;

        protected override void Awake()
        {
            base.Awake();

            GameplayEvents.OnGameplayPhaseStateChanged += OnGameplayPhaseStateChanged;
        }

        private void OnDestroy()
        {
            GameplayEvents.OnGameplayPhaseStateChanged -= OnGameplayPhaseStateChanged;
        }

        public override void OnButtonPressed()
        {
            GameplaySystem.SetGameplayPhaseState(representedState);
        }

        protected void OnGameplayPhaseStateChanged(object sender,
            GameplayEvents.GameplayPhaseStateChangedPayload payload)
        {
            if (payload.newGameplayPhaseState == representedState)
            {
                button.interactable = false;
                ChangeActiveColor(EUnityButtonStates.Disabled, Color.yellow);
            }
            else
            {
                ValidGameplayTransitions validTransitions =
                    GameplayManagerSingleton.instance.GetValidGameplayTransitions(payload.newGameplayPhaseState);

                ResetActiveColorToDefault(EUnityButtonStates.Disabled);

                button.interactable = validTransitions.validToStates != null &&
                                      validTransitions.validToStates.Contains(representedState);
            }
        }
    }
}