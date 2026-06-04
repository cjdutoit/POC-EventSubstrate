// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Text.Json;

namespace StudentApp.Core.Brokers.Serializations
{
    public sealed class JsonSerializationBroker : IJsonSerializationBroker
    {
        public ValueTask<string> SerializeAsync<T>(T value) =>
            ValueTask.FromResult(JsonSerializer.Serialize(value));
    }
}
