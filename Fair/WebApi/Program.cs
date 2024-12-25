using Explorer.Api.Configurations;
using Explorer.WebApi.Configurations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().
	AddJsonOptions(x =>
	{
		x.JsonSerializerOptions.Converters.Add(new BigIntegerJsonConverter());
		x.JsonSerializerOptions.Converters.Add(new BytesToHexJsonConverter());
		x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddCorsPolicy(builder.Configuration);

// Configure services.
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Register app services.
builder.Services.AddMemoryCache();
builder.Services.RegisterServices(builder.Configuration);
builder.Services.RegisterValidators();
builder.Services.AddAutoMapper();
builder.Services.AddBlockchainDatabaseStore(builder.Configuration);

var app = builder.Build();

#if DEBUG
app.Services.AssertAutoMapperConfiguration();
#endif

// Configure the HTTP request pipeline.
// Learn more at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
app.UseExceptionTransform();

if(app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
