using System;
using System.Threading.Tasks;
using Debug;
using Gameplay.Card;
using Gameplay.Piece;
using Gameplay.Tile;
using Global;
using Player;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Gameplay
{
    [Serializable]
    public struct ValidGameplayTransitions
    {
        public EGameplayPhaseState fromState;
        public EGameplayPhaseState[] validToStates;
    }

    public class GameplayManagerSingleton : MonoBehaviour
    {
        //TODO: What is a reasonable size for these pools?
        private const int DeckPoolSize = 512;
        private const int PiecePoolSize = 256;

        public static GameplayManagerSingleton instance { get; private set; }

        [SerializeField]
        private ValidGameplayTransitions[] validGameplayTransitions = Array.Empty<ValidGameplayTransitions>();

        private void Awake()
        {
            //Ensure singleton is unique
            if (instance != null && instance != this)
            {
                DebugSystem.Warn("Multiple GameplayManagerSingleton in scene, this is invalid!");
                Destroy(gameObject);
                return;
            }

            instance = this;

            //Bind events
            TileEvents.OnTileSelected += OnTileSelected;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private async void Start()
        {
            try
            {
                //Local player
                new GameObject("LocalPlayer").AddComponent<Player.Player>().Init(isLocallyControlled: true);
                //AI Player
                new GameObject("AI Player").AddComponent<Player.Player>().Init(isLocallyControlled: false);

                //Load addressable then instantiate it
                var loadHandleCard = Addressables.LoadAssetAsync<GameObject>(
                    "Card.prefab");
                loadHandleCard.Completed += h =>
                {
                    //Log
                    DebugSystem.Log("Successfully loaded card template, setting up pool.");
                    //Set template and then pool size (which will instantiate the template)
                    PoolSystem<CardLogic>.SetTemplateToInstantiate(h.Result);
                    PoolSystem<CardLogic>.SetPoolSize(DeckPoolSize);
                    //Communicate it's ready
                    PoolEvents.InvokeOnPoolSetup(this, new PoolEvents.PoolSetupPayload(typeof(CardLogic)));
                };

                //Acynch load piece then act on it
                var loadHandlePiece = Addressables.LoadAssetAsync<GameObject>(
                    "Piece.prefab");

                //Wait until loaded
                while (!loadHandlePiece.IsDone)
                {
                    await Task.Yield();
                }

                //Log
                DebugSystem.Log("Successfully loaded piece template, setting up pool.");
                //Set template and then pool size (which will instantiate the template)
                PoolSystem<PieceLogic>.SetTemplateToInstantiate(loadHandlePiece.Result);
                PoolSystem<PieceLogic>.SetPoolSize(PiecePoolSize);

                //Init all Pieces
                foreach (var items in PoolSystem<PieceLogic>.GetPool().GetAll())
                {
                    await items.GetComponent<PieceObject>().Init();
                }

                //Communicate it's ready
                PoolEvents.InvokeOnPoolSetup(this, new PoolEvents.PoolSetupPayload(typeof(PieceObject)));
                
                // Set initial gameplay phase state (TODO: We probably want to move this into some kind of TurnManager? Which could be a part of gameplay system)
                GameplayEvents.InvokeOnGameplayPhaseStateChanged(this, new GameplayEvents.GameplayPhaseStateChangedPayload(EGameplayPhaseState.CardPhase, EGameplayPhaseState.CardPhase));
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to start gameplay manager due to {e}");
                throw;
            }
        }

        protected static void OnTileSelected(object sender, TileEvents.OnTileSelectedPayload payload)
        {
            if (GameplaySystem.HasCardBeingPlayed())
            {
                PlayCardToTile(payload.tileObject, true);
            }
            else if (GameplaySystem.HasPieceBeingControlled())
            {
                HandlePieceToTileAction(GameplaySystem.GetPieceBeingControlled(), payload.tileObject);
            }
        }

        protected static Util.ActionResult PlayCardToTile(TileObject tileObject, bool playedByLocalPlayer)
        {
            //Ensure this is a valid play
            if (tileObject.GetState().GetIsOccupiedByPiece())
                return new Util.ActionResult(false, "Cannot play card to tile as destination tile is occupied");

            return GameplaySystem.PlayCard(BoardSystem<TileObject>.GetItemLocationOnGrid(tileObject),
                playedByLocalPlayer ? PlayerSystem.GetLocalPlayer() : PlayerSystem.GetAIPlayer());
        }

        protected static Util.ActionResult HandlePieceToTileAction(PieceObject pieceObject, TileObject tileObject)
        {
            //Ensure this is a valid play
            if (tileObject.GetState().GetIsOccupiedByPiece())
                return new Util.ActionResult(false, "Cannot handle piece movement as tile is occupied");

            return GameplaySystem.MovePiece(pieceObject, BoardSystem<TileObject>.GetItemLocationOnGrid(tileObject));
        }

        public ValidGameplayTransitions GetValidGameplayTransitions(EGameplayPhaseState fromState)
        {
            // Find next valid transitions
            ValidGameplayTransitions validTransitions = new ValidGameplayTransitions();
            for (int i = 0; i < validGameplayTransitions.Length; i++)
            {
                if (validGameplayTransitions[i].fromState != fromState) continue;
                validTransitions = validGameplayTransitions[i];
                break;
            }

            return validTransitions;
        }
    }
}