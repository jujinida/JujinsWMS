using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server 연결 풀링 최적화
builder.Services.AddSingleton<SqlConnectionStringBuilder>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection");
    return new SqlConnectionStringBuilder(connectionString)
    {
        Pooling = true,
        MaxPoolSize = 100,
        MinPoolSize = 5,
        ConnectTimeout = 30,
        CommandTimeout = 30,
        ConnectRetryCount = 3,
        ConnectRetryInterval = 10
    };
});

// AWS S3 서비스 등록
builder.Services.AddSingleton<IAmazonS3>(provider =>
{
    var s3Config = new Amazon.S3.AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2,
        ServiceURL = "https://s3.ap-southeast-2.amazonaws.com"
    };
    
    return new AmazonS3Client("AKIASKD5PB3ZHMNVFSVG", "3mmMbruDzQfsZ61PSCsE6zo92aDc0EmlBA/Axu0I", s3Config);
});

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

