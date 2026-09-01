using Microsoft.Extensions.Configuration;

public static class AppConfig
{
    private static readonly IConfigurationRoot _config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false)
        .AddEnvironmentVariables(prefix: "SAUCEDEMO_")   // lets CI override without touching the file
        .Build();

    public static string BaseUrl => _config["BaseUrl"]!;
    public static string DefaultUsername => _config["DefaultUser:Username"]!;
    public static string DefaultPassword => _config["DefaultUser:Password"]!;
}