using PlaywrightNUnitFramework.Pages;

namespace PlaywrightNUnitFramework.Tests;

[TestFixture]
public class CheckoutWorkflowTests : BaseTest
{
    [Test]
    public async Task CompletePurchase_WithMultipleItems_Succeeds()
    {
        await Page.GotoAsync("https://www.saucedemo.com/inventory.html");
       // var _homePage = new PlaywrightHomePage(Page);
       // await _homePage.LoginAsync(AppConfig.DefaultUsername, AppConfig.DefaultPassword);
        var inventoryPage = new InventoryPage(Page);
       // Assert.That(await inventoryPage.IsLoadedAsync(), Is.True);
        await Expect(inventoryPage.PageTitle).ToContainTextAsync("Products");

        await inventoryPage.AddProductToCartByNameAsync("sauce-labs-backpack");
        await inventoryPage.AddProductToCartByNameAsync("sauce-labs-bike-light");
        Assert.That(await inventoryPage.GetCartItemCountAsync(), Is.EqualTo(2));
        await Expect(Page.Locator(".shopping_cart_badge")).ToHaveTextAsync("2");

        var cartPage = await inventoryPage.GoToCartAsync();
        var itemNames = await cartPage.GetItemNamesAsync();
        Assert.That(itemNames, Is.EquivalentTo(new[] { "Sauce Labs Backpack", "Sauce Labs Bike Light" }));

        var checkoutStepOne = await cartPage.ProceedToCheckoutAsync();
        var checkoutStepTwo = await checkoutStepOne.FillAndContinueAsync("Jane", "Doe", "94105");

        // Sanity-check the price math rather than hard-coding a total, so this test
        // doesn't silently rot if SauceDemo's catalogue prices change.
        var subtotal = await checkoutStepTwo.GetSubtotalAsync();
        var tax = await checkoutStepTwo.GetTaxAsync();
        var total = await checkoutStepTwo.GetTotalAsync();
        Assert.That(total, Is.EqualTo(subtotal + tax).Within(0.01m));
        Assert.That(await checkoutStepTwo.GetItemCountAsync(), Is.EqualTo(2));

        var confirmation = await checkoutStepTwo.FinishAsync();
        //var header = await confirmation.GetConfirmationHeaderAsync();
        //Assert.That(header, Is.EqualTo("Thank you for your order!"));
        await Expect(confirmation.CompleteHeader).ToHaveTextAsync("Thank you for your order!");

        // Cart should be empty again after a completed order.
        var backToProducts = await confirmation.BackToProductsAsync();
        Assert.That(await backToProducts.GetCartItemCountAsync(), Is.EqualTo(0));
    }
}
