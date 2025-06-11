using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using RecomendadorDePeliulas.ML;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.SlidingExpiration = true;
    });


//insertar ml y ademas el movie recomender

builder.Services.AddScoped<RecomendadorPeliculasContext>();

builder.Services.AddSingleton<MLContext>();

var modelPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "MovieRecommenderModel.zip");
var dataPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "data-ratings.csv");

// Registrar el servicio pasando las rutas
builder.Services.AddSingleton<IModelMovieRecomender>(sp =>
{
    var mlContext = sp.GetRequiredService<MLContext>();
    return new ModelMovieRecommender(mlContext, modelPath, dataPath);
});

builder.Services.AddScoped<IUsuarioLogica,UsuarioLogica>();

builder.Services.AddScoped<IPeliculaLogica,PeliculaLogica>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();
app.UseSession();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
