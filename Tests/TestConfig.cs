using Microsoft.Extensions.Configuration;

namespace Hotel.Tests;

public static class TestConfig
{
    private static IConfiguration? _configuration;
    
    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
                
                _configuration = builder.Build();
            }
            return _configuration;
        }
    }
    
    public static string TestConnectionString => 
        Configuration.GetConnectionString("DefaultConnection") ?? 
        throw new InvalidOperationException("Test connection string not found");
    
    public static string TestDatabaseName => "HotelDb_Test";
    
    public static string SeedFilePath
    {
        get
        {
            var currentDir = Directory.GetCurrentDirectory();
            var projectRoot = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName;
            
            if (projectRoot == null)
            {
                throw new DirectoryNotFoundException("Could not find project root directory");
            }
            
            return Path.Combine(projectRoot, "SeedData", "small_seed.sql");
        }
    }
}