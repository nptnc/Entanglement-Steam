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
        private static List<PlayerId> playerIds = new List<PlayerId>();

        public static void Dispose(PlayerId playerId) {
            // PlayerRepresentation rep = PlayerRepresentation.representations[playerId.LargeId];
            //rep.DeleteRepresentations();

            playerIds.Remove(playerId);
        }

        public static void Add(ulong largeId, byte smallId, string userName) {
            if (GetPlayerFromLargeId(largeId) == null)
                return;
            
            playerIds.Add(new PlayerId(largeId,smallId,userName));
        }

        public static PlayerId GetPlayerFromSmallId(byte SmallId) => playerIds.Where(id => id.SmallId == SmallId).FirstOrDefault();
        public static PlayerId GetPlayerFromLargeId(ulong LargeId) => playerIds.Where(id => id.LargeId == LargeId).FirstOrDefault();
    }
}
