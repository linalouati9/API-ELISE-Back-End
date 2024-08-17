using api_elise.Data;
using api_elise.Helper;
using api_elise.Interfaces;
using api_elise.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<SeedData>();
builder.Services.AddTransient<ApiClient>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register the service in the dependency injection container
builder.Services.AddScoped<IModelRepository, ModelRepository>();
builder.Services.AddScoped<IQRCodeRepository, QRCodeRepository>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers().AddJsonOptions(options =>{
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        });


var app = builder.Build();

if(args.Length == 1 && args[0].ToLower() == "seeddata")
    SeedData(app);

void SeedData(IHost app)
{
    try
    {
        var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

        using (var scope = scopedFactory.CreateScope())
        {
            var service = scope.ServiceProvider.GetService<SeedData>();
            service?.SeedDataContext(); // Using the Null-conditional operator to avoid NullReferenceException
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding data: {ex.Message}");
        // Add specific error handling here, like logging, sending notifications ..
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
