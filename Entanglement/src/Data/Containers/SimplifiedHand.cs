using StressLevelZero.Player;

namespace Entanglement.Data
{
    public struct SimplifiedHand {
        public const ushort size = sizeof(byte) * 5;

        public float indexCurl;
        public float middleCurl;
        public float ringCurl;
        public float pinkyCurl;
        public float thumbCurl;

        public SimplifiedHand(float indexCurl, float middleCurl, float ringCurl, float pinkyCurl, float thumbCurl) {
            this.indexCurl = indexCurl;
            this.middleCurl = middleCurl;
            this.ringCurl = ringCurl;
            this.pinkyCurl = pinkyCurl;
            this.thumbCurl = thumbCurl;
        }

        public SimplifiedHand(FingerCurl curler) {
            indexCurl = curler.index;
            middleCurl = curler.middle;
            ringCurl = curler.ring;
            pinkyCurl = curler.pinky;
            thumbCurl = curler.thumb;
        }

        public byte[] GetBytes()
        {
            return new byte[] {
                (byte)(indexCurl * 255f),
                (byte)(middleCurl * 255f),
                (byte)(ringCurl * 255f),
                (byte)(pinkyCurl * 255f),
                (byte)(thumbCurl * 255f),
            };
        }

        public static SimplifiedHand FromBytes(byte[] bytes)
        {
            return new SimplifiedHand(
                bytes[0] / 255f,
                bytes[1] / 255f,
                bytes[2] / 255f,
                bytes[3] / 255f,
                bytes[4] / 255f
            );
        }
    }
}
