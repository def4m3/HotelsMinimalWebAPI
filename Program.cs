using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<HotelDb>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Sql"));
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HotelDb>();
            db.Database.EnsureCreated();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapGet("/hotels", async (HotelDb db) => await db.Hotels.ToListAsync());
        app.MapGet("/hotels/{id}", async (int id, HotelDb db) => await db.Hotels.FirstOrDefaultAsync(hotel => hotel.Id == id) is Hotel hotel ? Results.Ok(hotel) : Results.NotFound());
        app.MapPost("/hotels", async (Hotel hotel, HotelDb db) => await db.Hotels.AddAsync(hotel));
        app.MapPut("/hotels", async (Hotel h, HotelDb db) =>
        {
            var hotel = await db.Hotels.FindAsync(h);
            hotel = h;
            await db.SaveChangesAsync();
        });
        app.MapDelete("/hotels/{id}", async (int id, HotelDb db) =>
        {
            var hotel = await db.Hotels.FirstOrDefaultAsync(hotel => hotel.Id == id);
            if (hotel == null) throw new Exception("Not Found");

            db.Hotels.Remove(hotel);
            await db.SaveChangesAsync();
        });

        app.UseHttpsRedirection();

        app.Run();


    }
}

public class HotelDb : DbContext
{
    public HotelDb(DbContextOptions<HotelDb> options) : base(options) {}

    public DbSet<Hotel> Hotels => Set<Hotel>();
}
public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public double Latitude {  get; set; }

    public double Longitude { get; set; }
}