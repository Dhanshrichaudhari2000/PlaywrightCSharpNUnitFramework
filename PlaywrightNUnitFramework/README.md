# SauceDemo E2E Test Suite — Playwright (C# / .NET)

An automated end-to-end test suite for the [Swag Labs / SauceDemo](https://www.saucedemo.com)
demo application, built with **Playwright for .NET** and **NUnit**.

> **System under test:** https://www.saucedemo.com (chosen over the RealWorld app because its
> `data-test` attributes make for an especially clean demonstration of resilient locator strategy,
> and its built-in fixture users — `locked_out_user`, `problem_user`, `error_user`, `visual_user`,
> `performance_glitch_user` — are purpose-built for exercising negative paths and visual regression.)

---

## 1. A note on "playwright.config.ts"

The brief asks for **C#**, and Playwright's .NET port does not use a `playwright.config.ts` file —
that format is specific to the JS/TS package. The .NET equivalent configuration surface is:

| JS/TS concept | C# equivalent in this repo |
|---|---|
| `playwright.config.ts` → `use.baseURL`, `use.trace`, `projects` (browsers) | [`.runsettings`](SauceDemo.Tests/.runsettings) — read by `Microsoft.Playwright.NUnit`'s `BrowserTest`/`PageTest` base classes |
| App-level config (base URL, timeouts, default user) | [`appsettings.json`](SauceDemo.Tests/appsettings.json) — read by [`Utilities/AppConfig.cs`](SauceDemo.Tests/Utilities/AppConfig.cs) |
| `retries`, `reporter` | `.runsettings` `<NUnit>`/`<LoggerRunSettings>` blocks + CI flags |

This mirrors how a real .NET/Playwright project is configured; there is no missing TS file.

---

## 2. Project structure

```
saucedemo-playwright-tests/
├── SauceDemo.Tests.sln
├── SauceDemo.Tests/
│   ├── SauceDemo.Tests.csproj      # NuGet deps: Playwright.NUnit, NUnit, axe-core, etc.
│   ├── .runsettings                # browser/timeout/retry config (playwright.config.ts equivalent)
│   ├── appsettings.json            # base URL, default timeouts, default test user
│   ├── Pages/                      # Page Object Model
│   │   ├── BasePage.cs             # shared nav (logout, cart link, reset state)
│   │   ├── LoginPage.cs
│   │   ├── InventoryPage.cs
│   │   ├── CartPage.cs
│   │   ├── CheckoutStepOnePage.cs
│   │   ├── CheckoutStepTwoPage.cs
│   │   └── CheckoutCompletePage.cs
│   ├── Tests/
│   │   ├── AuthenticationTests.cs      # login success/failure, locked-out, logout
│   │   ├── CheckoutWorkflowTests.cs    # full purchase journey, sort, remove-from-cart
│   │   ├── EdgeCaseTests.cs            # validation errors, empty-cart checkout, error_user
│   │   ├── DataDrivenTests.cs          # parameterised login across many credential sets
│   │   ├── ApiTests.cs                 # bonus: HTTP-level checks via APIRequestContext
│   │   ├── VisualRegressionTests.cs    # bonus: screenshot comparison
│   │   ├── NetworkMockTests.cs         # bonus: request interception/mocking
│   │   └── AccessibilityTests.cs       # bonus: axe-core WCAG scans
│   ├── Utilities/
│   │   ├── TestBase.cs              # PageTest subclass: tracing + screenshot-on-failure
│   │   ├── AppConfig.cs             # typed appsettings.json reader
│   │   └── TestDataProvider.cs      # loads TestData/users.json for [TestCaseSource]
│   └── TestData/
│       └── users.json               # data-driven credential sets
├── .github/workflows/playwright.yml # CI: Chromium + Firefox matrix
└── README.md
```

**Test isolation:** every `[Test]` inherits from `TestBase` → `PageTest`, which gives each test
its own fresh `BrowserContext`/`Page` (separate cookies, localStorage, and session) — no login
state or cart contents ever leaks between tests, and tests are safe to run in parallel
(`NumberOfTestWorkers` in `.runsettings`).

---

## 3. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Internet access to NuGet (for package restore) and to the browser binaries Playwright downloads

## 4. Setup

```bash
cd SauceDemo.Tests

# 1. Restore NuGet packages
dotnet restore

# 2. Build once — this also drops a playwright.ps1 helper script in bin/
dotnet build

# 3. Install the browser binaries (Chromium + Firefox + WebKit, plus OS deps)
pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps
# (no pwsh available? `dotnet tool install --global PowerShell` first, or run
#  `playwright install` from the JS CLI if you have Node — either installs the same browsers)
```

## 5. Configuring the base URL / environment

Two ways, in order of precedence:

```bash
# a) Environment variable override (handy for CI / testing a staging deploy)
export SAUCEDEMO_BaseUrl="https://www.saucedemo.com"

# b) Edit SauceDemo.Tests/appsettings.json directly
{
  "BaseUrl": "https://www.saucedemo.com",
  ...
}
```

Browser choice, headless mode, and timeouts live in `.runsettings`.

## 6. Running the tests

```bash
# Run everything (defaults to Chromium, headless, as configured in .runsettings)
dotnet test --settings .runsettings

# Run against a specific browser
dotnet test --settings .runsettings -- TestRunParameters.Parameter(name="BrowserName", value="firefox")

# Run headed, for local debugging
HEADED=1 dotnet test --settings .runsettings

# Run a single class or test
dotnet test --filter "FullyQualifiedName~AuthenticationTests"
dotnet test --filter "Name=Login_WithInvalidCredentials_ShowsErrorAndStaysOnLoginPage"

# Skip the bonus visual/accessibility suites (useful if browsers/axe aren't installed)
dotnet test --filter "TestCategory!=Visual&TestCategory!=Accessibility"
```

### Updating visual baselines

The first run of `VisualRegressionTests` creates baseline screenshots under
`__screenshots__/`. To intentionally accept a new baseline after a real UI change:

```bash
dotnet test --filter "TestCategory=Visual" -- Playwright.LaunchOptions.Headless=true
# then delete/regenerate the specific .png under __screenshots__/ and re-run
```

## 7. Reports & CI artifacts

Every run produces, under `TestResults/`:

- **HTML report** (`test-report.html`) — human-readable pass/fail summary
- **TRX** — for CI test-result publishing (e.g. GitHub Actions' `test-reporter`)
- **NUnit XML** — for tools expecting the classic NUnit format
- **`screenshots/`** — full-page PNG captured automatically for every *failed* test
  (see `TestBase.BaseTearDown`)
- **`traces/`** — a Playwright trace `.zip` per test (pass or fail). Open with:
  ```bash
  pwsh bin/Debug/net8.0/playwright.ps1 show-trace TestResults/traces/<test-name>.zip
  ```
  Traces give you a full DOM snapshot timeline, network log, and console log per step —
  by far the fastest way to diagnose a flaky or failing test after the fact.

## 8. CI

[`\.github/workflows/playwright.yml`](.github/workflows/playwright.yml) runs the full suite on
every push/PR against a **Chromium + Firefox matrix**, uploads the HTML report/traces/screenshots
as build artifacts, and publishes a test-result summary on the PR via `dorny/test-reporter`.

---

## 9. Design notes / testing philosophy

- **Locator strategy:** SauceDemo ships stable `data-test` attributes on every interactive
  element, which is the most resilient selector available for this app (more stable than text,
  which can change with copy edits, and far more stable than CSS class names, which SauceDemo
  reuses across unrelated elements). Where an element already has a strong accessible role
  (buttons, links), `GetByRole` is used instead so the tests double as a light accessibility
  check on their own. Raw CSS/XPath selectors are avoided everywhere.
- **Page Objects return the next Page Object.** e.g. `LoginPage.LoginAsync()` returns an
  `InventoryPage`, `CartPage.ProceedToCheckoutAsync()` returns a `CheckoutStepOnePage`. This
  makes illegal states hard to represent in a test (you can't call an inventory-page method
  before actually navigating there) and keeps tests reading like a user journey.
- **Assertions live in tests, not Page Objects.** Page Objects only expose getters/actions;
  this keeps them reusable across both "happy path" and "assert this failed" tests.
- **Flakiness/wait handling:** no `Task.Delay`/sleeps anywhere. All waits are Playwright's
  built-in auto-waiting locators (`FillAsync`, `ClickAsync`, `WaitForAsync`) or explicit
  `Expect(...)` polling assertions, both of which retry until the condition is met or the
  configured timeout elapses — this is the main defense against the flaky waits that
  AI-generated test code tends to reach for a fixed `sleep()` to paper over.
- **Data-driven tests read from `TestData/*.json` rather than hard-coded arrays**, so adding a
  new credential combination doesn't require touching test code.

---

## 10. Claude Code usage

Per the challenge requirements, this suite was developed using Claude as the primary AI pairing
tool. See [`claude-code-conversation-log.md`](claude-code-conversation-log.md) for the prompt/response
log, including where AI-generated locators/waits needed manual correction (e.g. fixing a
`GetByRole` overload name, tightening the `error_user` edge case to not over-assert on an
implementation detail, and reworking the empty-cart test to check computed totals instead of a
hard-coded price so it doesn't rot if SauceDemo's catalogue prices change).
