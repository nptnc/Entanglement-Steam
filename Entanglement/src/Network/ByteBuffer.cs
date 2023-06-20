using Entanglement.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.src.Network
{
    public class ByteBuffer
    {
        private byte[] Buffer;
        private readonly List<byte> Bytes = new List<byte>();
        private int _readPos;
        private readonly bool _sizeLess = false;

        public ByteBuffer()
        {
            _sizeLess = true;
        }

        public ByteBuffer(int size)
        {
            Buffer = new byte[size];
            _readPos = 0;
        }

        public ByteBuffer(byte[] data)
        {
            Buffer = data;
            _readPos = 0;
        }

        public int ReadInt()
        {
            var value = BitConverter.ToInt32(Buffer, _readPos);
            _readPos += sizeof(int);
            return value;
        }

        public string ReadString()
        {
            var value = System.Text.Encoding.UTF8.GetString(Buffer, _readPos, Buffer.Length - _readPos);
            _readPos = Buffer.Length;
            return value;
        }

        public PlayerId ReadPlayerEntry()
        {
            return PlayerIds.GetPlayerFromSmallId(ReadByte());
        }

        public float ReadFloat()
        {
            var value = BitConverter.ToSingle(Buffer, _readPos);
            _readPos += sizeof(float);
            return value;
        }

        public short ReadShort()
        {
            var value = BitConverter.ToInt16(Buffer, _readPos);
            _readPos += sizeof(short);
            return value;
        }

        public sbyte ReadSByte()
        {
            var value = (sbyte) Buffer[_readPos];
            _readPos += sizeof(sbyte);
            return value;
        }

        public byte ReadByte()
        {
            var value = Buffer[_readPos];
            _readPos += sizeof(byte);
            return value;
        }

        public bool ReadBool()
        {
            var value = BitConverter.ToBoolean(Buffer, _readPos);
            _readPos += sizeof(bool);
            return value;
        }

        public ushort ReadUShort()
        {
            var value = BitConverter.ToUInt16(Buffer, _readPos);
            _readPos += sizeof(ushort);
            return value;
        }

        public ulong ReadULong()
        {
            var value = BitConverter.ToUInt64(Buffer, _readPos);
            _readPos += sizeof(ulong);
            return value;
        }

        public void WriteULong(ulong value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteUShort(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteBool(bool value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteInt(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteString(string value)
        {
            WriteBytes(System.Text.Encoding.UTF8.GetBytes(value));
        }

        public void WriteFloat(float value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteShort(short value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteByte(byte value)
        {
            InternalAttemptWrite(value);
        }

        public void WriteSByte(sbyte value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteBytes(byte[] value)
        {
            foreach (byte b in value)
            {
                InternalAttemptWrite(b);
            }
        }

        public void WritePlayerEntry(PlayerId entry)
        {
            WriteByte(entry.SmallId);
        }

        public byte[] GetByteRange(int length)
        {
            if (_sizeLess)
            {
                Buffer = Bytes.ToArray();
            }

            var bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = Buffer[_readPos];
                _readPos++;
            }
            return bytes;
        }

        public byte[] GetRemainingBytes() {
            return GetByteRange(Buffer.Length - _readPos);
        }

        public byte[] GetBytes()
        {
            if (_sizeLess)
            {
                return Bytes.ToArray();
            }

            return Buffer;
        }

        private void InternalAttemptWrite(byte b)
        {
            if (_sizeLess)
            {
                Bytes.Add(b);
            }
            else
            {
                Buffer[_readPos] = b;
                _readPos++;
            }
        }
    }
}
