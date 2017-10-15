using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AirlockAmmoGenerator
{
    public class AirlockAmmoGenerator : IAmmoGenerator
    {
        private readonly string _apiKey;
        private readonly IAirlockEventGenerator _eventGenerator;
        private readonly Uri _sendUri;

        public AirlockAmmoGenerator(string host, int port, string apiKey, IAirlockEventGenerator eventGenerator)
        {
            _apiKey = apiKey;
            _eventGenerator = eventGenerator;
            _sendUri = BuildSendUri(host, port);
        }

        public IEnumerable<Ammo> Generate(int count)
        {
            foreach (var e in _eventGenerator.Generate().Take(count))
            {
                yield return new Ammo
                {
                    Target = _sendUri,
                    Body = e,
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Length", e.Length.ToString(NumberFormatInfo.InvariantInfo)},
                        {"x-apikey", _apiKey}
                    }
                };
            }
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