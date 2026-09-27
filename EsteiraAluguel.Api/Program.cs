using Microsoft.EntityFrameworkCore;
using EsteiraAluguel.Infrastructure.Data;
using EsteiraAluguel.Application.UseCases;

var builder = WebApplication.CreateBuilder(args);

// Configuração do PostgreSQL
builder.Services.AddDbContext<EsteiraDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do MediatR injetando a montagem correta (Assembly)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AtivarPropostaCommand).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-Migration e inicialização do banco ao rodar o container
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EsteiraDbContext>();
    // No ambiente Docker, o banco demorar uns segundos para subir. Um retry básico pode ser implementado.
    db.Database.EnsureCreated(); 
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();