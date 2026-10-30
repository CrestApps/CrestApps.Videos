using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    // ----- Clips -----

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("feat-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "API", delay: 120);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(Row(scene, "Cart API"), "Cart API");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Checkout API"), "Checkout API");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Orders API"), "Orders API");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Customers API"), "Customers API");
        });

        await ClearAsync(scene);

        await scene.StepAsync("feat-2", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Inventory API"), "Inventory API");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator("#btn-enable-CrestApps_OrchardCore_Inventory_Api"), pauseAfter: 6000);
        });
    }

    private static async Task ExplorersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "OpenAPI", delay: 110);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(Row(scene, "Swagger UI"), "Swagger UI");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "ReDoc UI"), "ReDoc UI, read-only");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator("#btn-enable-OrchardCore_OpenApi_ScalarUI"), pauseAfter: 6000);
        });

        await scene.StepAsync("exp-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/openapi");
            await scene.HighlightAsync(Field(scene, "ISite_OpenApiSettings_AllowAnonymousSchemaAccess"), "Allow anonymous access to the API schema");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
        });

        await scene.StepAsync("exp-3", async () =>
        {
            await scene.NavigateAsync("/swagger");
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByText("Account", new() { Exact = true }).First, "The commerce API, in groups");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
        });
    }

    private static async Task ServerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/OpenId/ServerConfiguration");
        await scene.StartCaptureAsync();

        await scene.StepAsync("srv-1", async () =>
        {
            await scene.HighlightAsync(Field(scene, "OpenIdServerSettings_EnableTokenEndpoint"), "Token endpoint");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "OpenIdServerSettings_EnableAuthorizationEndpoint"), "Authorization endpoint");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
        });

        await scene.StepAsync("srv-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "OpenIdServerSettings_AllowAuthorizationCodeFlow"), "Allow authorization code flow");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAsync(Field(scene, "OpenIdServerSettings_RequireProofKeyForCodeExchange"), pauseAfter: 700);
            await scene.HighlightAsync(Field(scene, "OpenIdServerSettings_RequireProofKeyForCodeExchange"), "Require proof key for code exchange (PKCE)");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAsync(Field(scene, "OpenIdServerSettings_AllowClientCredentialsFlow"), pauseAfter: 700);
            await scene.HighlightAsync(Field(scene, "OpenIdServerSettings_AllowClientCredentialsFlow"), "Allow client credentials flow");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "OpenIdServerSettings_AllowRefreshTokenFlow"), "Allow refresh token flow");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
        });

        await scene.StepAsync("srv-3", async () =>
        {
            await scene.NavigateAsync("/Admin/OpenId/ValidationConfiguration");
            await scene.HighlightAsync(Field(scene, "OpenIdValidationSettings_Tenant"), "The tenant whose tokens are accepted");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            Console.WriteLine("VALIDATION: " + string.Join(" | ", await scene.Page.Locator("#OpenIdValidationSettings_Tenant option").AllInnerTextsAsync()));
        });
    }

    private static async Task AppsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/OpenId/Application");
        await scene.StartCaptureAsync();

        await scene.StepAsync("app-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/OpenId/Application"), "Security > OpenID Connect > Applications");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.NavigateAsync("/Admin/OpenId/Application/Create");
            await scene.TypeAsync(Field(scene, "DisplayName"), "Contoso shop app", delay: 50);
            await Field(scene, "Type").SelectOptionAsync("public");
            await scene.PauseAsync(600);
            await scene.TypeAsync(Field(scene, "ClientId"), "contoso-shop-app", delay: 40);
            await scene.HighlightAsync(Field(scene, "Type"), "Public: an app can't keep a secret");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(Field(scene, "AllowAuthorizationCodeFlow"), pauseAfter: 600);
            await scene.ClickAsync(Field(scene, "RequireProofKeyForCodeExchange"), pauseAfter: 600);
            await scene.ClickAsync(Field(scene, "AllowRefreshTokenFlow"), pauseAfter: 600);
            await scene.TypeAsync(Field(scene, "RedirectUris"), "https://shop.contoso.example/signin-callback", delay: 30);
            Console.WriteLine("SCOPES: " + string.Join(" | ", await scene.Page.Locator("label[for^='ScopeEntries_']").AllInnerTextsAsync()) + " ROLES: " + string.Join(" | ", await scene.Page.Locator("label[for^='RoleEntries_']").AllInnerTextsAsync()));
        });

        await scene.StepAsync("app-2", async () =>
        {
            foreach (var scope in new[] { "profile", "roles" })
            {
                await scene.Page.Locator("label[for^='ScopeEntries_']").Filter(new() { HasTextString = scope }).First.ClickAsync();
                await scene.PauseAsync(500);
            }

            await scene.HighlightAsync(scene.Page.Locator("label[for^='ScopeEntries_']").Filter(new() { HasTextString = "roles" }).First, "The roles scope carries the person's roles");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
        });

        await scene.StepAsync("app-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles/Create");
            await scene.TypeAsync(Field(scene, "RoleName"), "Warehouse integration", delay: 50);
            await scene.TypeAsync(Field(scene, "RoleDescription"), "Ships orders and receives stock", delay: 35);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }));
        });

        await scene.StepAsync("app-4", async () =>
        {
            await scene.NavigateAsync("/Admin/OpenId/Application/Create");
            await scene.TypeAsync(Field(scene, "DisplayName"), "Warehouse server", delay: 50);
            await Field(scene, "Type").SelectOptionAsync("confidential");
            await scene.PauseAsync(700);
            await scene.TypeAsync(Field(scene, "ClientId"), "warehouse-server", delay: 40);
            await scene.TypeAsync(Field(scene, "ClientSecret"), "demo-secret-only-for-this-video", delay: 20);
            await scene.HighlightAsync(Field(scene, "ClientSecret"), "A strong client secret");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(Field(scene, "AllowClientCredentialsFlow"), pauseAfter: 600);
            await scene.Page.Locator("label[for^='RoleEntries_']").Filter(new() { HasTextString = "Warehouse integration" }).First.ClickAsync();
            await scene.HighlightAsync(scene.Page.Locator("label[for^='RoleEntries_']").Filter(new() { HasTextString = "Warehouse integration" }).First, "The role decides what it can do");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
        });
    }

    private static async Task CorsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Cors");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cors-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Cors"), "Security > Cross-Origin Resource Sharing");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add a policy" }), pauseAfter: 1500);
            await scene.PauseAsync(1500);
            await scene.Page.Locator("input[type=text]").First.FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("input[type=text]").First, "Contoso shop", delay: 50);
            await scene.PauseAsync(1500);
            await scene.TypeAsync(scene.Page.Locator("input[type=text]").Nth(1), "https://shop.contoso.example", delay: 30);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Origin" }), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.GetByText("Allowed origins").First, "Your storefront's address");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.PauseAsync(2000);
        });
    }
}
