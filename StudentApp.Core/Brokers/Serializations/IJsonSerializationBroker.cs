// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Brokers.Serializations
{
    public interface IJsonSerializationBroker
    {
        ValueTask<string> SerializeAsync<T>(T value);
    }
}
