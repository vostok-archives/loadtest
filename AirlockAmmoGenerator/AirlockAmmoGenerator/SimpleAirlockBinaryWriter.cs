using System;
using System.IO;
using System.Text;
using Vostok.Commons.Binary;

namespace AirlockAmmoGenerator
{
    public class SimpleAirlockBinaryWriter : IBinaryWriter, IDisposable
    {
        private readonly BinaryWriter _binaryWriter;

        public SimpleAirlockBinaryWriter(Stream stream) => _binaryWriter = new BinaryWriter(stream);

        public void Write(int value) => _binaryWriter.Write(value);

        public void Write(long value) => _binaryWriter.Write(value);

        public void Write(short value) => _binaryWriter.Write(value);

        public void Write(uint value) => _binaryWriter.Write(value);

        public void Write(ulong value) => _binaryWriter.Write(value);

        public void Write(ushort value) => _binaryWriter.Write(value);

        public void Write(byte value) => _binaryWriter.Write(value);

        public void Write(bool value) => _binaryWriter.Write(value);

        public void Write(float value) => _binaryWriter.Write(value);

        public void Write(double value) => _binaryWriter.Write(value);

        public void Write(Guid value) => _binaryWriter.Write(value.ToByteArray());

        public void Write(string value, Encoding encoding)
        {
            var bytes = encoding.GetBytes(value);
            _binaryWriter.Write(bytes.Length);
            _binaryWriter.Write(bytes);
        }

        public void WriteWithoutLengthPrefix(string value, Encoding encoding) => _binaryWriter.Write(encoding.GetBytes(value));

        public void Write(byte[] value)
        {
            _binaryWriter.Write(value.Length);
            _binaryWriter.Write(value);
        }

        public void Write(byte[] value, int offset, int length)
        {
            _binaryWriter.Write(length);
            _binaryWriter.Write(value, offset, length);
        }

        public void WriteWithoutLengthPrefix(byte[] value) => _binaryWriter.Write(value);

        public void WriteWithoutLengthPrefix(byte[] value, int offset, int length) => _binaryWriter.Write(value, offset, length);

        public int Position
        {
            get => (int) _binaryWriter.BaseStream.Position;
            set => _binaryWriter.Seek(value, SeekOrigin.Begin);
        }

        public void Dispose() => _binaryWriter?.Dispose();

        public void Flush() => _binaryWriter.Flush();
    }
}