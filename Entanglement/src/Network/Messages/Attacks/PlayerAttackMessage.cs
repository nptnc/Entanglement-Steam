using System;

using Entanglement.Data;
using Entanglement.Representation;
using Entanglement.Extensions;

using StressLevelZero.Combat;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class PlayerAttackMessageHandler : NetworkMessageHandler<PlayerAttackMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.PlayerAttack;


        public override NetworkMessage CreateMessage(PlayerAttackMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(byte) + sizeof(ushort)];

            int index = 0;
            message.messageData[index++] = (byte)data.attackType;

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes((ushort)(data.attackDamage * 10000f)), ref index);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            AttackType attackType = (AttackType)message.messageData[index++];

            float attackDamage = BitConverter.ToUInt16(message.messageData, index) / 10000f;

            PlayerScripts.playerHealth.TAKEDAMAGE(attackDamage);

            // Play Sound
            if (PlayerRepresentation.representations.ContainsKey(sender))
            {
                PlayerRepresentation rep = PlayerRepresentation.representations[sender];
                switch (attackType) {
                    case AttackType.Stabbing:
                        rep.repStabSFX.GunShot();
                        break;
                }
            }
        }
    }

    public class PlayerAttackMessageData : NetworkMessageData {
        public AttackType attackType = AttackType.None;
        public float attackDamage;
    }
}
