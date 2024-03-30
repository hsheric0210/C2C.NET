using C2C.Processor;
using System;

namespace C2C.Handshake.Generator
{
    public interface IHandshakeGenerator
    {
        IHandshake Generate(Guid channelId, IProcessor[] processors);
    }
}