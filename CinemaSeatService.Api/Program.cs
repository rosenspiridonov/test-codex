using System.Net;
using Polly;
using Polly.Extensions.Http;
using CinemaSeatService.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<SeatMapService>(client =>
    client.BaseAddress = new Uri("https://raw.githubusercontent.com/DataArtInc/interview-technical-exercise/main/seatmap-example.json"))
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/seats", async (SeatMapService service, CancellationToken ct) =>
{
    var plan = await service.GetSeatPlanAsync(ct);
    return Results.Ok(plan);
}).WithName("GetSeatPlan");

app.MapGet("/seats/{row}/{seat}", async (string row, int seat, SeatMapService service, CancellationToken ct) =>
{
    var available = await service.CheckSeatAsync(row, seat, ct);
    return available.HasValue ? Results.Ok(new { available }) : Results.NotFound();
}).WithName("CheckSeat");

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200));
}
