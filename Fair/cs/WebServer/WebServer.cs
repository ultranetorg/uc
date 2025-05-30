using System.Text.Json.Serialization;
using Uccs.Web.Configurations;
using Uccs.Web.Filters;

namespace Uccs.Fair;

public class WebServer
{
	FairNode		Node;
	WebApplication	WebApplication;

	// SEE: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-9.0#middleware-order
	public WebServer(FairNode node, string[] args)
	{
		Node = node;

		var t = node.CreateThread(() =>	{
											var o = new WebApplicationOptions
											{
														ApplicationName = GetType().Assembly.GetName().Name,
														ContentRootPath = Path.GetDirectoryName(GetType().Assembly.Location),
														//WebRootPath = $"{Path.GetDirectoryName(GetType().Assembly.Location)}/WebUI",
														EnvironmentName = Node.Net.Zone > Zone._Public ? Environments.Production : Environments.Development
											};

											var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(o);

											// Add services to the container.
											builder.Services.RegisterServices(node);

#if DEBUG
											builder.Services.AddCorsPolicy(new[] { "*" });
#endif

											builder.Services
												.AddControllers(options =>
												{
													options.Filters.Add<HttpResponseExceptionFilter>();
												})
												.AddJsonOptions(options =>
												{
													options.JsonSerializerOptions.TypeInfoResolver = new PolymorphicTypeResolver();

													options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
												});

											WebApplication = builder.Build();

											// Configure the HTTP request pipeline.

											//app.UseHttpsRedirection();

											WebApplication.UseCors();

											WebApplication.UseAuthorization();

											WebApplication.MapControllers();

											WebApplication.Run(node.Settings.WebServerListenUrl);

										});
		t.Name = $"{node.Name} {GetType().Name}";
		t.Start();
	}

	public void Stop()
	{
		WebApplication.StopAsync();
	}
}
