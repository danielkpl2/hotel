using Hotel.Data;
using Hotel.Services;
using Hotel.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(); // <-- Enable Swagger/OpenAPI

// builder.Services.AddSwaggerGen();


// Add Entity Framework
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Add Services
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<HotelService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // <-- Serve OpenAPI JSON at /swagger/v1/swagger.json
    app.UseSwaggerUI(); 
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
