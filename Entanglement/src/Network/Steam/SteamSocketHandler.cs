using Entanglement.Network;

namespace Entanglement.Network {
    public static class SteamSocketHandler {
        public static void OnPlayerVerified(PlayerId playerId) {
            // TODO: send every other player this playerid
            // TODO: send this playerid to the player who owns it
            // TODO: send every other playerid to this player
        }
    }
}