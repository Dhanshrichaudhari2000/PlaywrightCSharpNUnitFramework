using PlaywrightNUnitFramework.Pages;
using PlaywrightNUnitFramework.TestData;

namespace PlaywrightNUnitFramework.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlaywrightHomeTests : BaseTest
{
    [TestCaseSource(typeof(LoginTestData),nameof(LoginTestData.Users))]
    [Test]
    public async Task Login_With_Different_Users(string username, string password, bool expectedToLogin)
    {
        var loginPage = new PlaywrightHomePage(Page);

        await loginPage.LoginAsync(username, password);

        if (expectedToLogin)
        {
            await Expect(Page.GetByText("Products",new() { Exact = true })).ToBeVisibleAsync();
        }
        else
        {
            await Expect(Page.Locator("//h3[@data-test='error']")).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task LoginWithInvalidCredentials()
    {
       var loginPage = new PlaywrightHomePage(Page);

      //await loginPage.NavigateAsync();

        await loginPage.LoginAsync(
            "invalid_user",
            "invalid_password");

        var error = await loginPage.GetErrorMessageAsync();

        Assert.That(error, Does.Contain(
            "Username and password do not match"));
    }

    [Test]
    public async Task LogoutSuccessfully()
    {
        var loginPage = new PlaywrightHomePage(Page);
        var productsPage = new InventoryPage(Page);

        await loginPage.LoginAsync("standard_user","secret_sauce");

        Assert.That(await productsPage.IsProductsPageDisplayedAsync(), Is.True);

        await productsPage.LogoutAsync();

        Assert.That(Page.Url,Does.Contain("saucedemo.com"));
    }
}
