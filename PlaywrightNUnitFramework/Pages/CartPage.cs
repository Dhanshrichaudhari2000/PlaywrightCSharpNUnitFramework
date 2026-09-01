using Microsoft.Playwright;
using System.Collections.Generic;
using System.Linq;

namespace PlaywrightNUnitFramework.Pages
{
    public class CartPage
    {
        private readonly IPage _page;

        public CartPage(IPage page)
        {
            _page = page;
        }

        private ILocator CartItems => _page.Locator(".cart_item");
        private ILocator CheckoutButton => _page.GetByRole(AriaRole.Button, new() {Name= "Checkout"});
        private ILocator ContinueShoppingButton => _page.GetByTestId("continue-shopping");
        private ILocator CartItemNames =>CartItems.Locator(".inventory_item_name");

        public Task<int> GetItemCountAsync() => CartItems.CountAsync();

        public async Task<List<string>> GetItemNamesAsync() =>
            (await CartItemNames.AllInnerTextsAsync()).ToList();

        public async Task<CheckoutStepOnePage> ProceedToCheckoutAsync(){
            await CheckoutButton.ClickAsync();
            return new CheckoutStepOnePage(_page);
        }

        public async Task<InventoryPage> ContinueShoppingAsync()
        {
            await ContinueShoppingButton.ClickAsync();
            return new InventoryPage(_page);
        }

        public async Task RemoveItemByNameAsync(string productName)
        {
            var row = CartItems.Filter(new() { HasText = productName });
            await row.GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();
        }
    }
}

