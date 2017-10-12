using System;
using System.IO;
using Vostok.Airlock;
using Vostok.Commons.Binary;

namespace AirlockAmmoGenerator
{
    public class SimpleAirlockSink : IAirlockSink, IDisposable
    {
        private readonly SimpleAirlockBinaryWriter _writer;
        private readonly MemoryStream _writeStream;

        public SimpleAirlockSink()
        {
            _writeStream = new MemoryStream();
            _writer = new SimpleAirlockBinaryWriter(WriteStream);
        }

        public Stream WriteStream => _writeStream;

        public IBinaryWriter Writer => _writer;

        public byte[] ToArray()
        {
            _writer.Flush();
            return _writeStream.ToArray();
        }

        public void Dispose()
        {
            _writer.Dispose();
            WriteStream?.Dispose();
        }
    }
}