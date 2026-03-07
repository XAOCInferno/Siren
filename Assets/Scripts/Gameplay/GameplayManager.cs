using Debug;
using Global;
using UnityEditor.MPE;
using UnityEngine;

namespace Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private const GameObject LocalPlayerPrefab = null;
        [SerializeField] private const GameObject AIPlayerPrefab = null;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            //Local player
            Instantiate(new GameObject()).AddComponent<Player.Player>().Init(isLocallyControlled: true);
            //AI Player
            Instantiate(new GameObject()).AddComponent<Player.Player>().Init(isLocallyControlled: false);
        }
    }
}