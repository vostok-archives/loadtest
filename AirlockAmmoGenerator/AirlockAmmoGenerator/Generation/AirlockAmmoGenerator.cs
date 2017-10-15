using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AirlockAmmoGenerator.Gate;

namespace AirlockAmmoGenerator.Generation
{
    public class AirlockAmmoGenerator : IAmmoGenerator
    {
        private readonly string _apiKey;
        private readonly IMessageGenerator _messageGenerator;
        private readonly Uri _sendUri;
        private readonly AirlockMessageSerializer _messageSerializer;

        public AirlockAmmoGenerator(string host, int port, string apiKey, IMessageGenerator messageGenerator)
        {
            _apiKey = apiKey;
            _messageGenerator = messageGenerator;
            _sendUri = BuildSendUri(host, port);
            _messageSerializer = new AirlockMessageSerializer();
        }

        public IEnumerable<Ammo> Generate(int count)
        {
            var messages = _messageGenerator.Generate(count);
            return from msg in messages
                let bytes = _messageSerializer.Serialize(msg)
                select new Ammo
                {
                    Target = _sendUri,
                    Body = bytes,
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Length", bytes.Length.ToString(NumberFormatInfo.InvariantInfo)},
                        {"x-apikey", _apiKey}
                    }
                };
        }

        private Uri BuildSendUri(string host, int port)
        {
            var builder = new UriBuilder
            {
                Scheme = "http",
                Host = host,
                Port = port,
                Path = "send"
            };
            return builder.Uri;
        }
    }
}