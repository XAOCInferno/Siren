using Debug;
using Gameplay;
using Gameplay.Card;
using Global;
using Player;
using UnityEngine;

namespace UI.Deck
{
    public class DrawOptionButton : SimpleButton
    {
        [SerializeField] protected ECardType cardTypeToDraw;

        protected override void Awake()
        {
            base.Awake();

            GameplayEvents.OnGameplayPhaseStateChanged += OnGameplayPhaseStateChanged;
        }

        public override void OnButtonPressed()
        {
            Player.Player localPlayer = PlayerSystem.GetLocalPlayer();
            if (!localPlayer)
            {
                DebugSystem.Error("Cannot draw card as local player is null.");
                return;
            }

            DeckEvents.InvokeOnDrawCard(this,
                new DeckEvents.DrawCardPayload(player: localPlayer, EDrawFromDeckOption.DrawNext,
                    cardToDrawType: cardTypeToDraw));
        }

        protected void OnGameplayPhaseStateChanged(object sender,
            GameplayEvents.GameplayPhaseStateChangedPayload payload)
        {
            button.interactable = payload.newGameplayPhaseState == EGameplayPhaseState.CardPhase;
        }
    }
}