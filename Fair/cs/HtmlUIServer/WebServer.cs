using Uccs.Web.Configurations;

namespace Uccs.Fair;

public class WebServer
{
	FairNode Node;

	// SEE: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-9.0#middleware-order
	public WebServer(FairNode node, string[] args, string url)
	{
		Node = node;

		var t = node.CreateThread(() =>	{
											var builder = WebApplication.CreateBuilder(args);

											// Add services to the container.
											builder.Services.AddCorsPolicy(builder.Configuration);

											builder.Services.AddControllers();

											var app = builder.Build();

											// Configure the HTTP request pipeline.

											app.UseHttpsRedirection();

											app.UseCors();

											app.UseAuthorization();

											app.MapControllers();

											app.Run(url);

										});
		t.Name = $"{node.Name} {GetType().Name}";
		t.Start();
	}
}
