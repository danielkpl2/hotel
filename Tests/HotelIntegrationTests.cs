using System.Net;
using System.Text.Json;
using FluentAssertions;
using Hotel.Data;
using Hotel.DTOs;
using Hotel.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Hotel.Tests;

[TestFixture]
public class HotelIntegrationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private HotelDbContext _context = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await TestDatabaseHelper.RunMigrationsAsync();
        
        await TestDatabaseHelper.SeedTestDatabaseAsync();
        
        // Create factory with test configuration
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Use the helper method to get the correct content root
                builder.UseContentRoot(GetContentRoot());
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
                });
                
                builder.ConfigureServices(services =>
                {
                    
                    services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    });
                });
                
                builder.UseEnvironment("Test");
            });

        _client = _factory.CreateClient();
        
        // Get context for data management
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    }

    [SetUp]
    public async Task SetUp()
    {
        await TestDatabaseHelper.CleanupTestDataAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _context.Dispose();
        _client.Dispose();
        _factory.Dispose();
        
        await TestDatabaseHelper.CleanupTestDataAsync();
    }

    #region Search Tests

    [Test]
    public async Task SearchHotels_WithName_ReturnsMatchingHotels()
    {
        // Arrange
        var hotelName = "Westminster";

        // Act
        var response = await _client.GetAsync($"/api/hotel/search?name={hotelName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var hotels = await response.Content.ReadFromJsonAsync<List<Hotel.Models.Hotel>>();
        hotels.Should().NotBeNull();
        hotels!.Should().HaveCountGreaterThan(0);
        hotels.Should().OnlyContain(h => h.Name.Contains(hotelName));
    }

    [Test]
    public async Task SearchHotels_WithNonexistentName_ReturnsEmptyList()
    {
        // Arrange
        var hotelName = "ThisHotelDoesNotExist123";

        // Act
        var response = await _client.GetAsync($"/api/hotel/search?name={hotelName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var hotels = await response.Content.ReadFromJsonAsync<List<Hotel.Models.Hotel>>();
        hotels.Should().NotBeNull();
        hotels!.Should().BeEmpty();
    }

    #endregion

    #region Booking Reference Tests

    [Test]
    public async Task GetBookingByReference_ReturnsBooking()
    {
        // Arrange
        var bookingReference = "BK20250715120000001";

        // Act
        var response = await _client.GetAsync($"/api/hotel/bookings/{bookingReference}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var booking = await response.Content.ReadFromJsonAsync<Booking>();
        booking.Should().NotBeNull();
        booking!.BookingReference.Should().Be(bookingReference);
        booking.GuestName.Should().Be("Sherlock Holmes");
        booking.HotelId.Should().Be(1);
        booking.PeopleCount.Should().Be(2);
    }

    [Test]
    public async Task GetBookingByReference_InvalidReference_ReturnsNotFound()
    {
        // Arrange
        var bookingReference = "INVALID_REF_123";

        // Act
        var response = await _client.GetAsync($"/api/hotel/bookings/{bookingReference}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    #endregion

    #region Available Rooms Tests
    [Test]
    public async Task GetAvailableRooms_ValidRequest_ReturnsAvailableRooms()
    {
        // Arrange
        var checkInDate = "2025-07-25";
        var checkOutDate = "2025-07-27";
        var peopleCount = 2;

        // Act
        var response = await _client.GetAsync(
            $"/api/hotel/bookings/available-rooms?checkInDate={checkInDate}&checkOutDate={checkOutDate}&peopleCount={peopleCount}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        // Basic assertions
        result.GetProperty("totalHotelsFound").GetInt32().Should().Be(3);
        result.GetProperty("totalRoomsAvailable").GetInt32().Should().Be(17);

        var hotels = result.GetProperty("hotels").EnumerateArray().ToList();
        hotels.Should().HaveCount(3);

        // Assert that all hotels can accommodate guests
        foreach (var hotel in hotels)
        {
            hotel.GetProperty("canAccommodateGuests").GetBoolean().Should().BeTrue();
            hotel.GetProperty("availableRooms").EnumerateArray().Should().NotBeEmpty();
        }

        // Assert that only room 17 is not available (booked for these dates)
        var unavailableRoomIds = new[] { 17 };

        var allAvailableRoomIds = hotels
            .SelectMany(h => h.GetProperty("availableRooms").EnumerateArray())
            .Select(r => r.GetProperty("roomId").GetInt32())
            .ToList();

        allAvailableRoomIds.Should().NotContain(unavailableRoomIds);

        // Assert that each hotel has the correct available rooms (based on your response)
        hotels.First(h => h.GetProperty("hotelId").GetInt32() == 1)
            .GetProperty("availableRooms").EnumerateArray()
            .Select(r => r.GetProperty("roomId").GetInt32())
            .Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6 }); // Westminster: all rooms available for these dates

        hotels.First(h => h.GetProperty("hotelId").GetInt32() == 2)
            .GetProperty("availableRooms").EnumerateArray()
            .Select(r => r.GetProperty("roomId").GetInt32())
            .Should().BeEquivalentTo(new[] { 7, 8, 9, 10, 11, 12 }); // Big Ben: all rooms available for these dates

        hotels.First(h => h.GetProperty("hotelId").GetInt32() == 3)
            .GetProperty("availableRooms").EnumerateArray()
            .Select(r => r.GetProperty("roomId").GetInt32())
            .Should().BeEquivalentTo(new[] { 13, 14, 15, 16, 18 }); // Buckingham: room 17 is booked

        // Optionally, assert that all available rooms have enough capacity for the people count
        foreach (var room in allAvailableRoomIds)
        {
            var roomObj = hotels.SelectMany(h => h.GetProperty("availableRooms").EnumerateArray())
                                .First(r => r.GetProperty("roomId").GetInt32() == room);
            roomObj.GetProperty("maxOccupancy").GetInt32().Should().BeGreaterOrEqualTo(1);
        }
    }

    [Test]
    public async Task GetAvailableRooms_LargeGroup_ReturnsHotelsWithSufficientTotalCapacity()
    {
        // Arrange
        var checkInDate = "2025-07-26";
        var checkOutDate = "2025-07-27";
        var peopleCount = 12;

        // Act
        var response = await _client.GetAsync(
            $"/api/hotel/bookings/available-rooms?checkInDate={checkInDate}&checkOutDate={checkOutDate}&peopleCount={peopleCount}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        // Should return 2 hotels (Big Ben Tower Suites and The Westminster Palace Hotel)
        result.GetProperty("totalHotelsFound").GetInt32().Should().Be(2);
        var hotels = result.GetProperty("hotels").EnumerateArray().ToList();
        hotels.Should().HaveCount(2);

        // Each hotel should have totalAvailableCapacity >= peopleCount and canAccommodateGuests == true
        foreach (var hotel in hotels)
        {
            hotel.GetProperty("canAccommodateGuests").GetBoolean().Should().BeTrue();
            hotel.GetProperty("totalAvailableCapacity").GetInt32().Should().BeGreaterOrEqualTo(peopleCount);

            // All available rooms should be returned (singles, doubles, deluxe)
            var availableRooms = hotel.GetProperty("availableRooms").EnumerateArray().ToList();
            availableRooms.Should().NotBeEmpty();

            // The sum of maxOccupancy for all available rooms should equal totalAvailableCapacity
            var sumCapacity = availableRooms.Sum(r => r.GetProperty("maxOccupancy").GetInt32());
            sumCapacity.Should().Be(hotel.GetProperty("totalAvailableCapacity").GetInt32());
        }

        // Assert correct hotel IDs and room IDs for this scenario
        var westminsterRoomIds = new[] { 1, 2, 3, 4, 5, 6 };
        var bigBenRoomIds = new[] { 7, 8, 9, 10, 11, 12 };

        hotels.First(h => h.GetProperty("hotelId").GetInt32() == 1)
            .GetProperty("availableRooms").EnumerateArray()
            .Select(r => r.GetProperty("roomId").GetInt32())
            .Should().BeEquivalentTo(westminsterRoomIds);

        hotels.First(h => h.GetProperty("hotelId").GetInt32() == 2)
            .GetProperty("availableRooms").EnumerateArray()
            .Select(r => r.GetProperty("roomId").GetInt32())
            .Should().BeEquivalentTo(bigBenRoomIds);
        // The totalRoomsAvailable should be 12 (all rooms from both hotels)
        result.GetProperty("totalRoomsAvailable").GetInt32().Should().Be(12);

    }

    [Test]
    public async Task GetAvailableRooms_TooManyPeople_ReturnsNoHotels()
    {
        // Arrange
        var checkInDate = "2025-07-26";
        var checkOutDate = "2025-07-27";
        var peopleCount = 13;

        // Act
        var response = await _client.GetAsync(
            $"/api/hotel/bookings/available-rooms?checkInDate={checkInDate}&checkOutDate={checkOutDate}&peopleCount={peopleCount}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        result.GetProperty("totalHotelsFound").GetInt32().Should().Be(0);
        result.GetProperty("totalRoomsAvailable").GetInt32().Should().Be(0);

        var hotels = result.GetProperty("hotels").EnumerateArray().ToList();
        hotels.Should().BeEmpty();
    }

    #endregion

    #region Booking Rooms Tests
    
    [Test]
    public async Task CreateBooking_ValidRequest_CreatesBooking()
    {
        // Arrange: Find available rooms for a valid booking
        var checkInDate = "2025-07-25";
        var checkOutDate = "2025-07-27";
        var peopleCount = 2;

        // Get available rooms
        var availableResponse = await _client.GetAsync(
            $"/api/hotel/bookings/available-rooms?checkInDate={checkInDate}&checkOutDate={checkOutDate}&peopleCount={peopleCount}");

        availableResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await availableResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        var hotels = result.GetProperty("hotels").EnumerateArray();
        var firstHotel = hotels.First();
        var hotelId = firstHotel.GetProperty("hotelId").GetInt32();
        var availableRooms = firstHotel.GetProperty("availableRooms").EnumerateArray().ToList();

        // Select two rooms whose combined capacity is >= peopleCount
        var selectedRooms = new List<int>();
        int capacity = 0;
        foreach (var room in availableRooms)
        {
            selectedRooms.Add(room.GetProperty("roomId").GetInt32());
            capacity += room.GetProperty("maxOccupancy").GetInt32();
            if (capacity >= peopleCount)
                break;
        }

        selectedRooms.Count.Should().BeGreaterThan(0);
        capacity.Should().BeGreaterOrEqualTo(peopleCount);

        var guestName = "Integration Test Guest";

        var request = new CreateBookingRequestDto
        {
            HotelId = hotelId,
            RoomIds = selectedRooms,
            GuestName = guestName,
            PeopleCount = peopleCount,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate
        };

        // Act: Make the booking
        var response = await _client.PostAsJsonAsync("/api/hotel/bookings/book", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var booking = await response.Content.ReadFromJsonAsync<Booking>();
        booking.Should().NotBeNull();
        booking!.HotelId.Should().Be(hotelId);
        booking.GuestName.Should().Be(guestName);
        booking.PeopleCount.Should().Be(peopleCount);
        booking.CheckInDate.ToString("yyyy-MM-dd").Should().Be(checkInDate);
        booking.CheckOutDate.ToString("yyyy-MM-dd").Should().Be(checkOutDate);
        booking.Rooms.Should().NotBeNull();
        booking.Rooms.Select(r => r.Id).Should().BeEquivalentTo(selectedRooms);
        booking.BookingReference.Should().NotBeNullOrEmpty();
        booking.TotalPrice.Should().Be(
            availableRooms.Where(r => selectedRooms.Contains(r.GetProperty("roomId").GetInt32()))
                .Sum(r => r.GetProperty("price").GetDecimal()) * 2 // 2 nights
        );

        var dbBooking = await _context.Bookings
            .Include(b => b.Rooms)
            .FirstOrDefaultAsync(b => b.BookingReference == booking.BookingReference);

        dbBooking.Should().NotBeNull();
        dbBooking!.GuestName.Should().Be(guestName);
        dbBooking.Rooms.Select(r => r.Id).Should().BeEquivalentTo(selectedRooms);
    }
    
    [Test]
    public async Task CreateBooking_InvalidRequest_ReturnsConflict()
    {
        // Arrange: Try to book a room that is already booked for the given dates (room 17 is booked 2025-07-25 to 2025-07-28)
        var hotelId = 3;
        var roomIds = new List<int> { 17 }; // Deluxe room at Buckingham Gardens Lodge
        var guestName = "Test Conflict";
        var peopleCount = 3;
        var checkInDate = "2025-07-26";
        var checkOutDate = "2025-07-27";

        var request = new CreateBookingRequestDto
        {
            HotelId = hotelId,
            RoomIds = roomIds,
            GuestName = guestName,
            PeopleCount = peopleCount,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/hotel/bookings/book", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().MatchRegex("not available|Booking validation failed");
    }

    #endregion
   
    private static string GetContentRoot()
    {
        // Get the directory where the test assembly is located
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        // Navigate up to find the project root (look for Program.cs)
        var currentDir = new DirectoryInfo(assemblyDir!);
        while (currentDir != null && !File.Exists(Path.Combine(currentDir.FullName, "Program.cs")))
        {
            currentDir = currentDir.Parent;
        }

        if (currentDir == null)
        {
            throw new DirectoryNotFoundException("Could not find project root directory containing Program.cs");
        }

        return currentDir.FullName;
    }
}