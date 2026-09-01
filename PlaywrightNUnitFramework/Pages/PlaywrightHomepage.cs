using Microsoft.Playwright;
namespace PlaywrightNUnitFramework.Pages;

public class PlaywrightHomePage
{
    private readonly IPage _page;
    public PlaywrightHomePage(IPage page)
    {
        _page = page;
    }

    private ILocator UserNameInput => _page.GetByPlaceholder("Username");
    private ILocator PasswordInput => _page.GetByPlaceholder("Password");
    private ILocator LoginButton => _page.Locator("#login-button");
    public ILocator SuccessAlert => _page.Locator("#inventory_container").Nth(0);

    public async Task LoginAsync(string username, string password)
    {
        await UserNameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();  
      
        var pages = _page.Context.Pages;
        foreach (var page in pages)
        {
            var title = await page.TitleAsync();
            if (title == "mazon")
            {
                await page.BringToFrontAsync();
            }
        }

    }
}
