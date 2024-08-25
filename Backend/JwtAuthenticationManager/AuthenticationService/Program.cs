using AuthenticationService.Models;
using AuthenticationService.Repositories;
using AuthenticationService.Repositories.CustomerRepositories;
using AuthenticationService.Services;
using MessageBrokerService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure database context
builder.Services.AddDbContext<AuthenticateContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register scoped services
builder.Services.AddScoped<JwtTokenHandler>();
<<<<<<< HEAD
=======
builder.Services.AddScoped<JwtTokenHandlerEmp>();
>>>>>>> 960a83c (commit)
builder.Services.AddScoped<ICustomerRepo, CustomerRepo>();

// Register singleton services
builder.Services.AddSingleton<BaseMessageBroker>();
builder.Services.AddSingleton<CustomerAuthenticationConsumer>();
builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<CustomerAuthenticationConsumer>());
<<<<<<< HEAD

=======
builder.Services.AddSingleton<EmployeeAuthenticationConsumer>();
builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<EmployeeAuthenticationConsumer>());
>>>>>>> 960a83c (commit)
// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdmin",
<<<<<<< HEAD
        builder => builder.WithOrigins("http://localhost:9000")
=======
        builder => builder.WithOrigins("http://localhost:1000")
>>>>>>> 960a83c (commit)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPublic",
        builder => builder.WithOrigins("http://localhost:2000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
});
<<<<<<< HEAD
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOther",
        builder => builder.WithOrigins("http://localhost:1000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
});
=======
>>>>>>> 960a83c (commit)

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAdmin");
app.UseCors("AllowPublic");
<<<<<<< HEAD
app.UseCors("AllowOther");
=======
>>>>>>> 960a83c (commit)
app.UseAuthorization();
app.MapControllers();

app.Run();
