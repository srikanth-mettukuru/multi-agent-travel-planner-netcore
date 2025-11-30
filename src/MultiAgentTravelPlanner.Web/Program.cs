using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Services;
using MultiAgentTravelPlanner.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Register configuration settings
builder.Services.Configure<AzureAISettings>(
    builder.Configuration.GetSection(AzureAISettings.SectionName));

builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection(OpenAISettings.SectionName));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the Azure AI Travel service
//builder.Services.AddScoped<IAzureAITravelService, AzureAITravelService>();
builder.Services.AddScoped<IFlightsAgent, FlightsAgent>();
builder.Services.AddScoped<IHotelsAgent, HotelsAgent>();
builder.Services.AddScoped<IAttractionsAgent, AttractionsAgent>();
builder.Services.AddScoped<IFoodAgent, FoodAgent>();
builder.Services.AddScoped<ITravelAgent, TravelAgent>(); // Supervisor agent

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Travel/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Travel}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/test-flights", async (IFlightsAgent flightsAgent) =>
    {
        var result = await flightsAgent.SearchFlightsAsync(
            "Seattle", 
            "Tokyo", 
            new DateTime(2025, 12, 20), 
            new DateTime(2025, 12, 28));
        
        return Results.Json(result);
    });

    app.MapGet("/test-hotels", async (IHotelsAgent hotelsAgent) =>
    {
        var result = await hotelsAgent.SearchHotelsAsync(
            "Tokyo",
            new DateTime(2025, 12, 20),
            new DateTime(2025, 12, 28));
        
        return Results.Json(result);
    });

    app.MapGet("/test-attractions", async (IAttractionsAgent attractionsAgent) =>
    {
        var result = await attractionsAgent.SearchAttractionsAsync(
            "Tokyo",
            numberOfAttractions: 7);
        
        return Results.Json(result);
    });

    app.MapGet("/test-food", async (IFoodAgent foodAgent) =>
    {
        var result = await foodAgent.SearchRestaurantsAsync(
            "Tokyo",
            numberOfRestaurants: 6);
        
        return Results.Json(result);
    });

    app.MapGet("/test-travel", async (ITravelAgent travelAgent) =>
    {
        var request = new TravelRequest
        {
            Origin = "Seattle",
            Destination = "Tokyo",
            StartDate = new DateTime(2025, 12, 20),
            EndDate = new DateTime(2025, 12, 28)
        };

        var itinerary = await travelAgent.PlanTripAsync(request);
        
        return Results.Json(itinerary);
    });
}


app.Run();
