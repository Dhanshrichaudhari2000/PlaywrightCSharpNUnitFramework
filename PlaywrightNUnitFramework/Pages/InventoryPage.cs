using Microsoft.Playwright;
using PlaywrightNUnitFramework.Pages;

namespace PlaywrightNUnitFramework.Pages;

public enum SortOrder
{
    NameAToZ,
    NameZToA,
    PriceLowToHigh,
    PriceHighToLow,
}

public class InventoryPage : BaseTest
{
    private readonly IPage _page;
    public InventoryPage(IPage page)
    {
        _page = page;
    }

    public ILocator PageTitle => _page.Locator(".title");
    private ILocator InventoryItems => _page.GetByTestId("inventory-item");
    private ILocator SortDropdown => _page.GetByTestId("product-sort-container");
    private ILocator ItemPrices => _page.GetByTestId("inventory-item-price");
    private ILocator CartItems => _page.GetByTestId("inventory-item");
    public ILocator CartBadge => _page.Locator("#shopping_cart_container");
    private ILocator CartLink => _page.GetByTestId("shopping-cart-link");
    private ILocator MenuButton => _page.GetByRole(AriaRole.Button, new() {Name= "Open Menu"});
    private ILocator Logout => _page.GetByText("Logout");
    public Task<int> GetItemCountAsync() => CartItems.CountAsync();

    public async Task<bool> IsLoadedAsync()
    {
        await PageTitle.WaitForAsync();
        return await PageTitle.InnerTextAsync() == "Products";
    }

    public Task<int> GetProductCountAsync() => InventoryItems.CountAsync();
    public async Task<int> GetCartItemCountAsync()
    {
        if (!await CartBadge.IsVisibleAsync())
        {
            return 0;
        }

        var text = await CartBadge.InnerTextAsync();
         if (string.IsNullOrWhiteSpace(text))
        return 0;

        return int.Parse(text);
    }

    /// <summary>Adds a product to the cart by its visible name, e.g. "Sauce Labs Backpack".
    /// Scopes the "Add to cart" button lookup to the specific product card so this works
    /// correctly no matter how many other items are on the page.</summary>
    public async Task AddProductToCartByNameAsync(string productName)
    {
        await _page.Locator($"#add-to-cart-{productName}").ClickAsync();
        /*var card = InventoryItems.Filter(new() { HasText = productName });
        await card.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();*/
    }

     public async Task<bool> IsProductsPageDisplayedAsync()
    {
        return await InventoryItems.IsVisibleAsync();
    }

    public async Task<Pages.CartPage> GoToCartAsync()
    {
        await CartBadge.ClickAsync();
        return new Pages.CartPage(_page);
    }
    public async Task SortByAsync(SortOrder order)
    {
        var value = order switch
        {
            SortOrder.NameAToZ => "az",
            SortOrder.NameZToA => "za",
            SortOrder.PriceLowToHigh => "lohi",
            SortOrder.PriceHighToLow => "hilo",
            _ => throw new ArgumentOutOfRangeException(nameof(order)),
        };
        await SortDropdown.SelectOptionAsync(value);
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        }

        public async Task LogoutAsync()
        {
            await MenuButton.ClickAsync();
            await Logout.ClickAsync();
        }
    }
}
