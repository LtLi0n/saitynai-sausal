using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.DataAccess;
using System.Reflection;

namespace Saitynai.Backend.Services;

public class SaitynaiDbContext : DbContext
{
	private readonly IConfiguration _configuration;

	public SaitynaiDbContext(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connString = _configuration.GetConnectionString("Postgres");
		
		optionsBuilder
			.UseNpgsql(
				connString, 
				x => 
				{ 
					x.UseVector();
					x.MigrationsHistoryTable("migrations", "saitynai");
				})
			.UseSnakeCaseNamingConvention();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresExtension("vector");
		modelBuilder.HasDefaultSchema("saitynai");

		var assembly = Assembly.GetAssembly(typeof(DbEntityAttribute));
		var types = assembly.GetTypes();
		
		var dbModelTypes = types
			.Where(x => x.GetCustomAttribute<DbEntityAttribute>() != null)
			.ToList();
		foreach(var dbModelType in dbModelTypes)
		{
			var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod;
			dbModelType.InvokeMember("CreateModel", flags, null, null, [modelBuilder]);
		}
	}
}
