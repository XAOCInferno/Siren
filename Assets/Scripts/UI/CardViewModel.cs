using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Debug;
using Gameplay.Card;
using Gameplay.Piece;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    [Serializable]
    public class CardViewModelData
    {
        public Sprite mainImage;
        public string titleText;
        public string costText;

        [DoNotSerialize] protected ECardType type;

        public ECardType GetCardType() => type;

        public void SetType(ECardType cardType)
        {
            type = cardType;
        }
    }

    [Serializable]
    public struct CardTypeSpriteMap
    {
        public ECardType type;
        public Sprite image;
    }

    public class CardViewModel : MonoBehaviour
    {
        [SerializeField] protected Image mainImageComponent;
        [SerializeField] protected Image typeImageComponent;
        [SerializeField] protected TextMeshProUGUI titleTextComponent;
        [SerializeField] protected TextMeshProUGUI costTextComponent;
        [SerializeField] protected Image borderImageComponent;

        protected readonly CardViewModelData cardViewModelData = new();
        [SerializeField] protected CardTypeSpriteMap[] cardTypeSprites; //TODO: Convert this to addressables later?

        public CardViewModelData GetViewModelData() => cardViewModelData;

        public void SetViewModelData(CardViewModelData data)
        {
            SetMainImage(data.mainImage);
            SetTitleText(data.titleText);
            SetCostText(data.costText);
        }

        public void SetType(ECardType cardType)
        {
            SetTypeImage(cardType);
        }

        public ECardType GetCardType() => cardViewModelData.GetCardType();

        public void SetMainImage(Sprite sprite)
        {
            if (cardViewModelData.mainImage != sprite)
            {
                cardViewModelData.mainImage = sprite;
                mainImageComponent.sprite = sprite;
            }
        }

        public void SetTypeImage(ECardType cardType)
        {
            if (cardViewModelData.GetCardType() != cardType || !typeImageComponent.sprite)
            {
                // Update type
                cardViewModelData.SetType(cardType);

                // Try get and set image
                Sprite sprite =
                    (from cardTypeSprite in cardTypeSprites
                        where cardTypeSprite.type == cardType
                        select cardTypeSprite.image).FirstOrDefault();
                if (!sprite)
                {
                    DebugSystem.Error("Failed to display sprite for card type " + cardType);
                    return;
                }

                typeImageComponent.sprite = sprite;
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