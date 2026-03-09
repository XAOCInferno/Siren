using System;
using Global;
using JetBrains.Annotations;
using UnityEngine;

namespace Gameplay
{
    public class HandData
    {
        
    }
    
    public class Hand : MonoBehaviour
    {
        [SerializeField] protected bool isLocallyControlled;
        protected HandData handData = new();

        protected void Awake()
        {
            HandEvents.OnAddCardToHand += OnAddCardToHand;
        }
        
        protected void OnAddCardToHand([CanBeNull] object sentFrom, HandEvents.AddCardToHandPayload payload)
        {
            if (isLocallyControlled == payload.Player.playerData.isLocallyControlled)
            {
                payload.Card.SetState(ECardState.InHand);
            }
        }
    }
}
