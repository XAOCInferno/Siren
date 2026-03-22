using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public class CardViewModelData
    {
        public Sprite mainImage;
        public string titleText;
        public string costText;
    }

    public class CardViewModel : MonoBehaviour
    {
        [SerializeField] protected Image mainImageComponent;
        [SerializeField] protected TextMeshProUGUI titleTextComponent;
        [SerializeField] protected TextMeshProUGUI costTextComponent;
        [SerializeField] protected Image borderImageComponent;

        protected readonly CardViewModelData cardViewModelData = new();

        public CardViewModelData GetViewModelData() => cardViewModelData;
        public void SetViewModelData(CardViewModelData data)
        {
            SetMainImage(data.mainImage);
            SetTitleText(data.titleText);
            SetCostText(data.costText);
        }

        public void SetMainImage(Sprite sprite)
        {
            if (cardViewModelData.mainImage != sprite)
            {
                cardViewModelData.mainImage = sprite;
                mainImageComponent.sprite = sprite;
            }
        }

        public void SetTitleText(string text)
        {
            if (cardViewModelData.titleText != text)
            {
                cardViewModelData.titleText = text;
                titleTextComponent.text = text;
            }
        }

        public void SetCostText(string text)
        {
            if (cardViewModelData.costText != text)
            {
                cardViewModelData.costText = text;
                costTextComponent.text = text;
            }
        }

        public void SetBorderTint(Color color)
        {
            borderImageComponent.color = color;
        }

        public void SetBorderVisibility(bool isVisible)
        {
            borderImageComponent.gameObject.SetActive(isVisible);
        }
    }
}