using Entanglement.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.src.Network.PlayerId
{
    public static class PlayerIds
    {
        public static List<PlayerId> playerIds = new List<PlayerId>();

        public static void Dispose(PlayerId playerId)
        {
            
        }

        public static PlayerId GetPlayerFromSmallId(byte SmallId) => playerIds.Where(id => id.SmallId == SmallId).FirstOrDefault();
        public static PlayerId GetPlayerFromLargeId(ulong LargeId) => playerIds.Where(id => id.LargeId == LargeId).FirstOrDefault();
    }
}
