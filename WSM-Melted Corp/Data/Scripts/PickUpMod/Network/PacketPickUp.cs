using ProtoBuf;
using VRage.Game.ModAPI;
using VRageMath;

namespace PickUpMod.PickUpMod
{
    [ProtoContract]
    class PacketPickUp : MyEasyNetworkManager.IPacket
    {

        [ProtoMember(1)]
        public Vector3 DesiredPos { get; private set; }

        [ProtoMember(2)]
        public Vector3 Translation { get; private set; }

        [ProtoMember(3)]
        public Vector3 Foward { get; private set; }

        [ProtoMember(4)]
        public long GridId { get; private set; }
		
        [ProtoMember(5)]
        public Vector3 Rotation { get; private set; }
        [ProtoMember(6)]
        public bool IsThrow { get; private set; }
        [ProtoMember(7)]
        public long characterID { get; private set; }

        public PacketPickUp() { }

        public PacketPickUp(long gridId, Vector3 Foward, Vector3 pos, Vector3 translation, Vector3 rot, bool IsThrow, IMyCharacter charact)
        {
            this.GridId = gridId;
            this.Foward = Foward;
            this.Translation = translation;
            this.DesiredPos = pos;
			this.Rotation = rot;
            this.IsThrow = IsThrow;
            this.characterID = charact.EntityId;
        }

        public int GetId()
        {
            return 1;
        }
    }
}
