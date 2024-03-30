using System;

namespace C2C.Processor
{
    public interface IProcessor
    {
        Guid ProcessorID { get; }

        /// <summary>
        /// Get the processor negotiation data. It will be sent with the outgoing handshake packet.
        /// </summary>
        /// <remarks>
        /// Send the processor parameters such as DH parameters, compression ratio and others through this.
        /// </remarks>
        /// <returns>The negotiation data. (outgoing)</returns>
        byte[] GetNegotationData();

        /// <summary>
        /// Finish the processor negotiation by parsing the returned negotiation data from incoming handshake packet.
        /// </summary>
        /// <remarks>
        /// e.g. Receive and initiate the cipher by DH parameters.
        /// </remarks>
        /// <param name="negotiationData">The negotiation data send by the server. (server-to-client)</param>
        void FinishNegotiate(byte[] negotiationData);

        /// <summary>
        /// Process the outgoing data.
        /// </summary>
        /// <param name="data">The outgoing data.</param>
        /// <returns>The processed incoming data.</returns>
        byte[] ProcessOutgoingData(byte[] data);

        /// <summary>
        /// Process the incoming data.
        /// </summary>
        /// <param name="data">The incoming data.</param>
        /// <returns>The processed incoming data.</returns>
        byte[] ProcessIncomingData(byte[] data);
    }
}
