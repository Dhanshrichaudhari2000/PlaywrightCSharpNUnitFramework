using Microsoft.Playwright;

namespace PlaywrightNUnitFramework.Pages;

public class CheckoutStepTwoPage
{
    private readonly IPage _page;

    public CheckoutStepTwoPage(IPage page)
    {
        _page = page;
    }

    private ILocator FinishButton => _page.GetByRole(AriaRole.Button, new() {Name="Finish"});
    private ILocator CancelButton => _page.GetByTestId("cancel");
    private ILocator SummarySubtotal => _page.Locator(".summary_subtotal_label");
    private ILocator SummaryTax => _page.Locator(".summary_tax_label");    
    private ILocator SummaryTotal => _page.Locator(".summary_total_label");
    private ILocator CartItems => _page.Locator(".cart_item");

    public Task<int> GetItemCountAsync() => CartItems.CountAsync();

    public async Task<decimal> GetSubtotalAsync() =>
        ParseCurrency(await SummarySubtotal.InnerTextAsync());

    public async Task<decimal> GetTaxAsync() =>
        ParseCurrency(await SummaryTax.InnerTextAsync());

    public async Task<decimal> GetTotalAsync() =>
        ParseCurrency(await SummaryTotal.InnerTextAsync());

    public async Task<CheckoutCompletePage> FinishAsync()
    {
        await FinishButton.ClickAsync();
        return new CheckoutCompletePage(_page);
    }

    private static decimal ParseCurrency(string label)
    {
        // Labels look like "Total: $32.39" / "Item total: $29.99" / "Tax: $2.40"
        var numeric = label.Split('$').Last();
        return decimal.Parse(numeric);
    }
}
