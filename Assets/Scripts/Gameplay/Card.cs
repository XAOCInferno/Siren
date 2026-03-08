namespace Gameplay
{
    public enum ECardType
    {
        Structure,
        Troop,
        Reaction
    }
    public class CardData
    {
        public ECardType type;
    }

    public class Card : Utils.PooledObject
    {
        protected CardData cardData = new CardData();

        public override void SetActive()
        {
            throw new System.NotImplementedException();
        }

        public override void SetInActive()
        {
            transform.position = Global.Defines.OutOfWorldLocation;
        }
    }
}