using Debug;
using Global;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private const GameObject LocalPlayerPrefab = null;
        [SerializeField] private const GameObject AIPlayerPrefab = null;

        //TODO: What is a reasonable size for the pool?
        private const int DeckPoolSize = 512;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            //Local player
            new GameObject("LocalPlayer").AddComponent<Player.Player>().Init(isLocallyControlled: true);
            //AI Player
            new GameObject("AI Player").AddComponent<Player.Player>().Init(isLocallyControlled: false);
            
            //Load addressable then instantiate it
            var loadHandle = Addressables.LoadAssetAsync<GameObject>(
                "Card.prefab");
            loadHandle.Completed += h =>
            {
                //Log
                DebugSystem.Log("Successfully loaded card template, setting up pool.");
                //Set template and then pool size (which will instantiate the template)
                PoolSystem<Card>.SetTemplateToInstantiate(h.Result);
                PoolSystem<Card>.SetPoolSize(DeckPoolSize);
                //Communicate it's ready
                PoolEvents.InvokeOnPoolSetup(this, new PoolEvents.PoolSetupPayload(typeof(Card)));
            };
        }
    }
}