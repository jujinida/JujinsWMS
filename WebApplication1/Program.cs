var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

var app = builder.Build();

// 컨트롤러 사용 시 필요
app.MapControllers();

app.Run();