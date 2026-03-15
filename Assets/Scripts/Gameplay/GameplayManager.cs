using Debug;
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
        private void Start()
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

            var loadHandlePiece = Addressables.LoadAssetAsync<GameObject>(
                "Piece.prefab");
            loadHandlePiece.Completed += h =>
            {
                //Log
                DebugSystem.Log("Successfully loaded piece template, setting up pool.");
                //Set template and then pool size (which will instantiate the template)
                PoolSystem<Piece>.SetTemplateToInstantiate(h.Result);
                PoolSystem<Piece>.SetPoolSize(PiecePoolSize);
                //Communicate it's ready
                PoolEvents.InvokeOnPoolSetup(this, new PoolEvents.PoolSetupPayload(typeof(Piece)));
            };
        }
    }
}