using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Entanglement.Data;
using Entanglement.Representation;
using Entanglement.Extensions;

using StressLevelZero;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class PlayerRepSyncHandler : NetworkMessageHandler<PlayerRepSyncData> {
        public override byte? MessageIndex => BuiltInMessageType.PlayerRepSync;

        public override NetworkMessage CreateMessage(PlayerRepSyncData data) {
            NetworkMessage message = new NetworkMessage();

            List<byte> rawBytes = new List<byte>();

            rawBytes.Add(DiscordIntegration.GetByteId(data.userId));
            rawBytes.Add(Convert.ToByte(data.isGrounded));

            rawBytes.AddRange(data.rootPosition.GetBytes());

            for (int r = 0; r < data.simplifiedTransforms.Length; r++)
                rawBytes.AddRange(data.simplifiedTransforms[r].GetSmallBytes(data.rootPosition));

            rawBytes.AddRange(data.simplifiedLeftHand.GetBytes());
            rawBytes.AddRange(data.simplifiedRightHand.GetBytes());

            message.messageData = rawBytes.ToArray();

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            long userId = DiscordIntegration.GetLongId(message.messageData[index++]);

            if (PlayerRepresentation.representations.ContainsKey(userId)) {
                PlayerRepresentation rep = PlayerRepresentation.representations[userId];

                if (rep.repFord) {

                    bool isGrounded = Convert.ToBoolean(message.messageData[index]);
                    index += sizeof(byte);
                    rep.isGrounded = isGrounded;

                    List<byte> data = message.messageData.ToList();

                    Vector3 rootPosition = new Vector3();

                    rootPosition.x = BitConverter.ToSingle(message.messageData, index);
                    index += sizeof(float);
                    rootPosition.y = BitConverter.ToSingle(message.messageData, index);
                    index += sizeof(float);
                    rootPosition.z = BitConverter.ToSingle(message.messageData, index);
                    index += sizeof(float);

                    rep.repRoot.position = rootPosition;

                    for (int r = 0; r < rep.repTransforms.Length; r++)
                    {
                        SimplifiedTransform simpleTransform = SimplifiedTransform.FromSmallBytes(data.GetRange(index, SimplifiedTransform.size_small).ToArray(), rootPosition);
                        index += SimplifiedTransform.size_small;

                        if (rep.repTransforms[r])
                            simpleTransform.Apply(rep.repTransforms[r]);
                    }

                    SimplifiedHand simplifiedLeftHand = SimplifiedHand.FromBytes(data.GetRange(index, SimplifiedHand.size).ToArray());
                    index += SimplifiedHand.size;
                    SimplifiedHand simplifiedRightHand = SimplifiedHand.FromBytes(data.GetRange(index, SimplifiedHand.size).ToArray());

                    rep.UpdateFingers(Handedness.LEFT, simplifiedLeftHand);
                    rep.UpdateFingers(Handedness.RIGHT, simplifiedRightHand);

                    if (rep.repCanvasTransform) {
                        rep.repCanvasTransform.position = rep.repTransforms[0].position + Vector3.up * 0.4f;

                        if (Camera.current)
                            rep.repCanvasTransform.rotation = Quaternion.LookRotation(Vector3.Normalize(rep.repCanvasTransform.position - Camera.current.transform.position), Vector3.up);
                    }
                }
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Unreliable, msgBytes, userId);
            }
        }
    }

    public class PlayerRepSyncData : NetworkMessageData {
        public long userId;
        public bool isGrounded;
        public SimplifiedTransform[] simplifiedTransforms = new SimplifiedTransform[3];
        public Vector3 rootPosition;
        public SimplifiedHand simplifiedLeftHand;
        public SimplifiedHand simplifiedRightHand;
    }
}
