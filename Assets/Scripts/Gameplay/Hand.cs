using System;
using System.Collections.Generic;
using Behaviours;
using Debug;
using Gameplay.Card;
using Global;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Utils;

namespace Gameplay
{
    public class HandData
    {
    }

    public class HandSlot
    {
        public CardLogic cardLogicInSlot;

        private Vector3 position;
        private Quaternion rotation;

        public HandSlot(CardLogic cardLogicInSlot)
        {
            this.cardLogicInSlot = cardLogicInSlot;
            UpdateCardPosition();
            UpdateCardRotation();
        }

        public void SetPosition(Vector3 newPos)
        {
            position = newPos;
            UpdateCardPosition();
        }

        public void SetRotation(Quaternion newRotation)
        {
            this.rotation = newRotation;
            UpdateCardRotation();
        }

        protected void UpdateCardPosition()
        {
            if (cardLogicInSlot != null)
            {
                cardLogicInSlot.MoveToPosition(position);
            }
        }

        protected void UpdateCardRotation()
        {
            cardLogicInSlot.transform.rotation = rotation;
        }
    }

    public class Hand : MonoBehaviour
    {
        [SerializeField] protected SplineContainer cardPositionSplineContainer;
        [SerializeField] protected Transform handCenterTransform;
        [SerializeField] protected bool isLocallyControlled;
        protected HandData handData = new();

        protected List<HandSlot> handSlots = new();

        protected void Awake()
        {
            HandEvents.OnAddCardToHand += OnAddCardToHand;
            HandEvents.OnRemoveCardFromHand += OnRemoveCardFromHand;
        }

        protected void OnAddCardToHand([CanBeNull] object sentFrom, HandEvents.AddCardToHandPayload payload)
        {
            //Ensure we're only adding to the correct hand
            if (isLocallyControlled != payload.player.playerData.isLocallyControlled)
            {
                return;
            }

            payload.cardLogic.transform.position = payload.fromPosition;
            payload.cardLogic.GetComponent<CardState>().GetLogicStateMachine().SetState(ECardLogicState.InHand);
            handSlots.Add(new HandSlot(payload.cardLogic));
            UpdateHandSlotLocations();
        }

        protected void OnRemoveCardFromHand([CanBeNull] object sentFrom, HandEvents.RemoveCardFromHandPayload payload)
        {
            //Find idx of card
            int idxActive = handSlots.FindIndex((v) => v.cardLogicInSlot == payload.cardLogic);
            if (idxActive == -1) return;
            //Remove it, if present in our hand
            //TODO: Animation and state change, then return on complete, rather than instantly returning like this
            handSlots.RemoveAt(idxActive);
            UpdateHandSlotLocations();
        }

        protected void UpdateHandSlotLocations()
        {
            //Err check
            if (cardPositionSplineContainer == null)
            {
                DebugSystem.Error("Cannot set Hand Slot locations as spline is not set.");
                return;
            }

            if (handCenterTransform == null)
            {
                DebugSystem.Error("Cannot set Hand Slot locations as hand  center transform is not set.");
                return;
            }

            //Return early if we have no cards
            if (handSlots.Count == 0)
            {
                return;
            }

            //TODO: How do we position the AIs cards?
            if (isLocallyControlled)
            {
                //TODO: Spread them out better so the cards look like they're touching
                Spline spline = cardPositionSplineContainer.Spline;
                float cardSpacing = 1f / handSlots.Count;
                float firstCardPosition = 0.5f - (handSlots.Count - 1) * cardSpacing / 2;
                for (int i = 0; i < handSlots.Count; i++)
                {
                    float p = firstCardPosition + (i * cardSpacing);
                    Vector3 splinePosition = spline.EvaluatePosition(p);
                    Vector3 fwd = spline.EvaluateTangent(p);
                    Vector3 up = spline.EvaluateUpVector(p);
                    Quaternion rotation = Quaternion.LookRotation(CameraSubsystem.GetMainCamera().transform.forward,
                        Vector3.Cross(up * -1, fwd).normalized);
                    handSlots[i].SetPosition(handCenterTransform.position + splinePosition);
                    handSlots[i].SetRotation(rotation);
                }
            }
        }
    }
}