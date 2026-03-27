using Behaviours;
using Debug;
using NUnit.Framework;
using UI;
using UnityEngine;

namespace Gameplay.Card
{
    public class PlayingCardPreviewSingleton : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private float scale = 2;

        // Starting position when moving in
        [SerializeField] private Transform mkrMoveInFromLocation;

        // Ending position when moving in
        [SerializeField] private Transform mkrMoveInToLocation;
        private DynamicObject _dynamicObject;

        [SerializeField] protected GameObject graphicsParent;
        [SerializeField] protected CardViewModel viewModel;

        public static PlayingCardPreviewSingleton instance { get; private set; }

        private void Awake()
        {
            //Ensure singleton is unique
            if (instance != null && instance != this)
            {
                DebugSystem.Warn("Multiple PlayerPreviewCard in scene, this is invalid!");
                Destroy(gameObject);
                return;
            }

            //Set instance
            instance = this;

            // Cache
            _dynamicObject = GetComponent<DynamicObject>();

            //Ensure all required are not null
            Assert.NotNull(_dynamicObject);
            Assert.NotNull(viewModel);
            Assert.NotNull(graphicsParent);
            Assert.NotNull(mkrMoveInFromLocation);
            Assert.NotNull(mkrMoveInToLocation);

            //Set scale
            transform.localScale = Vector3.one * scale;

            //Hide at start
            graphicsParent.SetActive(false);
        }

        public void SetFocusedCardData(CardViewModelData data)
        {
            //Set our VM
            viewModel.SetViewModelData(data);

            //Do move in animation
            transform.position = mkrMoveInFromLocation.position;
            _dynamicObject.MoveTo(mkrMoveInToLocation.position, moveDuration);

            //Finally show the preview
            graphicsParent.SetActive(true);
        }

        public void ClearFocusedCardData()
        {
            //Move out the card
            transform.position = mkrMoveInToLocation.position;
            _dynamicObject.MoveTo(mkrMoveInFromLocation.position, moveDuration, type =>
            {
                //When moved out, deactivate it
                graphicsParent.SetActive(false);
                return 0;
            });
        }
    }
}