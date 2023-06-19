using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.src.Network.PlayerId
{
    public class PlayerId
    {
        public ulong LargeId;
        public byte SmallId;
        public string Username;

        public PlayerId(ulong LargeId, byte SmallId, string Username)
        {
            this.LargeId = LargeId;
            this.SmallId = SmallId;
            this.Username = Username;
        }
    }
}
