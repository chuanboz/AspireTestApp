using AspireTestApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Cosmos DB client
builder.AddAzureCosmosClient(AspireConstants.Resources.CosmosDb);

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
