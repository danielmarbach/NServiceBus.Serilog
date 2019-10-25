<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# <img src="/src/icon.png" height="30px"> NServiceBus.Serilog

[![Build status](https://ci.appveyor.com/api/projects/status/nmcughyrado8smay/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/nservicebus-Serilog)
[![NuGet Status](https://img.shields.io/nuget/v/NServiceBus.Serilog.svg?cacheSeconds=86400)](https://www.nuget.org/packages/NServiceBus.Serilog/)

Add support for sending [NServiceBus](http://particular.net/NServiceBus) logging through [Serilog](http://serilog.net/)

<!-- toc -->
## Contents

  * [Community backed](#community-backed)
    * [Sponsors](#sponsors)
    * [Patrons](#patrons)
  * [Usage](#usage)
  * [Filtering](#filtering)
  * [Tracing](#tracing)
    * [Create an instance of a Serilog logger](#create-an-instance-of-a-serilog-logger)
    * [Configure the tracing feature to use that logger](#configure-the-tracing-feature-to-use-that-logger)
    * [Contextual logger](#contextual-logger)
    * [Exception enrichment](#exception-enrichment)
    * [Saga tracing](#saga-tracing)
    * [Message tracing](#message-tracing)
  * [Logging to Seq](#logging-to-seq)
  * [Sample](#sample)
    * [Configure Serilog](#configure-serilog)
    * [Pass the configuration to NServiceBus](#pass-the-configuration-to-nservicebus)
    * [Ensure logging is flushed on shutdown](#ensure-logging-is-flushed-on-shutdown)
  * [Seq Sample](#seq-sample)
    * [Prerequisites](#prerequisites)
    * [Configure Serilog](#configure-serilog-1)
    * [Pass that configuration to NServiceBus](#pass-that-configuration-to-nservicebus)
    * [Ensure logging is flushed on shutdown](#ensure-logging-is-flushed-on-shutdown-1)
<!-- endtoc -->


<!--- StartOpenCollectiveBackers -->

[Already a Patron? skip past this section](#endofbacking)


## Community backed

**It is expected that all developers [become a Patron](https://opencollective.com/nservicebusextensions/order/6976) to use any of these libraries. [Go to licensing FAQ](https://github.com/NServiceBusExtensions/Home/blob/master/readme.md#licensingpatron-faq)**


### Sponsors

Support this project by [becoming a Sponsors](https://opencollective.com/nservicebusextensions/order/6972). The company avatar will show up here with a link to your website. The avatar will also be added to all GitHub repositories under this organization.


### Patrons

Thanks to all the backing developers! Support this project by [becoming a patron](https://opencollective.com/nservicebusextensions/order/6976).

<img src="https://opencollective.com/nservicebusextensions/tiers/patron.svg?width=890&avatarHeight=60&button=false">

<!--- EndOpenCollectiveBackers -->

<a href="#" id="endofbacking"></a>


## Usage

<!-- snippet: SerilogInCode -->
<a id='snippet-serilogincode'/></a>
```cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .CreateLogger();

LogManager.Use<SerilogFactory>();
```
<sup>[snippet source](/src/Tests/Snippets/Usage.cs#L10-L18) / [anchor](#snippet-serilogincode)</sup>
<!-- endsnippet -->


## Filtering

NServiceBus can write a significant amount of information to the log. To limit this information use the filtering features of the underlying logging framework.

For example to limit log output to a specific namespace.

Here is a code configuration example for adding a [Filter](https://github.com/serilog/serilog/wiki/Configuration-Basics#filters).

<!-- snippet: SerilogFiltering -->
<a id='snippet-serilogfiltering'/></a>
```cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path:"log.txt",
        restrictedToMinimumLevel: LogEventLevel.Debug
    )
    .Filter.ByIncludingOnly(
        inclusionPredicate: Matching.FromSource("MyNamespace"))
    .CreateLogger();

LogManager.Use<SerilogFactory>();
```
<sup>[snippet source](/src/Tests/Snippets/Filtering.cs#L11-L24) / [anchor](#snippet-serilogfiltering)</sup>
<!-- endsnippet -->


## Tracing

Writing diagnostic log entries to [Serilog](https://serilog.net/). Plugs into the low level [pipeline](https://docs.particular.net/nservicebus/pipeline) to give more detailed diagnostics.

When using Serilog for tracing, it is optional to use Serilog as the main NServiceBus logger. i.e. there is no need to include `LogManager.Use<SerilogFactory>();`.


### Create an instance of a Serilog logger

<!-- snippet: SerilogTracingLogger -->
<a id='snippet-serilogtracinglogger'/></a>
```cs
var tracingLog = new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .MinimumLevel.Information()
    .CreateLogger();
```
<sup>[snippet source](/src/Tests/Snippets/TracingUsage.cs#L8-L15) / [anchor](#snippet-serilogtracinglogger)</sup>
<!-- endsnippet -->


### Configure the tracing feature to use that logger

<!-- snippet: SerilogTracingPassLoggerToFeature -->
<a id='snippet-serilogtracingpassloggertofeature'/></a>
```cs
var serilogTracing = endpointConfiguration.EnableSerilogTracing(tracingLog);
serilogTracing.EnableMessageTracing();
```
<sup>[snippet source](/src/Tests/Snippets/TracingUsage.cs#L19-L24) / [anchor](#snippet-serilogtracingpassloggertofeature)</sup>
<!-- endsnippet -->


### Contextual logger

Serilog tracing injects a contextual `Serilog.Ilogger` into the NServiceBus pipeline.

NOTE: Saga and message tracing will use the current contextual logger.

There are several layers of enrichment based on the pipeline phase.


#### Endpoint enrichment

All loggers for an endpoint will have the the property `ProcessingEndpoint` added that contains the current [endpoint name](https://docs.particular.net/nservicebus/endpoints/specify-endpoint-name).


#### Incoming message enrichment

When a message is received, the following enrichment properties are added:

 * [SourceContext](https://github.com/serilog/serilog/wiki/Writing-Log-Events#source-contexts) will be the message type [FullName](https://docs.microsoft.com/de-de/dotnet/api/system.type.fullname) extracted from the [EnclosedMessageTypes header](https://docs.particular.net/nservicebus/messaging/headers#serialization-headers-nservicebus-enclosedmessagetypes). `UnknownMessageType` will be used if no header exists. The same value will be added to a property named `MessageType`.
 * `MessageId` will be the value of the [MessageId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-messageid).
 * `CorrelationId` will be the value of the [CorrelationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-correlationid) if it exists.
 * `ConversationId` will be the value of the [ConversationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-conversationid) if it exists.


#### Handler enrichment

When a handler is invoked, a new logger is forked from the above enriched physical logger with a new enriched property named `Handler` that contains the the [FullName](https://docs.microsoft.com/de-de/dotnet/api/system.type.fullname) of the current handler.


#### Outgoing message enrichment

When a message is sent, the same properties as described in "Incoming message enrichment" will be added to the outgoing pipeline. Note that if a handler sends a message, the logger injected into the outgoing pipeline will be forked from the logger instance as described in "Handler enrichment". As such it will contain a property `Handler` for the handler that sent the message.


#### Accessing the logger

The contextual logger instance can be accessed from anywhere in the pipeline via `SerilogTracingExtensions.Logger(this IPipelineContext context)`.

<!-- snippet: ContextualLoggerUsage -->
<a id='snippet-contextualloggerusage'/></a>
```cs
public class SimpleHandler :
    IHandleMessages<TheMessage>
{
    public Task Handle(TheMessage message, IMessageHandlerContext context)
    {
        var logger = context.Logger();
        logger.Information("Hello from {@Handler}.");
        return Task.CompletedTask;
    }
}
```
<sup>[snippet source](/src/Tests/Snippets/ContextualLoggerUsage.cs#L4-L16) / [anchor](#snippet-contextualloggerusage)</sup>
<!-- endsnippet -->


### Exception enrichment

When an exception occurs in the message processing pipeline, the current pipeline state is added to the exception. When that exception is logged that state can be add to the log entry.

The type added to the exception data is `ExceptionLogState`. It contains the following data:

 * `ProcessingEndpoint` will be the current [endpoint name](https://docs.particular.net/nservicebus/endpoints/specify-endpoint-name).
 * `MessageId` will be the value of the [MessageId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-messageid).
 * `Headers` will be the value of the [Message headers](https://docs.particular.net/nservicebus/messaging/headers).
 * `MessageType` will be the message type [FullName](https://docs.microsoft.com/de-de/dotnet/api/system.type.fullname) extracted from the [EnclosedMessageTypes header](https://docs.particular.net/nservicebus/messaging/headers#serialization-headers-nservicebus-enclosedmessagetypes). `UnknownMessageType` will be used if no header exists.
 * `CorrelationId` will be the value of the [CorrelationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-correlationid) if it exists.
 * `ConversationId` will be the value of the [ConversationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-conversationid) if it exists.
 * `HandlerType` will be type name for the current handler if it exists.
 * `Message` will be the value of current logical message if it exists.

The instance of `ExceptionLogState` can be accessed using the following.

<!-- snippet: ExceptionLogState -->
<a id='snippet-exceptionlogstate'/></a>
```cs
if (ExceptionLogState.TryReadFromException(exception, out var logState))
{
    var endpoint = logState.ProcessingEndpoint;
    var incomingMessageId = logState.IncomingMessageId;
    var incomingMessageType = logState.IncomingMessageType;
    var correlationId = logState.CorrelationId;
    var conversationId = logState.ConversationId;
    var handlerType = logState.HandlerType;
    var incomingHeaders = logState.IncomingHeaders;
    var incomingMessage = logState.IncomingMessage;
}
```
<sup>[snippet source](/src/Tests/Snippets/Usage.cs#L37-L51) / [anchor](#snippet-exceptionlogstate)</sup>
<!-- endsnippet -->

When routing the NServiceBus log event with `LogManager.Use<SerilogFactory>();`, the above properties will be promoted to the log event.


### Saga tracing

<!-- snippet: EnableSagaTracing -->
<a id='snippet-enablesagatracing'/></a>
```cs
var serilogTracing = endpointConfiguration.EnableSerilogTracing(logger);
serilogTracing.EnableSagaTracing();
```
<sup>[snippet source](/src/Tests/Snippets/TracingUsage.cs#L29-L34) / [anchor](#snippet-enablesagatracing)</sup>
<!-- endsnippet -->


### Message tracing

Both incoming and outgoing messages will be logged at the [Information level](https://github.com/serilog/serilog/wiki/Writing-Log-Events#the-role-of-the-information-level). The current message will be included in a property named `Message`. For outgoing messages any unicast routes will be included in a property named `UnicastRoutes`.

<!-- snippet: EnableMessageTracing -->
<a id='snippet-enablemessagetracing'/></a>
```cs
var serilogTracing = endpointConfiguration.EnableSerilogTracing(logger);
serilogTracing.EnableMessageTracing();
```
<sup>[snippet source](/src/Tests/Snippets/TracingUsage.cs#L39-L44) / [anchor](#snippet-enablemessagetracing)</sup>
<!-- endsnippet -->


## Logging to Seq

To log to [Seq](https://getseq.net/):

<!-- snippet: SerilogTracingSeq -->
<a id='snippet-serilogtracingseq'/></a>
```cs
var tracingLog = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .MinimumLevel.Information()
    .CreateLogger();
```
<sup>[snippet source](/src/Tests/Snippets/TracingUsage.cs#L49-L56) / [anchor](#snippet-serilogtracingseq)</sup>
<!-- endsnippet -->


## Sample

The sample illustrates how to customize logging by configuring Serilog targets and rules.


### Configure Serilog

<!-- snippet: ConfigureSerilog -->
<a id='snippet-configureserilog'/></a>
```cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
```
<sup>[snippet source](/src/Sample/Program.cs#L13-L17) / [anchor](#snippet-configureserilog)</sup>
<a id='snippet-configureserilog-1'/></a>
```cs
var tracingLog = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .MinimumLevel.Information()
    .CreateLogger();
var serilogFactory = LogManager.Use<SerilogFactory>();
serilogFactory.WithLogger(tracingLog);
```
<sup>[snippet source](/src/SeqSample/Program.cs#L13-L20) / [anchor](#snippet-configureserilog-1)</sup>
<!-- endsnippet -->


### Pass the configuration to NServiceBus

<!-- snippet: UseConfig -->
<a id='snippet-useconfig'/></a>
```cs
LogManager.Use<SerilogFactory>();

var configuration = new EndpointConfiguration("SerilogSample");
```
<sup>[snippet source](/src/Sample/Program.cs#L19-L24) / [anchor](#snippet-useconfig)</sup>
<a id='snippet-useconfig-1'/></a>
```cs
var configuration = new EndpointConfiguration("SeqSample");
var serilogTracing = configuration.EnableSerilogTracing(tracingLog);
serilogTracing.EnableSagaTracing();
serilogTracing.EnableMessageTracing();
```
<sup>[snippet source](/src/SeqSample/Program.cs#L22-L29) / [anchor](#snippet-useconfig-1)</sup>
<!-- endsnippet -->


### Ensure logging is flushed on shutdown

<!-- snippet: Cleanup -->
<a id='snippet-cleanup'/></a>
```cs
await endpoint.Stop();
Log.CloseAndFlush();
```
<sup>[snippet source](/src/Sample/Program.cs#L34-L37) / [anchor](#snippet-cleanup)</sup>
<a id='snippet-cleanup-1'/></a>
```cs
await endpoint.Stop();
Log.CloseAndFlush();
```
<sup>[snippet source](/src/SeqSample/Program.cs#L45-L48) / [anchor](#snippet-cleanup-1)</sup>
<!-- endsnippet -->


## Seq Sample

Illustrates customizing [Serilog](https://serilog.net/) usage to log to [Seq](https://getseq.net/).


### Prerequisites

An instance of [Seq](https://getseq.net/) running one `http://localhost:5341`.


### Configure Serilog

<!-- snippet: ConfigureSerilog -->
<a id='snippet-configureserilog'/></a>
```cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
```
<sup>[snippet source](/src/Sample/Program.cs#L13-L17) / [anchor](#snippet-configureserilog)</sup>
<a id='snippet-configureserilog-1'/></a>
```cs
var tracingLog = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .MinimumLevel.Information()
    .CreateLogger();
var serilogFactory = LogManager.Use<SerilogFactory>();
serilogFactory.WithLogger(tracingLog);
```
<sup>[snippet source](/src/SeqSample/Program.cs#L13-L20) / [anchor](#snippet-configureserilog-1)</sup>
<!-- endsnippet -->


### Pass that configuration to NServiceBus

<!-- snippet: UseConfig -->
<a id='snippet-useconfig'/></a>
```cs
LogManager.Use<SerilogFactory>();

var configuration = new EndpointConfiguration("SerilogSample");
```
<sup>[snippet source](/src/Sample/Program.cs#L19-L24) / [anchor](#snippet-useconfig)</sup>
<a id='snippet-useconfig-1'/></a>
```cs
var configuration = new EndpointConfiguration("SeqSample");
var serilogTracing = configuration.EnableSerilogTracing(tracingLog);
serilogTracing.EnableSagaTracing();
serilogTracing.EnableMessageTracing();
```
<sup>[snippet source](/src/SeqSample/Program.cs#L22-L29) / [anchor](#snippet-useconfig-1)</sup>
<!-- endsnippet -->


### Ensure logging is flushed on shutdown

<!-- snippet: Cleanup -->
<a id='snippet-cleanup'/></a>
```cs
await endpoint.Stop();
Log.CloseAndFlush();
```
<sup>[snippet source](/src/Sample/Program.cs#L34-L37) / [anchor](#snippet-cleanup)</sup>
<a id='snippet-cleanup-1'/></a>
```cs
await endpoint.Stop();
Log.CloseAndFlush();
```
<sup>[snippet source](/src/SeqSample/Program.cs#L45-L48) / [anchor](#snippet-cleanup-1)</sup>
<!-- endsnippet -->


## Release Notes

See [closed milestones](../../milestones?state=closed).


## Icon

[Brain](https://thenounproject.com/noun/brain/#icon-No10411) designed by [Rémy Médard](https://thenounproject.com/catalarem) from [The Noun Project](https://thenounproject.com).
