using Autofac;
using Autofac.Extensions.DependencyInjection;
using dafukMedia.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureContainer<ContainerBuilder>(b => b.RegisterModule(new AutofacModule()));
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllers();
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
