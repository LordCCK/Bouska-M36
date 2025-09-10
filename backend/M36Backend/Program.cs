using M36Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowElectron",
        policy =>
        {
            policy.WithOrigins("*")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Registrace služeb
builder.Services.AddSingleton<IBMSQLService>();
builder.Services.AddSingleton<MSSQLService>();
builder.Services.AddSingleton<ZebraPrinterService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors("AllowElectron");
app.UseAuthorization();

app.MapControllers();

app.Run("http://localhost:5000");
