using System;
using System.Collections.Generic;
using Behaviours;
using Debug;
using Global;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Gameplay
{
    public class HandData
    {
    }

    public class HandSlot
    {
        public Card cardInSlot;

        private Vector3 position;
        private Quaternion rotation;

        public HandSlot(Card cardInSlot)
        {
            this.cardInSlot = cardInSlot;
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
            if (cardInSlot != null)
            {
                cardInSlot.transform.position = position;
            }
        }

        protected void UpdateCardRotation()
        {
            cardInSlot.transform.rotation = rotation;
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
        }

        protected void OnAddCardToHand([CanBeNull] object sentFrom, HandEvents.AddCardToHandPayload payload)
        {
            if (isLocallyControlled == payload.Player.playerData.isLocallyControlled)
            {
                payload.Card.SetState(ECardState.InHand);
                handSlots.Add(new HandSlot(payload.Card));
                UpdateHandSlotLocations();
            }
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
                    //rotation = CameraSubsystem.GetMainCamera().transform.rotation * rotation;
                    handSlots[i].SetPosition(handCenterTransform.position + splinePosition);
                    handSlots[i].SetRotation(rotation);
                }
            }
        }
    }
}