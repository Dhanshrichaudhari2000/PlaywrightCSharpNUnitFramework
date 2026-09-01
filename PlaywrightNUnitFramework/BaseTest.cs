using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace PlaywrightNUnitFramework;

public class BaseTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            StorageStatePath = "auth.json",
            //BaseURL = AppConfig.BaseUrl,
            ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
        };
    }

    [SetUp]
    public async Task SetDefaultTimeouts()
    {
        await Context.Tracing.StartAsync(new()
       {
           //Title = $"{TestContext.CurrentContext.Test.ClassName}.{safeName}",
           Screenshots = true,
           Snapshots = true,
           Sources = true,
           //Video= true,
       });
        await Page.GotoAsync(AppConfig.BaseUrl);
        /*await context.StorageStateAsync(new()
        {
            Path = "auth.json"
        });*/

        Page.SetDefaultTimeout(10000);              // action timeout (click, fill, etc.)
        Page.SetDefaultNavigationTimeout(30000);     // GotoAsync specifically
    }

    [TearDown]
    public async Task TearDown()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var safeName = Regex.Replace(testName, @"[^a-zA-Z0-9_-]","_");
        await Context.Tracing.StopAsync(new()
       {
           Path = Path.Combine(
               Environment.CurrentDirectory,
               "traces",
               $"{TestContext.CurrentContext.Test.ClassName}.{safeName}.zip"),
       });
    }

}



