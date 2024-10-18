using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

namespace Saitynai.Backend;

public class Program
{
	public static async Task Main(string[] args) =>
		await new Startup(args).StartAsync();
}

public class Startup
{
	private readonly string[] _args;

	public Startup(string[] args)
	{
		_args = args;
	}

	public async Task StartAsync()
	{
		var builder = WebApplication.CreateBuilder(_args);
		ConfigureServices(builder.Services, builder.Configuration);

		var app = builder.Build();
		ConfigureMiddlewares(app);

		using var scope = app.Services.CreateScope();
		var services = scope.ServiceProvider;

		var dbContext = services.GetRequiredService<SaitynaiDbContext>();
		dbContext.Database.Migrate();

		await app.RunAsync();
	}

	private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddTransient<ICrudService, CrudService>();
		services.AddTransient<IUserService, UserService>();
		services.AddTransient<INotesService, NotesService>();
		services.AddTransient<ITagsService, TagsService>();

		services.AddEndpointsApiExplorer();
		services.AddControllers();
		services.AddSwaggerGen(x =>
		{
			x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "Input your Bearer token to access this API"
			});

			x.AddSecurityRequirement(new OpenApiSecurityRequirement 
			{
				{
					new OpenApiSecurityScheme 
					{
						Reference = new OpenApiReference 
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});
		});

		services.AddAuthentication(x =>
		{
			x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(x =>
		{
			x.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
			};
		});

		services.AddAuthorization();

		services.AddDbContext<SaitynaiDbContext>();
	}

	private static void ConfigureMiddlewares(WebApplication app)
	{
		app.UseStaticFiles();

		app.UseSwagger();
		app.UseSwaggerUI(x =>
		{
			x.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			x.InjectStylesheet("/assets/swagger-dark-theme.css");
			x.RoutePrefix = string.Empty;
		});

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapControllers();
	}
}
