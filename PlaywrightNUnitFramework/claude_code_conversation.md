# SauceDemo Playwright Suite

## Prompt 1 — Scoping

**Me:** I need to build a Playwright test suite in C# against saucedemo.com for a take-home
challenge. Requirements: 2+ browsers, POM with 3+ pages, auth tests, a full checkout journey,
2+ negative tests, 1 data-driven test, HTML reporting + screenshot-on-failure, and a CI config.
Before writing code, help me pick the right Playwright .NET packages and explain how
config (browser choice, base URL, retries) works in .NET, since there's no playwright.config.ts.

**Claude:** Recommended `Microsoft.Playwright.NUnit` (gives `PageTest`/`BrowserTest` base classes
with per-test browser context isolation for free), explained that browser/timeout/retry config
in .NET Playwright goes in a `.runsettings` file rather than a JS config file, and that
app-level config (base URL) is cleaner kept in a plain `appsettings.json` read via
`Microsoft.Extensions.Configuration` rather than overloading `.runsettings` for everything.
Proposed the folder layout (Pages / Tests / Utilities / TestData).

**Manual intervention:** Confirmed I wanted NUnit over MSTest/xUnit (NUnit has the most mature
Playwright integration and native `[TestCaseSource]` support for the data-driven requirement).

---

## Prompt 2 — Page Object skeletons

**Me:** Generate the LoginPage, InventoryPage, CartPage, and Checkout page objects. Use
saucedemo's `data-test` attributes as the primary locator strategy, and `GetByRole` for buttons
where it makes sense instead. Each action method that navigates should return the next page
object.

**Claude:** Generated the six page objects (Login, Inventory, Cart, CheckoutStepOne,
CheckoutStepTwo, CheckoutComplete)

**Manual intervention (selector robustness):** Fixed bad locators which was not correct at all

---

## Prompt 3 — Auth + workflow tests

**Me:** Write the authentication tests (valid login, invalid login, locked-out user, logout) and
a full checkout journey test. For logout, also verify the session is actually gone, not just that
we're redirected.

**Claude:** Generated `AuthenticationTests` and `CheckoutWorkflowTests`.

---

## Prompt 4— Data-driven tests

**Me:** Add a data-driven login test that pulls credential sets from an external JSON file
rather than inline `[TestCase]` attributes, so it's clearly separated as test data.

**Claude:** Generated `TestData/users.json`, `TestDataProvider.cs`, and the
`[TestCaseSource]`-based test in `DataDrivenTests.cs`.

---

## Prompt 5 — Reporting, screenshots-on-failure, tracing

**Me:** Wire up HTML, and make sure every failing test automatically gets a
screenshot. Also consider whether a full Playwright trace per test would help debugging later.

**Claude:** Added the `.runsettings` logger config and a `TestBase` with `[TearDown]` screenshot
capture gated on `TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed`, plus
per-test tracing (not just on failure — cheap insurance for diagnosing intermittent flakiness
after the fact, since a passing-but-slow test can still be a canary).
