using System;
using System.Threading.Tasks;
using Debug;
using Gameplay.Card;
using Gameplay.Piece;
using Global;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        //TODO: What is a reasonable size for these pools?
        private const int DeckPoolSize = 512;
        private const int PiecePoolSize = 256;

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
    }
}