using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

using MelonLoader;

using UnityEngine;

using Entanglement.UI;
using Entanglement.Extensions;

namespace Entanglement.Network
{
    public static class DiscordIntegration
    {
        public static bool hasServer;
        public static bool isHost;
        public static PlayerId localId;

        public static byte GetByteId(ulong userId)
        {
            PlayerId playerId = PlayerIds.GetPlayerFromLargeId(userId);
            if (playerId != null)
            {
                return PlayerIds.GetPlayerFromLargeId(userId).SmallId;
            }
            return 255;
        }

        public static ulong GetLongId(byte byteId)
        {
            PlayerId playerId = PlayerIds.GetPlayerFromSmallId(byteId);
            if (playerId != null)
            {
                return PlayerIds.GetPlayerFromSmallId(byteId).LargeId;
            }
            return 0;
        }
    }
}
