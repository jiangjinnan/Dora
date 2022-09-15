namespace Dora.OpenTelemetry
{
    public class OpenTelemetryDefaults
    {
        public class ResourceAttributes 
        {
            public const string ServiceName = "service.name";
            public const string ServiceNamespace = "service.namespace";
            public const string ServiceInstanceId = "service.instance.id";
            public const string ServiceVersion = "service.version";

            public const string SdkName = "telemetry.sdk.name";
            public const string SdkVersion = "telemetry.sdk.version";
            public const string SdkLanguage = "telemetry.sdk.language";

            public const string AutoInstrumentationAgent = "telemetry.auto.version";
        }

        public class SpanAttributeNames 
        {
            public const string StatusCode = "otel.status_code";
            public const string StatusDescription= "otel.status_description";
            public const string DatabaseStatementType = "db.statement_type";
            public const string LibraryName = "otel.library.name";
            public const string LibraryVersion = "otel.library.version";

            public const string NetTransport = "net.transport";
            public const string NetPeerIp = "net.peer.ip";
            public const string NetPeerPort = "net.peer.port";
            public const string NetPeerName = "net.peer.name";
            public const string NetHostIp = "net.host.ip";
            public const string NetHostPort = "net.host.port";
            public const string NetHostName = "net.host.name";

            public const string EnduserId = "enduser.id";
            public const string EnduserRole = "enduser.role";
            public const string EnduserScope = "enduser.scope";

            public const string PeerService = "peer.service";

            public const string HttpMethod = "http.method";
            public const string HttpUrl = "http.url";
            public const string HttpTarget = "http.target";
            public const string HttpHost = "http.host";
            public const string HttpScheme = "http.scheme";
            public const string HttpStatusCode = "http.status_code";
            public const string HttpStatusText = "http.status_text";
            public const string HttpFlavor = "http.flavor";
            public const string HttpServerName = "http.server_name";
            public const string HttpRoute = "http.route";
            public const string HttpClientIP = "http.client_ip";
            public const string HttpUserAgent = "http.user_agent";
            public const string HttpRequestContentLength = "http.request_content_length";
            public const string HttpRequestContentLengthUncompressed = "http.request_content_length_uncompressed";
            public const string HttpResponseContentLength = "http.response_content_length";
            public const string HttpResponseContentLengthUncompressed = "http.response_content_length_uncompressed";

            public const string DbSystem = "db.system";
            public const string DbConnectionString = "db.connection_string";
            public const string DbUser = "db.user";
            public const string DbMsSqlInstanceName = "db.mssql.instance_name";
            public const string DbJdbcDriverClassName = "db.jdbc.driver_classname";
            public const string DbName = "db.name";
            public const string DbStatement = "db.statement";
            public const string DbOperation = "db.operation";
            public const string DbInstance = "db.instance";
            public const string DbUrl = "db.url";
            public const string DbCassandraKeyspace = "db.cassandra.keyspace";
            public const string DbHBaseNamespace = "db.hbase.namespace";
            public const string DbRedisDatabaseIndex = "db.redis.database_index";
            public const string DbMongoDbCollection = "db.mongodb.collection";

            public const string RpcSystem = "rpc.system";
            public const string RpcService = "rpc.service";
            public const string RpcMethod = "rpc.method";
            public const string RpcGrpcStatusCode = "rpc.grpc.status_code";

            public const string MessageType = "message.type";
            public const string MessageId = "message.id";
            public const string MessageCompressedSize = "message.compressed_size";
            public const string MessageUncompressedSize = "message.uncompressed_size";

            public const string FaasTrigger = "faas.trigger";
            public const string FaasExecution = "faas.execution";
            public const string FaasDocumentCollection = "faas.document.collection";
            public const string FaasDocumentOperation = "faas.document.operation";
            public const string FaasDocumentTime = "faas.document.time";
            public const string FaasDocumentName = "faas.document.name";
            public const string FaasTime = "faas.time";
            public const string FaasCron = "faas.cron";

            public const string MessagingSystem = "messaging.system";
            public const string MessagingDestination = "messaging.destination";
            public const string MessagingDestinationKind = "messaging.destination_kind";
            public const string MessagingTempDestination = "messaging.temp_destination";
            public const string MessagingProtocol = "messaging.protocol";
            public const string MessagingProtocolVersion = "messaging.protocol_version";
            public const string MessagingUrl = "messaging.url";
            public const string MessagingMessageId = "messaging.message_id";
            public const string MessagingConversationId = "messaging.conversation_id";
            public const string MessagingPayloadSize = "messaging.message_payload_size_bytes";
            public const string MessagingPayloadCompressedSize = "messaging.message_payload_compressed_size_bytes";
            public const string MessagingOperation = "messaging.operation";

            public const string ExceptionEventName = "exception";
            public const string ExceptionType = "exception.type";
            public const string ExceptionMessage = "exception.message";
            public const string ExceptionStacktrace = "exception.stacktrace";


            public const string PeerHostName = "peer.hostname";
            public const string PeerAddress = "peer.address";
        }
    }
}
