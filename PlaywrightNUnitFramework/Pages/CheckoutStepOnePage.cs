using Microsoft.Playwright;

namespace PlaywrightNUnitFramework.Pages;

public class CheckoutStepOnePage
{
    private readonly IPage _page;

    public CheckoutStepOnePage(IPage page)
    {
        _page = page;
    }

    private ILocator FirstNameInput => _page.GetByPlaceholder("First Name");
    private ILocator LastNameInput => _page.GetByPlaceholder("Last Name");
    private ILocator PostalCodeInput => _page.GetByPlaceholder("Zip/Postal Code");
    private ILocator ContinueButton => _page.Locator("#continue");
    private ILocator CancelButton => _page.Locator("#cancel");
    private ILocator ErrorMessage => _page.GetByTestId("error");

    public async Task<CheckoutStepTwoPage> FillAndContinueAsync(string firstName, string lastName, string postalCode)
    {
        await FirstNameInput.FillAsync(firstName);
        await LastNameInput.FillAsync(lastName);
        await PostalCodeInput.FillAsync(postalCode);
        await ContinueButton.ClickAsync();
        return new CheckoutStepTwoPage(_page);
    }

    /// <summary>Submits whatever is currently in the form without changing it — used to
    /// drive validation-error edge cases (e.g. leaving a required field blank).</summary>
    public Task SubmitAsync() => ContinueButton.ClickAsync();

    public Task FillFirstNameAsync(string value) => FirstNameInput.FillAsync(value);
    public Task FillLastNameAsync(string value) => LastNameInput.FillAsync(value);
    public Task FillPostalCodeAsync(string value) => PostalCodeInput.FillAsync(value);

    public async Task<string> GetErrorMessageAsync()
    {
        await ErrorMessage.WaitForAsync();
        return await ErrorMessage.InnerTextAsync();
    }

    public async Task<CartPage> CancelAsync()
    {
        await CancelButton.ClickAsync();
        return new CartPage(_page);
    }
}
