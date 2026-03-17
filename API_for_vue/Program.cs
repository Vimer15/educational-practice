var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// 👇 ДОБАВЛЕНО: Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // порт Vue (Vite)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ⚠️ ВАЖНО: UseCors() должен быть ПЕРЕД UseAuthorization() и UseRouting() (но после app.Build())
app.UseCors("AllowVue"); // 👈 ДОБАВЛЕНО

app.UseAuthorization();

app.MapControllers();

app.Run();