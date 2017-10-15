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
        private readonly AirlockMessageSerializer _messageSerializer;

        public AirlockAmmoGenerator(string host, int port, string apiKey, IAirlockEventGenerator eventGenerator)
        {
            _apiKey = apiKey;
            _eventGenerator = eventGenerator;
            _sendUri = BuildSendUri(host, port);
            _messageSerializer = new AirlockMessageSerializer();
        }

        public IEnumerable<Ammo> Generate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var events = _eventGenerator.Generate().Take(100);
                var records = events.Select(x => new EventRecord {Timestamp = DateTimeOffset.Now, Body = x}).ToList();
                var group = new EventGroup {EventRecords = records, RoutingKey = "load.tank.load.logs"};
                var message = new AirlockMessage {EventGroups = new List<EventGroup> {group}};
                var messageBytes = _messageSerializer.Serialize(message);

                yield return new Ammo
                {
                    Target = _sendUri,
                    Body = messageBytes,
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Length", messageBytes.Length.ToString(NumberFormatInfo.InvariantInfo)},
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