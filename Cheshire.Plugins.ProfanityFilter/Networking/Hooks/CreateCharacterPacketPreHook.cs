using Cheshire.Plugins.ProfanityFilter.Configuration;
using Cheshire.Plugins.Utilities.Logging;

using Intersect.Network;
using Intersect.Network.Packets.Client;
using Intersect.Network.Packets.Server;

namespace Cheshire.Plugins.ProfanityFilter.Networking.Hooks
{
    public class CreateCharacterPacketPreHook : IPacketHandler<CreateCharacterPacket>
    {
        public bool Handle(IPacketSender packetSender, CreateCharacterPacket packet)
        {
            // Are there any blocked words in the packet received just now?
            if (ProfanityFilter.HasFilteredWords(packet.Name))
            {
                packetSender.Send(new ErrorMessagePacket(string.Empty, PluginSettings.Settings.Strings.CharacterCreationError));
                return false;
            }

            return true;
        }

        public bool Handle(IPacketSender packetSender, IPacket packet) => Handle(packetSender, packet as CreateCharacterPacket);
    }
}
