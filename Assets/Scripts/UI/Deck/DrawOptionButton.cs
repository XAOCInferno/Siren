using Debug;
using Gameplay;
using Global;
using Player;
using UnityEngine;

namespace UI.Deck
{
    public class DrawOptionButton : SimpleButton
    {
        [SerializeField] protected Gameplay.ECardType cardTypeToDraw;

        public override void OnButtonPressed()
        {
            Player.Player localPlayer = PlayerSystem.GetLocalPlayer();
            if (!localPlayer)
            {
                DebugSystem.Error("Cannot draw card as local player is null.");
                return;
            }

            DeckEvents.InvokeOnDrawCard(this,
                new DeckEvents.DrawCardPayload(player: localPlayer, EDrawFromDeckOption.DrawNext, cardToDrawType: cardTypeToDraw));
        }
    }
}