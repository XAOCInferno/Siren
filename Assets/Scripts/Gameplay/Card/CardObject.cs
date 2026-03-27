using Behaviours;
using NUnit.Framework;
using Utils;

namespace Gameplay.Card
{
    public class CardObject : StateViewLogicObject<CardState, CardLogic, CardView>
    {
        protected DynamicObject dynamicObject;

        public override void Awake()
        {
            base.Awake();
            
            // Cache
            dynamicObject = GetComponent(typeof(DynamicObject)) as DynamicObject;
            Assert.NotNull(dynamicObject);
        }

        public DynamicObject GetMoveableObject() => dynamicObject;
    }
}