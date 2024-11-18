using AspNetCore.Identity.MongoDbCore.Models;
using gymnasium_academia.Settings;
using Microsoft.AspNetCore.Identity;
using gymnasium_academia.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

// Configurando os servi�os de identidade com MongoDB
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
        (
            mongoDbSettings.ConnectionString, mongoDbSettings.Name
        );

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    var roles = new[] { "Usuario", "Aluno", "Personal", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
