using ChatAppApi.DataService;
using ChatAppApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR(); // Add SignalR services

builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChatAppPolicy", builder =>
        builder.WithOrigins("http://localhost:5177", "http://localhost:5176", "http://localhost:5178")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<DataService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("ChatAppPolicy");

//app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>(ChatHub.HubUrl);

app.Run();
