﻿using System;
using System.Collections.Generic;
using NServiceBus;
using NServiceBus.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

class Logger :
    ILog
{
    ILogger logger;
    MessageTemplateParser templateParser;

    public Logger(ILogger logger)
    {
        this.logger = logger;
        templateParser = new MessageTemplateParser();
    }

    void WriteExceptionEvent(string message, Exception exception, LogEventLevel level)
    {
        var properties = new List<LogEventProperty>();
        if (exception.TryReadData("Message type", out string messageType))
        {
            properties.Add(new LogEventProperty("IncomingMessageType", new ScalarValue(messageType)));
        }
        if (exception.TryReadData("Message ID", out string incomingMessageId))
        {
            properties.Add(new LogEventProperty("IncomingMessageId", new ScalarValue(incomingMessageId)));
        }
        if (exception.TryReadData("Transport message ID", out string incomingTransportMessageId))
        {
            properties.Add(new LogEventProperty("IncomingTransportMessageId", new ScalarValue(incomingTransportMessageId)));
        }
        if (exception.TryReadData("Handler start time", out string handlerStartTime))
        {
            properties.Add(new LogEventProperty("HandlerStartTime", new ScalarValue(DateTimeExtensions.ToUtcDateTime(handlerStartTime))));
        }
        if (exception.TryReadData("Handler failure time", out string handlerFailureTime))
        {
            properties.Add(new LogEventProperty("HandlerFailureTime", new ScalarValue(DateTimeExtensions.ToUtcDateTime(handlerFailureTime))));
        }
        if (exception.TryReadData("Handler type", out string handlerType))
        {
            properties.Add(new LogEventProperty("HandlerType", new ScalarValue(handlerType)));
        }

        if (exception.TryReadData("ExceptionLogState", out ExceptionLogState logState))
        {
            properties.Add(new LogEventProperty("ProcessingEndpoint", new ScalarValue(logState.ProcessingEndpoint)));
            if (logState.CorrelationId != null)
            {
                properties.Add(new LogEventProperty("CorrelationId", new ScalarValue(logState.CorrelationId)));
            }

            if (logState.ConversationId != null)
            {
                properties.Add(new LogEventProperty("ConversationId", new ScalarValue(logState.ConversationId)));
            }

            if (logState.IncomingMessage != null)
            {
                if (logger.BindProperty("IncomingMessage", logState.IncomingMessage, out var messageProperty))
                {
                    properties.Add(messageProperty);
                }
            }

            if (logger.BindProperty("IncomingHeaders", logState.IncomingHeaders, out var headersProperty))
            {
                properties.Add(headersProperty);
            }
        }

        var messageTemplate = templateParser.Parse(message);
        var logEvent = new LogEvent(DateTimeOffset.Now, level, exception, messageTemplate, properties);
        logger.Write(logEvent);
    }

    public void Debug(string message)
    {
        logger.Debug(message);
    }

    public void Debug(string message, Exception exception)
    {
        WriteExceptionEvent(message, exception, LogEventLevel.Debug);
    }

    public void DebugFormat(string format, params object[] args)
    {
        logger.Debug(format, args);
    }

    public void Info(string message)
    {
        logger.Information(message);
    }

    public void Info(string message, Exception exception)
    {
        WriteExceptionEvent(message, exception, LogEventLevel.Information);
    }

    public void InfoFormat(string format, params object[] args)
    {
        logger.Information(format, args);
    }

    public void Warn(string message)
    {
        logger.Warning(message);
    }

    public void Warn(string message, Exception exception)
    {
        WriteExceptionEvent(message, exception, LogEventLevel.Warning);
    }

    public void WarnFormat(string format, params object[] args)
    {
        logger.Warning(format, args);
    }

    public void Error(string message)
    {
        logger.Error(message);
    }

    public void Error(string message, Exception exception)
    {
        WriteExceptionEvent(message, exception, LogEventLevel.Error);
    }

    public void ErrorFormat(string format, params object[] args)
    {
        logger.Error(format, args);
    }

    public void Fatal(string message)
    {
        logger.Fatal(message);
    }

    public void Fatal(string message, Exception exception)
    {
        WriteExceptionEvent(message, exception, LogEventLevel.Fatal);
    }

    public void FatalFormat(string format, params object[] args)
    {
        logger.Fatal(format, args);
    }

    public bool IsDebugEnabled => logger.IsEnabled(LogEventLevel.Debug);
    public bool IsInfoEnabled => logger.IsEnabled(LogEventLevel.Information);
    public bool IsWarnEnabled => logger.IsEnabled(LogEventLevel.Warning);
    public bool IsErrorEnabled => logger.IsEnabled(LogEventLevel.Error);
    public bool IsFatalEnabled => logger.IsEnabled(LogEventLevel.Fatal);
}