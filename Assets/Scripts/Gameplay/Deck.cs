using System;
using System.Collections.Generic;
using System.Linq;
using Debug;
using Global;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public enum EDrawFromDeckOption
    {
        DrawNext,
        DrawRandom,
        DrawSpecific
    }
    public enum EAddToDeckOption
    {
        AddToTop,
        AddToBottom,
        AddToRandomPosition,
        AddToSpecificPosition,
    }
    public class DeckData
    {
        public Player.Player OwnerPlayer;
        private readonly List<Card> _cards = new List<Card>();
        
        public void DrawCardFromDeck([CanBeNull] object sentFrom, DeckEvents.DrawCardPayload payload)
        {
            //Check if we are assigned to the player who is drawing
            if (!OwnerPlayer || payload.Player != OwnerPlayer)
            {
                return;
            }
            //We are, so draw
            DebugSystem.Log($"{payload.Player.playerData.playerID} is attempting to draw a card");
            Card cardToDraw = null;
            switch (payload.DrawOption)
            { 
                //Get next card, closest to index 0
                case EDrawFromDeckOption.DrawNext:
                    DebugSystem.Log($"Drawing next...");
                    //Get the card to draw
                    if (payload.CardToDrawType != null)
                    {
                        cardToDraw = GetNextCardOfTypeInDeck(payload.CardToDrawType.Value);
                    }
                    else if(_cards.Count > 0)
                    {
                        cardToDraw = _cards[0];
                    }
                    break;
                //Get random card
                case EDrawFromDeckOption.DrawRandom:
                    DebugSystem.Log($"Drawing random...");
                    //Get the card to draw
                    if (payload.CardToDrawType != null)
                    {
                        cardToDraw = GetRandomCardOfTypeInDeck(payload.CardToDrawType.Value);
                    }
                    else if(_cards.Count > 0)
                    {
                        cardToDraw = _cards[Random.Range(0, _cards.Count)];
                    }
                    break;
                //Get specific card defined in payload
                case EDrawFromDeckOption.DrawSpecific:
                    DebugSystem.Log($"Drawing specific...");
                    if (payload.SpecificCardToDrawIndex == null)
                    {
                        DebugSystem.Error("Attempting to draw a specific card, however, index of card is null. So will do nothing.");
                    }
                    else if (_cards.Count > payload.SpecificCardToDrawIndex.Value)
                    {
                        //Try get card of that index
                        //Check card is correct type
                        Card possibleCardToDraw = _cards[payload.SpecificCardToDrawIndex.Value];
                        if (possibleCardToDraw.GetCardData().Type == payload.CardToDrawType)
                        {
                            cardToDraw = possibleCardToDraw;
                        }
                    }
                    break;
            }
            //Draw if we have a card to draw, otherwise log error
            if (!cardToDraw)
            {
                DebugSystem.Warn($"Cannot draw card from deck, as no cards of type {payload.CardToDrawType} are in deck!");
                return;
            }
            //Success, log, remove from deck, then add to hand
            DebugSystem.Log($"Successfully drawn card {cardToDraw} from deck, adding to hand.");
            RemoveCardFromDeck(cardToDraw);
            HandEvents.InvokeOnAddCardToHand(this, new HandEvents.AddCardToHandPayload(payload.Player, cardToDraw));
        }
        public void AddCardToDeck(Card card, EAddToDeckOption addOption, int? optionalSpecificPosition = 0)
        {
            //Place it in location of deck
            switch (addOption)
            {
                case EAddToDeckOption.AddToTop:
                    _cards.Insert(0, card);
                    break;
                case EAddToDeckOption.AddToBottom:
                    _cards.Add(card);
                    break;
                case EAddToDeckOption.AddToRandomPosition:
                    _cards.Insert(Random.Range(0, _cards.Count), card);
                    break;
                case EAddToDeckOption.AddToSpecificPosition:
                    if (optionalSpecificPosition == null)
                    {
                        DebugSystem.Error("Cannot add card to specific location, as specific location is undefined!");
                        return;
                    }
                    _cards.Insert(optionalSpecificPosition.Value, card);
                    break;
            }
            //State
            card.SetState(ECardState.InDeck);
        }
        public void RemoveCardFromDeck(Card cardToRemove)
        {
            //Check card is actually in our deck
            if (!_cards.Contains(cardToRemove))
            {
                DebugSystem.Error($"Cannot remove card {cardToRemove} from deck, as it is not in deck!");
                return;
            }
            //It is, so remove it
            _cards.RemoveAt(_cards.IndexOf(cardToRemove));
        }

        [CanBeNull]
        public Card GetNextCardOfTypeInDeck(ECardType type)
        {
            foreach (Card card in _cards)
            {
                if (card.GetCardData().Type == type && card.GetCardData().State == ECardState.InDeck)
                {
                    return card;
                }
            }

            return null;
        }
        
        public Card GetRandomCardOfTypeInDeck(ECardType type)
        {
            List<Card> cardsOfType = _cards.Where(card => card.GetCardData().Type == type && card.GetCardData().State == ECardState.InDeck).ToList();
            return cardsOfType.Count > 0 ? cardsOfType[Random.Range(0, cardsOfType.Count)] : null;
        }
    }

    public class Deck : MonoBehaviour
    {
        [SerializeField] protected bool isLocallyControlled;
        protected readonly DeckData DeckData = new DeckData();

        protected void Awake()
        {
            PlayerEvents.OnPlayerRegistered += OnPlayerRegistered;
            DeckEvents.OnDrawCard += DeckData.DrawCardFromDeck;
            PoolEvents.OnPoolSetup += OnPoolSetup;
        }
        
        protected void OnPlayerRegistered([CanBeNull] object sentFrom, PlayerEvents.PlayerRegisteredPayload payload)
        {
            if (isLocallyControlled == payload.RegisteredPlayer.playerData.isLocallyControlled)
            {
                DeckData.OwnerPlayer = payload.RegisteredPlayer;
            }
        }
        
        protected void OnPoolSetup([CanBeNull] object sentFrom, PoolEvents.PoolSetupPayload payload)
        {
            if (payload.PoolType == typeof(Card))
            {
                //TODO: Remove debug
                DebugFillDeckRandomly();
            }
        }

        protected void DebugFillDeckRandomly()
        {
            Array possibleTypes = Enum.GetValues(typeof(ECardType));
            for(int i = 0; i < 30; i ++)
            {
                //Get next card from pool
                PooledObject nextPooledObject = PoolSystem<Card>.GetPool().GetNextAvailable();
                if (nextPooledObject == null)
                {
                    DebugSystem.Error("Cannot get next card from pool, consider resizing the pool!");
                    return;
                }
                //Get data
                Card nextCard = nextPooledObject.GetComponent<Card>();
                nextCard.GetCardData().Type = possibleTypes.Cast<ECardType>().ElementAt(Random.Range(0, possibleTypes.Length));
                nextCard.GetCardData().State = ECardState.NotInPlay;
                //Add to deck
                DeckData.AddCardToDeck(nextCard, EAddToDeckOption.AddToRandomPosition);
            }
        }
    }
}