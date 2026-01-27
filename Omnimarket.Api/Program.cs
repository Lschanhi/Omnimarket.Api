using System.Text.Json.Serialization;
using Omnimarket.Api.Data;
using System.Text;
using Omnimarket.Api.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder
        .Configuration.GetConnectionString("ConexaoSomee"));
});
/*
builder.Services.AddHttpClient<ICpfService, CpfService>(client =>
{
    // Configure aqui a URL base da API que você escolheu
    // client.BaseAddress = new Uri("https://brasilapi.com.br/api/"); // Exemplo
    client.BaseAddress = new Uri("incluir a url do api de consultar CPF"); // Exemplo
    
    // Se precisar de Token (Authorization)
    // client.DefaultRequestHeaders.Add("Authorization", "Bearer SEU_TOKEN_AQUI");
});
*/
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();
app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
