using System;
using System.Collections.Generic;
using Debug;
using Global;
using JetBrains.Annotations;

namespace Gameplay
{
    //All types of resources players can spend
    public enum EResourceType
    {
        Energy = 0,
    }

    public class Resource
    {
        public EResourceType type { get; protected set; }
        public int value { get; protected set; }

        public Resource(EResourceType type, int value = 0)
        {
            this.type = type;
            this.value = value;
        }

        //Value
        public int SetValue(int value) => this.value = value;

        public int GetValue()
        {
            return value;
        }

        //Status
        public bool HasEnough(int required) => value >= required;
        public void DecreaseValue(int amount) => value -= amount;
        public void IncreaseValue(int amount) => value += amount;
    }

    public class PlayerResources
    {
        private readonly List<Resource> _resources = new();

        public PlayerResources()
        {
            //Set all possible resources to ensure we can get those later
            foreach (EResourceType resource in Enum.GetValues(typeof(EResourceType)))
            {
                _resources.Add(new Resource(resource));
            }
        }

        //Get resource, will never be null due to constructor
        public Resource GetResourceOfType(EResourceType type)
        {
            return _resources.Find((v) => v.type == type);
        }
    }

    public static class ResourceSystem
    {
        private static readonly Dictionary<Player.Player, PlayerResources> PlayerResources = new();

        public static void Init()
        {
            //Subscriptions
            PlayerEvents.OnPlayerRegistered += OnPlayerRegistered;
        }

        //Listen to a player being registered and set up their initial resources
        private static void OnPlayerRegistered([CanBeNull] object sender, PlayerRegisteredPayload payload)
        {
            if (PlayerResources.ContainsKey(payload.player))
            {
                Debug.DebugSystem.Warn(
                    $"Attempting to register player with ID {payload.player.playerData.playerID} which is already registered, this will be ignored");
                return;
            }

            DebugSystem.Log($"Creating registered player's resources");
            PlayerResources.Add(payload.player, new PlayerResources());
        }
    }
}