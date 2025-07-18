using Microsoft.EntityFrameworkCore;
using Hotel.Data;
using Npgsql;

namespace Hotel.Tests;

public static class TestDatabaseHelper
{
    public static async Task RunMigrationsAsync()
    {
        var connectionString = TestConfig.TestConnectionString;
        
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new HotelDbContext(options);
        
        // Apply all pending migrations
        await context.Database.MigrateAsync();
    }
    
    public static async Task SeedTestDatabaseAsync()
    {
        var connectionString = TestConfig.TestConnectionString;
        
        if (!File.Exists(TestConfig.SeedFilePath))
        {
            throw new FileNotFoundException($"Seed file not found at: {TestConfig.SeedFilePath}");
        }
        
        var seedSql = await File.ReadAllTextAsync(TestConfig.SeedFilePath);
        
        if (string.IsNullOrWhiteSpace(seedSql))
        {
            throw new InvalidOperationException("SQL file is empty");
        }
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Clear existing data first
        await ClearDatabaseAsync(connection);
        
        // Execute the seed SQL
        await using var command = new NpgsqlCommand(seedSql, connection);
        await command.ExecuteNonQueryAsync();
    }
    
    private static async Task ClearDatabaseAsync(NpgsqlConnection connection)
    {
        var clearSql = @"
            TRUNCATE TABLE ""BookingRooms"" RESTART IDENTITY CASCADE;
            TRUNCATE TABLE ""Bookings"" RESTART IDENTITY CASCADE;
            TRUNCATE TABLE ""Rooms"" RESTART IDENTITY CASCADE;
            TRUNCATE TABLE ""Hotels"" RESTART IDENTITY CASCADE;
            TRUNCATE TABLE ""RoomTypes"" RESTART IDENTITY CASCADE;
        ";
        
        await using var command = new NpgsqlCommand(clearSql, connection);
        await command.ExecuteNonQueryAsync();
    }
    
    public static async Task CleanupTestDataAsync()
    {
        var connectionString = TestConfig.TestConnectionString;
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Only clean test bookings, keep seed data
        var cleanupSql = @"
            DELETE FROM ""Bookings"" 
            WHERE ""GuestName"" LIKE '%Test%' 
            OR ""GuestName"" LIKE '%John%' 
            OR ""GuestName"" LIKE '%First%' 
            OR ""GuestName"" LIKE '%Second%'
            OR ""GuestName"" LIKE '%Large Group%';
        ";
        
        await using var command = new NpgsqlCommand(cleanupSql, connection);
        await command.ExecuteNonQueryAsync();
    }
}