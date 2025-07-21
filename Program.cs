using Hotel.Data;
using Hotel.Services;
using Hotel.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Add Entity Framework
// var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
//     ?? builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// var ConnectionStrings__DefaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Console.WriteLine($"Connection string: {Environment.GetEnvironmentVariable("DefaultConnection")}");
// Console.WriteLine($"ConnectionStrings__DefaultConnection: {ConnectionStrings__DefaultConnection}");

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Repositories
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Add Services
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<HotelService>();

var app = builder.Build();

// Apply EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

// }

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
