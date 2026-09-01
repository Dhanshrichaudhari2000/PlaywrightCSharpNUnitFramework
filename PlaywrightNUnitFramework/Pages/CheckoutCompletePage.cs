using Microsoft.Playwright;

namespace PlaywrightNUnitFramework.Pages;
public class CheckoutCompletePage
{
    private readonly IPage _page;

    public CheckoutCompletePage(IPage page)
    {
        _page = page;
    }

    public ILocator CompleteHeader => _page.Locator(".complete-header");
    private ILocator BackHomeButton => _page.GetByRole(AriaRole.Button, new() {Name="Back Home"});

    public async Task<string> GetConfirmationHeaderAsync() => await CompleteHeader.InnerTextAsync();

    public async Task<InventoryPage> BackToProductsAsync()
    {
        await BackHomeButton.ClickAsync();
        return new InventoryPage(_page);
    }
}
