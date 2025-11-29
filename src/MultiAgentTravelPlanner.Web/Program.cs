using MultiAgentTravelPlanner.Web.Configuration;
using MultiAgentTravelPlanner.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Register configuration settings
builder.Services.Configure<AzureAISettings>(
    builder.Configuration.GetSection(AzureAISettings.SectionName));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the Azure AI Travel service
builder.Services.AddScoped<IAzureAITravelService, AzureAITravelService>();

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


app.Run();
