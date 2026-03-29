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
    public class GameplayManagerSingleton : MonoBehaviour
    {
        //TODO: What is a reasonable size for these pools?
        private const int DeckPoolSize = 512;
        private const int PiecePoolSize = 256;

        public static GameplayManagerSingleton instance { get; private set; }

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
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to start gameplay manager due to {e}");
                throw;
            }
        }

        protected static void OnTileSelected(object sender, TileEvents.OnTileSelectedPayload payload)
        {
            if (GameplaySystem.HasLocalCardBeingPlayed())
            {
                PlayCardToTile(payload.tileObject, true);
            }
        }

        protected static ActionResult PlayCardToTile(TileObject tileObject, bool playedByLocalPlayer)
        {
            //Ensure this is a valid play
            if (tileObject.GetState().GetIsOccupiedByPiece())
                return new ActionResult(false, "Cannot play card to tile as destination tile is occupied");

            return GameplaySystem.PlayCard(tileObject.GetState().GetGridLocation(),
                playedByLocalPlayer ? PlayerSystem.GetLocalPlayer() : PlayerSystem.GetAIPlayer());
        }
    }
}