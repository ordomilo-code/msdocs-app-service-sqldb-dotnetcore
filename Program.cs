using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;
var builder = WebApplication.CreateBuilder(args);

// Add database context and cache
if(builder.Environment.IsDevelopment())
{
    var developmentConnectionString = DevelopmentConnectionStringSelector.GetDevelopmentConnectionString(builder.Configuration);

    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(developmentConnectionString));
    builder.Services.AddDistributedMemoryCache();
}
else
{
    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));
    builder.Services.AddStackExchangeRedisCache(options =>
    {
    options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
    options.InstanceName = "SampleInstance";
    });
}


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add App Service logging
builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DevelopmentDatabaseInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todos}/{action=Index}/{id?}");

await app.RunAsync();
