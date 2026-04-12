using System;
using System.Collections.Generic;
using Debug;
using Gameplay.Card;
using Global;
using JetBrains.Annotations;
using Player;
using Utils;

namespace Gameplay.Resources
{
    //All types of resources players can spend
    public enum EResourceType
    {
        Energy = 0,
    }

    public class ResourceValue
    {
        private int _value = 0;

        public ResourceValue(int value = 0)
        {
            this._value = value;
        }

        //Value
        public int SetValue(int value) => this._value = value;

        public int GetValue()
        {
            return _value;
        }

        //Status
        public bool HasEnough(int required) => _value >= required;
        public void DecreaseValue(int amount) => _value -= amount;
        public void IncreaseValue(int amount) => _value += amount;
    }

    public class PlayerResources
    {
        private readonly Dictionary<EResourceType, ResourceValue> _resources = new();

        public PlayerResources()
        {
            //Set all possible resources to ensure we can get those later
            foreach (EResourceType resource in Enum.GetValues(typeof(EResourceType)))
            {
                _resources.Add(resource, new ResourceValue());
            }
        }

        //Get resource, will never be null due to constructor
        public ResourceValue GetResourceOfType(EResourceType type)
        {
            return _resources[type];
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
        private static void OnPlayerRegistered([CanBeNull] object sender, PlayerEvents.PlayerRegisteredPayload payload)
        {
            if (PlayerResources.ContainsKey(payload.RegisteredPlayer))
            {
                DebugSystem.Warn(
                    $"Attempting to register player with ID {payload.RegisteredPlayer.playerData.playerID} which is already registered, this will be ignored");
                return;
            }

            DebugSystem.Log($"Creating registered player's resources");
            PlayerResources.Add(payload.RegisteredPlayer, new PlayerResources());
        }

        /// <summary>
        /// Gets player resources. Do not expose this as we don't want external modules editing player resources directly. Any edit should be done through this system so we can broadcast the result.
        /// </summary>
        /// <param name="player">The player resources we want to get.</param>
        /// <returns>The player's resources.</returns>
        private static PlayerResources GetPlayerResources(Player.Player player) => PlayerResources[player];

        /// <summary>
        /// Gets amount of resources a player has.
        /// </summary>
        /// <param name="player">The player to get from.</param>
        /// <param name="resourceType">The resource type we wish to query.</param>
        /// <returns>Number value of the amount we have.</returns>
        public static int GetPlayerResourceAmount(Player.Player player, EResourceType resourceType) =>
            PlayerResources[player].GetResourceOfType(resourceType).GetValue();

        public static Util.ActionResult PlayerHasEnoughResources(Player.Player player, ResourceChange[] costs)
        {
            // Check we have enough resources to play the card
            PlayerResources resources = GetPlayerResources(player);
            for (int i = 0; i < costs.Length; i++)
            {
                // Check if we have enough
                if (!resources.GetResourceOfType(costs[i].resourceType).HasEnough(costs[i].decrease))
                {
                    return new Util.ActionResult(false,
                        $"Not enough resources of type {costs[i].resourceType} to play card.");
                }
            }

            // Success
            return new Util.ActionResult(true);
        }

        /// <summary>
        /// Decrease player's multiple player resources. Ensure you check you have enough resources before doing this as we will not check in here.
        /// </summary>
        /// <param name="player">The player to decrease resources from.</param>
        /// <param name="changeBy">How much to decrease the resources by.</param>
        public static void PlayerChangeResources(Player.Player player, ResourceChange[] changeBy)
        {
            PlayerResources resources = GetPlayerResources(player);
            for (int i = 0; i < changeBy.Length; i++)
            {
                // Get resource type
                EResourceType resourceType = changeBy[i].resourceType;

                // Change resource
                resources.GetResourceOfType(resourceType).IncreaseValue(changeBy[i].increase);
                resources.GetResourceOfType(resourceType).DecreaseValue(changeBy[i].decrease);

                // Broadcast
                OnPlayerResourceChanged(player, resourceType);
            }
        }

        /// <summary>
        /// Called on resources changing. Broadcasts the result to other systems.
        /// </summary>
        /// <param name="player">Player who's resource has changed.</param>
        /// <param name="resourceType">The type of resource that has changed.</param>
        private static void OnPlayerResourceChanged(Player.Player player, EResourceType resourceType)
        {
            // Get
            PlayerResources resources = GetPlayerResources(player);

            // Broadcast
            ResourceEvents.InvokeOnResourcePreviewChanged(player,
                new ResourceEvents.ResourcePreviewChanged(resourceType, 0,
                    resources.GetResourceOfType(resourceType).GetValue(), 0,
                    PlayerSystem.IsLocalPlayer(player)));
        }
    }
}