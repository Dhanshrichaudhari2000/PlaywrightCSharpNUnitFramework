using Microsoft.Playwright;

public class Authentication
{
    public static async Task CreateSessionAsync(
        IPlaywright playwright,
        string authFile)
    {
        await using var browser = await playwright.Chromium.LaunchAsync();

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(AppConfig.BaseURL);
        
        var _homePage = new PlaywrightHomePage(Page);
        await _homePage.LoginAsync(AppConfig.DefaultUsername, AppConfig.DefaultPassword);

        //await page.WaitForURLAsync("**/dashboard");

        await context.StorageStateAsync(
            new BrowserContextStorageStateOptions
            {
                Path = authFile
            });

        await browser.CloseAsync();
    }
}