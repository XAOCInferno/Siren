using Gameplay.Resources;
using UnityEngine;

namespace Gameplay
{
    public class GameplaySystemsManager : MonoBehaviour
    {
        private void Awake()
        {
            ResourceSystem.Init();
        }
    }
}