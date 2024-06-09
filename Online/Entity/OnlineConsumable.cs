﻿namespace RainMeadow
{
    public class OnlineConsumable : OnlinePhysicalObject
    {
        public class OnlineConsumableDefinition : OnlinePhysicalObjectDefinition
        {
            [OnlineField]
            public short originRoom;
            [OnlineField]
            public sbyte placedObjectIndex;

            public OnlineConsumableDefinition() { }

            public OnlineConsumableDefinition(OnlineConsumable onlineConsumable, OnlineResource inResource) : base(onlineConsumable, inResource)
            {
                this.originRoom = (short)onlineConsumable.Consumable.originRoom;
                this.placedObjectIndex = (sbyte)onlineConsumable.Consumable.placedObjectIndex;
            }

            public override OnlineEntity MakeEntity(OnlineResource inResource, OnlineEntity.EntityState initialState)
            {
                return new OnlineConsumable(this, inResource, (OnlineConsumableState)initialState);
            }
        }

        public OnlineConsumable(OnlineConsumableDefinition entityDefinition, OnlineResource inResource, OnlineConsumableState initialState) : base(entityDefinition, inResource, initialState)
        {

        }

        public OnlineConsumable(AbstractConsumable ac, EntityId id, OnlinePlayer owner, bool isTransferable) : base(ac, id, owner, isTransferable)
        {

        }

        protected override AbstractPhysicalObject ApoFromDef(OnlinePhysicalObjectDefinition newObjectEvent, OnlineResource inResource, PhysicalObjectEntityState initialState)
        {
            OnlineConsumableDefinition entityDefinition = (OnlineConsumableDefinition)newObjectEvent;
            var acm = (AbstractConsumable)ApoFromDef(newObjectEvent, inResource, initialState);
            acm.originRoom = entityDefinition.originRoom;
            acm.placedObjectIndex = entityDefinition.placedObjectIndex;
            acm.isConsumed = (initialState as OnlineConsumableState).isConsumed;
            return acm;
        }

        public AbstractConsumable Consumable => apo as AbstractConsumable;

        internal override EntityDefinition MakeDefinition(OnlineResource onlineResource)
        {
            return new OnlineConsumableDefinition(this, onlineResource);
        }

        protected override EntityState MakeState(uint tick, OnlineResource inResource)
        {
            return new OnlineConsumableState(this, inResource, tick);
        }

        public class OnlineConsumableState : PhysicalObjectEntityState
        {
            [OnlineField]
            public bool isConsumed;

            public OnlineConsumableState() { }

            public OnlineConsumableState(OnlineConsumable onlineEntity, OnlineResource inResource, uint ts) : base(onlineEntity, inResource, ts)
            {
                isConsumed = onlineEntity.Consumable.isConsumed;
            }

            public override void ReadTo(OnlineEntity onlineEntity)
            {
                base.ReadTo(onlineEntity);
                var onlineConsumable = onlineEntity as OnlineConsumable;
                if (!onlineConsumable.Consumable.isConsumed && isConsumed) onlineConsumable.Consumable.Consume();
            }
        }
    }
}
