    namespace Entanglement.Network
{
    // This used to be an enum but C# doesn't like casting to explicit enum types so this is a class for now
    public class BuiltInMessageType
    {
        public static byte
            Unknown = 0,
            PlayerRepSync = 1,
            GunShot = 2,
            SpawnObject = 3,
            LevelChange = 4,
            PlayerAttack = 5,
            Connection = 6,
            Disconnect = 7,
            ModAsset = 8,
            HandPose = 9,
            GripRadius = 10,
            BalloonShot = 11,
            PowerPunch = 12,
            TransformSync = 13,
            PuppetSync = 14,
            TransformQueue = 15,
            PuppetQueue = 16,
            TransformCreate = 17,
            PuppetCreate = 18,
            IDCallback = 19,
            ZombieMode = 20,
            ZombieLoadout = 21,
            ZombieDiff = 22,
            ZombieStart = 23,
            ZombieWave = 24,
            FantasyCount = 25,
            FantasyDiff = 26,
            FantasyChal = 27,
            ShortId = 28,
            MagazinePlug = 29,
            TransactionBegin = 30,
            TransactionWork = 31,
            ObjectDestroy = 32,
            Heartbeat = 33,
            TransformCollision = 34,
            SpawnRequest = 35,
            SpawnClient = 36,
            SpawnTransfer = 37,
            GripEvent = 38,
            PlayerEvent = 39;
    }
}
