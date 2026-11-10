
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Varsayýlan dosyayý (index.html) belirle
app.UseDefaultFiles();

// wwwroot içindeki statik dosyalarý sun
app.UseStaticFiles();

app.Run();
