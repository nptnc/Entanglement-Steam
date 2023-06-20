using Entanglement.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Representation;
using Entanglement.Network;

namespace Entanglement.Network
{
    public static class PlayerIds
    {
        public static List<PlayerId> playerIds = new List<PlayerId>();

        public static byte TryGetAvailableSmallId() {
            for (byte i = 0; i < 255; i++) {
                if (GetPlayerFromSmallId(i) == null) {
                    return i;
                }
            }
            return 0;
        }

        public static void ClearAll() { 
            playerIds.Clear();
        }

        public static void Dispose(PlayerId playerId) {
            // PlayerRepresentation rep = PlayerRepresentation.representations[playerId.LargeId];
            //rep.DeleteRepresentations();

            playerIds.Remove(playerId);
        }

        public static PlayerId GetOwner() => playerIds.FirstOrDefault(id => id.SmallId == 0);

        public static PlayerId Add(ulong largeId, byte smallId, string userName) {
            if (GetPlayerFromLargeId(largeId) != null)
                return null;

            EntangleLogger.Log($"Registered {userName} with large id {largeId} and smallId {smallId}");

            PlayerId playerId = new PlayerId(largeId, smallId, userName);
            playerIds.Add(playerId);
            return playerId;
        }

        public static PlayerId GetPlayerFromSmallId(byte SmallId) => playerIds.Where(id => id.SmallId == SmallId).FirstOrDefault();
        public static PlayerId GetPlayerFromLargeId(ulong LargeId) => playerIds.Where(id => id.LargeId == LargeId).FirstOrDefault();
    }
}
