using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Debug;
using Gameplay.Card;
using Gameplay.Piece;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Utils.StateMachine;

namespace Gameplay.Tile
{
    public class TileView : MonoBehaviour, IStatedItem<ETileViewState>
    {
        [SerializeField] protected float yMoveOnHover = 0.5f;
        [SerializeField] protected float moveTime = 0.2f;
        [SerializeField] private MeshRenderer[] meshRenderers;

        protected Vector3 startingPos = Vector3.zero;

        private readonly List<TileObject> _currentlyPreviewedTiles = new();

        protected TileObject tileObject;


        private readonly Dictionary<ETileViewState, Material> _materialMap = new();

        private void Awake()
        {
            //Get our parent object
            tileObject = gameObject.GetComponent<TileObject>();
            Assert.NotNull(tileObject);

            //Get Start pos
            startingPos = transform.position;

            //Now subscribe to our State Machine
            SubscribeToStateChangedEvent();
        }

        private void OnDestroy()
        {
            UnSubscribeFromStateChangedEvent();
        }

        //~IStatedItem
        public async Task Init()
        {
            try
            {
                //Addressables
                await LoadAddressables();
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to Init due to {e.Message}");
                throw;
            }
        }

        public void SubscribeToStateChangedEvent()
        {
            tileObject.GetState().GetViewStateMachine().SubscribeToStateChangedCallback(this);
        }

        public void UnSubscribeFromStateChangedEvent()
        {
            tileObject.GetState().GetViewStateMachine().UnsubscribeToStateChangedCallback(this);
        }

        public int OnStateChanged(EnumStateMachine<ETileViewState>.StateChangedEventPayload payload)
        {
            switch (payload.newState)
            {
                case ETileViewState.Idle:
                    //Hide preview if we were previously previewing piece
                    if (payload.oldState == ETileViewState.Hovered && !GameplaySystem.GetCardBeingPlayed())
                    {
                        PiecePreviewSingleton.instance.Hide();
                    }

                    //Hide any other tiles we're previewing
                    for (int i = 0; i < _currentlyPreviewedTiles.Count; i++)
                    {
                        _currentlyPreviewedTiles[i].GetState().GetViewStateMachine().SetState(ETileViewState.Idle);
                    }

                    _currentlyPreviewedTiles.Clear();

                    //Set idle state
                    MoveToIdlePosition();
                    SetIdleMaterial();
                    break;
                case ETileViewState.Hovered:
                    CardObject card = GameplaySystem.GetCardBeingPlayed();
                    if (card)
                    {
                        HandleCardPlayPreview(card);
                    }
                    else
                    {
                        SetIdleMaterial();
                        MoveToActivePosition();
                    }

                    break;
                case ETileViewState.PreviewAttack:
                    PiecePreviewSingleton.instance.Hide();
                    MoveToActivePosition();
                    SetPreviewAttackMaterial();
                    break;
                case ETileViewState.PreviewMove:
                    PiecePreviewSingleton.instance.Hide();
                    MoveToActivePosition();
                    SetPreviewMoveMaterial();
                    break;
                case ETileViewState.PreviewPlayCard:
                    MoveToActivePosition();
                    SetIdleMaterial();
                    break;
            }

            return 0;
        }

        //~IStatedItem End
        private async Task LoadAddressables()
        {
            try
            {
                //Load addressables
                var loadIdleMaterialTask = AddressablesSystem<Material>.GetOrLoadAddressable("M_TileIdle.mat");
                var loadPreviewAttackMaterialTask =
                    AddressablesSystem<Material>.GetOrLoadAddressable("M_TilePreviewAttack.mat");
                var loadPreviewMoveMaterialTask =
                    AddressablesSystem<Material>.GetOrLoadAddressable("M_TilePreviewMove.mat");

                //Results
                await loadIdleMaterialTask;
                Assert.NotNull(loadIdleMaterialTask.Result);
                await loadPreviewAttackMaterialTask;
                Assert.NotNull(loadPreviewAttackMaterialTask.Result);
                await loadPreviewMoveMaterialTask;
                Assert.NotNull(loadPreviewMoveMaterialTask.Result);

                //Set in data
                _materialMap.Add(ETileViewState.Idle, loadIdleMaterialTask.Result);
                _materialMap.Add(ETileViewState.PreviewMove, loadPreviewMoveMaterialTask.Result);
                _materialMap.Add(ETileViewState.PreviewAttack, loadPreviewAttackMaterialTask.Result);
            }
            catch (Exception e)
            {
                DebugSystem.Error($"Failed to load addressables due to {e.Message}");
                throw;
            }
        }


        protected void MoveToActivePosition()
        {
            tileObject.GetMoveableObject().MoveTo(startingPos + (Vector3.up * yMoveOnHover), moveTime);
        }

        protected void MoveToIdlePosition()
        {
            tileObject.GetMoveableObject().MoveTo(startingPos, moveTime);
        }

        protected void SetIdleMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.Idle, out var mat)) return;
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.SetMaterials(new List<Material> { mat });
            }
        }

        protected void SetPreviewAttackMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.PreviewAttack, out var mat)) return;
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.SetMaterials(new List<Material> { mat });
            }
        }

        protected void SetPreviewMoveMaterial()
        {
            if (!_materialMap.TryGetValue(ETileViewState.PreviewMove, out var mat)) return;
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.SetMaterials(new List<Material> { mat });
            }
        }

        protected void HandleCardPlayPreview(CardObject card)
        {
            var response = GameplaySystem.GetTilesPieceWouldOccupy(
                card.GetLogic().GetCardData().GetAssociatedPieceData(),
                tileObject.GetState().GetGridLocation());

            for (int i = 0; i < response.foundItems.Length; i++)
            {
                response.foundItems[i].GetState().GetViewStateMachine().SetState(ETileViewState.PreviewPlayCard);
                _currentlyPreviewedTiles.Add(response.foundItems[i]);
            }

            PiecePreviewSingleton.instance.PreviewPieceOnTile(
                card.GetLogic().GetCardData().GetAssociatedPieceData(), tileObject);
        }
    }
}