using System.Collections.Generic;
using System.Linq;
using Debug;
using Gameplay.Card;
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
        //TODO: Don't use ref to card, we should ref the object or an ID perhaps to fetch it
        public readonly List<CardLogic> cards = new List<CardLogic>();

        [CanBeNull]
        public CardLogic GetNextCardOfTypeInDeck(ECardType type)
        {
            foreach (CardLogic card in cards)
            {
                if (card.GetCardData().GetCardType() == type && card.GetComponent<CardState>().GetLogicStateMachine().GetState() == ECardLogicState.InDeck)
                {
                    return card;
                }
            }

            return null;
        }

        public CardLogic GetRandomCardOfTypeInDeck(ECardType type)
        {
            List<CardLogic> cardsOfType = cards.Where(card =>
                card.GetCardData().GetCardType() == type && card.GetComponent<CardState>().GetLogicStateMachine().GetState() == ECardLogicState.InDeck).ToList();
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
            if (payload.PoolType == typeof(CardLogic))
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
            if (Database<CardData>.GetIsDatabaseSet() && PoolSystem<CardLogic>.GetIsPoolReady())
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
                PooledObject nextPooledObject = PoolSystem<CardLogic>.GetPool().GetNextAvailable();
                if (nextPooledObject == null)
                {
                    DebugSystem.Error("Cannot get next card from pool, consider resizing the pool!");
                    return;
                }

                //Get data
                CardLogic nextCardLogic = nextPooledObject.GetComponent<CardLogic>();
                nextCardLogic.SetCardData(cardDatas[Random.Range(0, cardDatas.Length)]);
                nextCardLogic.GetComponent<CardState>().GetLogicStateMachine().SetState(ECardLogicState.NotInPlay);
                //Add to deck
                AddCardToDeck(nextCardLogic, EAddToDeckOption.AddToRandomPosition);
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
            CardLogic cardLogicToDraw = null;
            switch (payload.DrawOption)
            {
                //Get next card, closest to index 0
                case EDrawFromDeckOption.DrawNext:
                    DebugSystem.Log($"Drawing next...");
                    //Get the card to draw
                    if (payload.CardToDrawType != null)
                    {
                        cardLogicToDraw = deckData.GetNextCardOfTypeInDeck(payload.CardToDrawType.Value);
                    }
                    else if (deckData.cards.Count > 0)
                    {
                        cardLogicToDraw = deckData.cards[0];
                    }

                    break;
                //Get random card
                case EDrawFromDeckOption.DrawRandom:
                    DebugSystem.Log($"Drawing random...");
                    //Get the card to draw
                    if (payload.CardToDrawType != null)
                    {
                        cardLogicToDraw = deckData.GetRandomCardOfTypeInDeck(payload.CardToDrawType.Value);
                    }
                    else if (deckData.cards.Count > 0)
                    {
                        cardLogicToDraw = deckData.cards[Random.Range(0, deckData.cards.Count)];
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
                        CardLogic possibleCardLogicToDraw = deckData.cards[payload.SpecificCardToDrawIndex.Value];
                        if (possibleCardLogicToDraw.GetCardData().GetCardType() == payload.CardToDrawType)
                        {
                            cardLogicToDraw = possibleCardLogicToDraw;
                        }
                    }

                    break;
            }

            //Draw if we have a card to draw, otherwise log error
            if (!cardLogicToDraw)
            {
                DebugSystem.Warn(
                    $"Cannot draw card from deck, as no cards of type {payload.CardToDrawType} are in deck!");
                return;
            }

            //Success, log, remove from deck, then add to hand
            DebugSystem.Log($"Successfully drawn card {cardLogicToDraw} from deck, adding to hand.");
            RemoveCardFromDeck(cardLogicToDraw);
            HandEvents.InvokeOnAddCardToHand(this,
                new HandEvents.AddCardToHandPayload(payload.Player, cardLogicToDraw, drawCardMkr.position));
        }

        public void AddCardToDeck(CardLogic cardLogic, EAddToDeckOption addOption, int? optionalSpecificPosition = 0)
        {
            //Place it in location of deck
            switch (addOption)
            {
                case EAddToDeckOption.AddToTop:
                    deckData.cards.Insert(0, cardLogic);
                    break;
                case EAddToDeckOption.AddToBottom:
                    deckData.cards.Add(cardLogic);
                    break;
                case EAddToDeckOption.AddToRandomPosition:
                    deckData.cards.Insert(Random.Range(0, deckData.cards.Count), cardLogic);
                    break;
                case EAddToDeckOption.AddToSpecificPosition:
                    if (optionalSpecificPosition == null)
                    {
                        DebugSystem.Error("Cannot add card to specific location, as specific location is undefined!");
                        return;
                    }

                    deckData.cards.Insert(optionalSpecificPosition.Value, cardLogic);
                    break;
            }

            //State
            cardLogic.GetComponent<CardState>().GetLogicStateMachine().SetState(ECardLogicState.InDeck);
        }

        public void RemoveCardFromDeck(CardLogic cardLogicToRemove)
        {
            //Check card is actually in our deck
            if (!deckData.cards.Contains(cardLogicToRemove))
            {
                DebugSystem.Error($"Cannot remove card {cardLogicToRemove} from deck, as it is not in deck!");
                return;
            }

            //It is, so remove it
            deckData.cards.RemoveAt(deckData.cards.IndexOf(cardLogicToRemove));
        }
    }
}