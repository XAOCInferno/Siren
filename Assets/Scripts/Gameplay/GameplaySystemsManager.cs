using System;
using Gameplay;
using UnityEngine;

public class GameplaySystemsManager : MonoBehaviour
{
    private void Awake()
    {
        ResourceSystem.Init();
    }
}
