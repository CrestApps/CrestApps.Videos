using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private const string PlanEdit = "/Admin/Contents/ContentItems/commercestartermembership00/Edit";

    private static async Task DumpAsync(Scene scene, string label, int from = 0, int length = 900)
    {
        var text = (await scene.Page.Locator("body").InnerTextAsync()).Replace("\n", " | ");
        Console.WriteLine(label + ": " + scene.Page.Url + " :: " + text.Substring(Math.Min(from, text.Length), Math.Min(length, Math.Max(0, text.Length - from))));
    }

    // ----- Clips -----

    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/Edit/MembershipPlan");
        await scene.StartCaptureAsync();

        await scene.StepAsync("plan-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add Parts" }).First);
            await scene.Page.Locator("label").Filter(new() { HasTextString = "Tenant Onboarding" }).First.ClickAsync();
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Tenant Onboarding" }).First, "Tenant Onboarding");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#add-selected-parts-button"));
        });

        await scene.StepAsync("plan-2", async () =>
        {
            await scene.NavigateAsync(PlanEdit);
            await scene.Page.Locator("#TenantOnboardingPart_RecipeName").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("#TenantOnboardingPart_RecipeName"), "The recipe the new site is set up from");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.Page.Locator("#TenantOnboardingPart_RecipeName").SelectOptionAsync(new SelectOptionValue { Label = "Blog" });
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Publish'], button:has-text('Publish')").First);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/subscriptions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("db-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("Tenant Onboarding", new() { Exact = true }).First, pauseAfter: 1500);
            await scene.HighlightAsync(Field(scene, "ISite_SubscriptionOnboardingSettings_DatabaseProvider"), "Database for new sites");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
        });
    }

    private static async Task BuyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/plans");
        await scene.StartCaptureAsync();

        await scene.StepAsync("buy-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).Or(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" })).First, "Sign up");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).Or(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" })).First, pauseAfter: 3000);
            await scene.HighlightAsync(scene.Page.GetByText("Site name", new() { Exact = true }).First, "Your new site");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
        });

        await scene.StepAsync("buy-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "CheckoutFlow_TenantName"), "contoso_blog", delay: 50);
            await scene.TypeAsync(Field(scene, "CheckoutFlow_TenantTitle"), "Contoso Blog", delay: 50);
            await scene.TypeAsync(Field(scene, "CheckoutFlow_Prefix"), "blog", delay: 60);
            await scene.HighlightAsync(Field(scene, "CheckoutFlow_Prefix"), "The part of the address after your domain");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "CheckoutFlow_Domains"), "Optional custom domains");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "CheckoutFlow_RecipeName"), "Site template");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
        });

        await scene.StepAsync("buy-3", async () =>
        {
            await scene.TypeAsync(Field(scene, "CheckoutFlow_AdminUsername"), "siteadmin", delay: 60);
            await scene.TypeAsync(Field(scene, "CheckoutFlow_AdminEmail"), "jamie.chen@example.com", delay: 40);
            await scene.TypeAsync(Field(scene, "CheckoutFlow_AdminPassword"), "Password1!", delay: 60);
            await scene.TypeAsync(Field(scene, "CheckoutFlow_ConfirmPassword"), "Password1!", delay: 60);
            await scene.HighlightAsync(scene.Page.GetByText("Site administrator").First, "The administrator of the new site");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
        });

        await scene.StepAsync("buy-4", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("Pay Later", new() { Exact = true }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Pay now" }).Or(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Confirm" })).First);
            await scene.PauseAsync(2000);
        });
    }

    private static async Task SitesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/Admin/my-sites");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sites-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item, .card").Filter(new() { HasTextString = "contoso_blog" }).First, "The site, with its status");
            await scene.PauseAsync(5000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("A new site usually takes a minute or two to appear").First, "A minute or two");
            await scene.PauseAsync(4000);
        });
    }

    private static async Task ProvisioningAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/provisioning");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prov-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "contoso_blog" }).First, "The job that creates the site");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator("a, button, input").Filter(new() { HasTextString = "Retry now" }).First, pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            await scene.PauseAsync(2500);
        });
    }

    private static async Task ReadyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/Admin/my-sites");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ready-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item, .card").Filter(new() { HasTextString = "contoso_blog" }).First, "Ready");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Open" }).First, "Open");
            await scene.PauseAsync(3500);
        });
    }
}
