using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using RecomendadorDePeliulas.ML;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Cargar configuración desde appsettings.json y variables de entorno
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
var config = builder.Configuration;
string apiKey = config["TMDB:ApiKey"];
string accesToken = config["TMDB:AccesToken"];

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
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
var moviePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "_movies.csv");

// Registrar el servicio pasando las rutas
builder.Services.AddSingleton<IModelMovieRecomender>(sp =>
{
    var mlContext = sp.GetRequiredService<MLContext>();
    return new ModelMovieRecommender(mlContext, modelPath, dataPath);
});

builder.Services.AddScoped<IPeliculasLogica, PeliculasLogica>(sp =>
{
    return new PeliculasLogica(moviePath);
});

builder.Services.AddScoped<ITmdbLogica, TmdbLogica>(sp =>
{
    return new TmdbLogica(apiKey,accesToken);
});

builder.Services.AddScoped<IUsuarioLogica,UsuarioLogica>();

builder.Services.AddScoped<IRecomenderLogica,RecomenderLogica>();

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
