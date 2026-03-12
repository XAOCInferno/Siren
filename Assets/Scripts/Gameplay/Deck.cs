using System.Collections.Generic;
using System.Linq;
using Debug;
using Global;
using JetBrains.Annotations;
using UnityEngine;
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
        public readonly List<Card> cards = new List<Card>();

        [CanBeNull]
        public Card GetNextCardOfTypeInDeck(ECardType type)
        {
            foreach (Card card in cards)
            {
                if (card.GetCardData().GetCardType() == type && card.GetState() == ECardState.InDeck)
                {
                    return card;
                }
            }

            return null;
        }

        public Card GetRandomCardOfTypeInDeck(ECardType type)
        {
            List<Card> cardsOfType = cards.Where(card =>
                card.GetCardData().GetCardType() == type && card.GetState() == ECardState.InDeck).ToList();
            return cardsOfType.Count > 0 ? cardsOfType[Random.Range(0, cardsOfType.Count)] : null;
        }
    }

    public class Deck : MonoBehaviour
    {
        [SerializeField] protected Transform drawCardMkr;
        [SerializeField] protected bool isLocallyControlled;
        protected Player.Player ownerPlayer;
        protected readonly DeckData deckData = new DeckData();

        protected void Awake()
        {
            PlayerEvents.OnPlayerRegistered += OnPlayerRegistered;
            DeckEvents.OnDrawCard += DrawCardFromDeck;
            PoolEvents.OnPoolSetup += OnPoolSetup;
            DatabaseEvents.OnDatabaseSet += OnDatabaseSet;
        }

        protected void OnPlayerRegistered([CanBeNull] object sentFrom, PlayerEvents.PlayerRegisteredPayload payload)
        {
            if (isLocallyControlled == payload.RegisteredPlayer.playerData.isLocallyControlled)
            {
                ownerPlayer = payload.RegisteredPlayer;
            }
        }

        protected void OnPoolSetup([CanBeNull] object sentFrom, PoolEvents.PoolSetupPayload payload)
        {
            if (payload.PoolType == typeof(Card))
            {
                TryLoadDeck();
            }
        }

        protected void OnDatabaseSet([CanBeNull] object sentFrom, DatabaseEvents.DatabaseSetupPayload payload)
        {
            if (payload.DatabaseType == typeof(CardData))
            {
                TryLoadDeck();
            }
        }

        protected void TryLoadDeck()
        {
            if (Database<CardData>.GetIsDatabaseSet() && PoolSystem<Card>.GetIsPoolReady())
            {
                //TODO: Remove debug
                DebugFillDeckRandomly();
            }
        }

        protected void DebugFillDeckRandomly()
        {
            //Get data, ensure we have cards
            CardData[] cardDatas = Database<CardData>.GetItems();
            if (cardDatas == null || cardDatas.Length == 0)
            {
                DebugSystem.Error("Cannot randomly fill deck as no cards are defined in the database!");
                return;
            }

            //Load our desired number of cards
            const int numberOfCardsInDeck = 30;
            for (int i = 0; i < numberOfCardsInDeck; i++)
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
                nextCard.SetCardData(cardDatas[Random.Range(0, cardDatas.Length)]);
                nextCard.SetState(ECardState.NotInPlay);
                //Add to deck
                AddCardToDeck(nextCard, EAddToDeckOption.AddToRandomPosition);
            }
        }


        public void DrawCardFromDeck([CanBeNull] object sentFrom, DeckEvents.DrawCardPayload payload)
        {
            //Check if we are assigned to the player who is drawing
            if (!ownerPlayer || payload.Player != ownerPlayer)
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
                        cardToDraw = deckData.GetNextCardOfTypeInDeck(payload.CardToDrawType.Value);
                    }
                    else if (deckData.cards.Count > 0)
                    {
                        cardToDraw = deckData.cards[0];
                    }

                    break;
                //Get random card
                case EDrawFromDeckOption.DrawRandom:
                    DebugSystem.Log($"Drawing random...");
                    //Get the card to draw
                    if (payload.CardToDrawType != null)
                    {
                        cardToDraw = deckData.GetRandomCardOfTypeInDeck(payload.CardToDrawType.Value);
                    }
                    else if (deckData.cards.Count > 0)
                    {
                        cardToDraw = deckData.cards[Random.Range(0, deckData.cards.Count)];
                    }

                    break;
                //Get specific card defined in payload
                case EDrawFromDeckOption.DrawSpecific:
                    DebugSystem.Log($"Drawing specific...");
                    if (payload.SpecificCardToDrawIndex == null)
                    {
                        DebugSystem.Error(
                            "Attempting to draw a specific card, however, index of card is null. So will do nothing.");
                    }
                    else if (deckData.cards.Count > payload.SpecificCardToDrawIndex.Value)
                    {
                        //Try get card of that index
                        //Check card is correct type
                        Card possibleCardToDraw = deckData.cards[payload.SpecificCardToDrawIndex.Value];
                        if (possibleCardToDraw.GetCardData().GetCardType() == payload.CardToDrawType)
                        {
                            cardToDraw = possibleCardToDraw;
                        }
                    }

                    break;
            }

            //Draw if we have a card to draw, otherwise log error
            if (!cardToDraw)
            {
                DebugSystem.Warn(
                    $"Cannot draw card from deck, as no cards of type {payload.CardToDrawType} are in deck!");
                return;
            }

            //Success, log, remove from deck, then add to hand
            DebugSystem.Log($"Successfully drawn card {cardToDraw} from deck, adding to hand.");
            RemoveCardFromDeck(cardToDraw);
            HandEvents.InvokeOnAddCardToHand(this,
                new HandEvents.AddCardToHandPayload(payload.Player, cardToDraw, drawCardMkr.position));
        }

        public void AddCardToDeck(Card card, EAddToDeckOption addOption, int? optionalSpecificPosition = 0)
        {
            //Place it in location of deck
            switch (addOption)
            {
                case EAddToDeckOption.AddToTop:
                    deckData.cards.Insert(0, card);
                    break;
                case EAddToDeckOption.AddToBottom:
                    deckData.cards.Add(card);
                    break;
                case EAddToDeckOption.AddToRandomPosition:
                    deckData.cards.Insert(Random.Range(0, deckData.cards.Count), card);
                    break;
                case EAddToDeckOption.AddToSpecificPosition:
                    if (optionalSpecificPosition == null)
                    {
                        DebugSystem.Error("Cannot add card to specific location, as specific location is undefined!");
                        return;
                    }

                    deckData.cards.Insert(optionalSpecificPosition.Value, card);
                    break;
            }

            //State
            card.SetState(ECardState.InDeck);
        }

        public void RemoveCardFromDeck(Card cardToRemove)
        {
            //Check card is actually in our deck
            if (!deckData.cards.Contains(cardToRemove))
            {
                DebugSystem.Error($"Cannot remove card {cardToRemove} from deck, as it is not in deck!");
                return;
            }

            //It is, so remove it
            deckData.cards.RemoveAt(deckData.cards.IndexOf(cardToRemove));
        }
    }
}