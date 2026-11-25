
using GymControl.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ClienteDAO>();
builder.Services.AddScoped<MembresiaDAO>();
builder.Services.AddScoped<PagoDAO>();
builder.Services.AddScoped<ProductoDAO>();
builder.Services.AddScoped<VentaDAO>();

// Actividad 10: Reportes
builder.Services.AddScoped<ReporteDAO>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
