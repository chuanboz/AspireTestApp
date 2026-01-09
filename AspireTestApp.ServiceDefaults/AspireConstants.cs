namespace AspireTestApp.ServiceDefaults;

/// <summary>
/// Shared constants for the Aspire application including resource names,
/// database configuration, and endpoint paths.
/// </summary>
public static class AspireConstants
{
    /// <summary>
    /// Resource names used in the Aspire AppHost configuration.
    /// </summary>
    public static class Resources
    {
        public const string CosmosDb = "cosmosdb";
        public const string CosmosDatabase = "cosmosdb-database";
        public const string CosmosCountersContainer = "cosmosdb-counters";
        public const string CosmosWeatherContainer = "cosmosdb-weather";
        public const string CosmosLeasesContainer = "cosmosdb-leases";
        public const string ApiService = "apiservice";
        public const string WebFrontend = "webfrontend";
        public const string CounterFunction = "counter-function";
    }

    /// <summary>
    /// Cosmos DB configuration values.
    /// </summary>
    public static class CosmosDb
    {
        public const string DatabaseName = "AspireTestApp";
        public const string CounterContainerName = "counters";
        public const string WeatherContainerName = "weather";
        public const string LeaseContainerName = "leases";
        public const string CounterPartitionKeyPath = "/name";
        public const string WeatherPartitionKeyPath = "/PartitionKey";
        public const string LeasePartitionKeyPath = "/id";
        
        /// <summary>
        /// Connection string configuration key name for Cosmos DB.
        /// Used by Azure Functions to lookup the connection string from configuration.
        /// </summary>
        public const string ConnectionStringKey = "CosmosDb";
    }

    /// <summary>
    /// Health check endpoint paths.
    /// </summary>
    public static class HealthEndpoints
    {
        public const string Health = "/health";
        public const string AdminHealth = "/admin/health";
        public const string Alive = "/alive";
    }

    /// <summary>
    /// Azure Functions configuration.
    /// </summary>
    public static class Functions
    {
        public const int DefaultHttpPort = 7071;
        public const string HttpEndpointName = "http";
    }

    /// <summary>
    /// Command-line switches for the distributed application host.
    /// </summary>
    public static class Switches
    {
        /// <summary>
        /// When true, the AppHost runs Cosmos DB with the vNext (preview) emulator.
        /// </summary>
        public const string UseCosmosVNextEmulator = "UseCosmosVNextEmulator";
    }
}

