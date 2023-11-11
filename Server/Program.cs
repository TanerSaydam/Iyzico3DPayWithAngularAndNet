using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(cfr =>
{
    cfr.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowCredentials().AllowAnyMethod().SetIsOriginAllowed(policy => true);
    });
});

builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.MapHub<PayHub>("/pay-hub");

app.Run();
