﻿using System;
using System.Collections.Generic;

#pragma warning disable 1591

namespace NServiceBus.Serilog
{
    [Serializable]
    public class ExceptionLogState
    {
        public readonly string ProcessingEndpoint;
        public readonly string? CorrelationId;
        public readonly string? ConversationId;
        public string? HandlerType;
        public object? IncomingMessage;
        public readonly IReadOnlyDictionary<string, string> IncomingHeaders;

        public ExceptionLogState(string processingEndpoint, IReadOnlyDictionary<string, string> incomingHeaders, string? correlationId, string? conversationId)
        {
            Guard.AgainstNull(processingEndpoint, nameof(processingEndpoint));
            ProcessingEndpoint = processingEndpoint;
            IncomingHeaders = incomingHeaders;
            CorrelationId = correlationId;
            ConversationId = conversationId;
        }

        public static bool TryReadFromException(Exception exception, out ExceptionLogState state)
        {
            var data = exception.Data;
            if (data.Contains("ExceptionLogState"))
            {
                state = (ExceptionLogState) data["ExceptionLogState"];
                return true;
            }

#pragma warning disable CS8625
            state = null;
#pragma warning restore CS8625
            return false;
        }
    }
}