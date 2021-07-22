using Intersect.Network;
using Intersect.Network.Packets.Client;

namespace Cheshire.Plugins.ProfanityFilter.Networking.Hooks
{
    public class ChatMsgPacketPreHook : IPacketHandler<ChatMsgPacket>
    {
        public bool Handle(IPacketSender packetSender, ChatMsgPacket packet)
        {
            packet.Message = ProfanityFilter.Apply(packet.Message);
            return true;
        }

        public bool Handle(IPacketSender packetSender, IPacket packet) => Handle(packetSender, packet as ChatMsgPacket);
    }
}
