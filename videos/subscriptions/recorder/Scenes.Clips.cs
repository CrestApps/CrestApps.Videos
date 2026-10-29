using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("plan-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/ContentTypes/List"), "Content Definition > Content Types", 2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/MembershipPlan']").First);
            await scene.HighlightAsync(Field(scene, "Stereotype"), "Stereotype: Subscription");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("plan-2", async () =>
        {
            await ScrollAsync(scene, 0, 900);
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "Subscription Entitlements" }).Filter(new() { HasNotTextString = "Full-text" }).Last, "Subscription, Product, Subscription Entitlements");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("plan-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/GiftCard,MembershipPlan,Product");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='commercestartermembership00/Edit']").First);
            await scene.HighlightAsync(Field(scene, "Billing duration"), "Billing duration");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("plan-4", async () =>
        {
            await ShowAsync(scene, Field(scene, "Initial amount description"), "Initial amount description", 2600);
            await scene.HighlightAsync(Field(scene, "Billing cycle limit"), "Billing cycle limit");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("plan-5", async () =>
        {
            await ShowAsync(scene, Field(scene, "Free trial days"), "Free trial days", 2600);
            await ShowAsync(scene, Field(scene, "Subscription day delay"), "Subscription day delay", 2200);
            await ScrollAsync(scene, 0, 1500);
            await scene.HighlightAsync(scene.Page.Locator("h5, h4, h3").Filter(new() { HasTextString = "Member access" }).First.Locator("xpath=.."), "Member access");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("plan-6", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='SubscriptionPart.TrialDays']"), "7", delay: 90);
            await scene.ClickAndWaitForNavigationAsync(Publish(scene));
        });
    }

    private static async Task SignupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/plans");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sign-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Sign up" }).First, "Price, billing period and Sign up");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Sign up'), a:has-text('Sign up')").First);
        });

        await scene.StepAsync("sign-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".checkout-stepper").First, "Steps");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();

            for (var guard = 0; guard < 3 && !(await scene.Page.Locator("text=Subscribe as guest").CountAsync() > 0); guard++)
            {
                await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
            }

            await scene.ClickAsync(scene.Page.GetByLabel("Subscribe as guest"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.GetByLabel("Subscribe as guest"), "Subscribe as guest");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sign-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
            await ShowAsync(scene, scene.Page.Locator("div").Filter(new() { HasTextString = "Payment method" }).Filter(new() { HasNotTextString = "Order summary" }).Last, "Payment method", 2600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
        });
    }

    private static async Task AgreementsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/subscription-agreements");
        await scene.StartCaptureAsync();

        await scene.StepAsync("agr-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/subscription-agreements"), "Subscriptions > Agreements", 2200);
            await scene.HighlightAsync(Row(scene, "Annual Membership"), "Plan, status, method, amount, renewal");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agr-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Any status" }).First, pauseAfter: 2500);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("agr-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Manage"));
            await ShowAsync(scene, scene.Page.Locator("div").Filter(new() { HasTextString = "Cancel subscription" }).Filter(new() { HasNotTextString = "History" }).Last, "Actions", 3500);
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "History" }).Last, "History");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/subscriptions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/subscriptions"), "Settings > Subscriptions", 2400);
            await scene.HighlightAsync(Field(scene, "Allow guest sign-up"), "Allow guest sign-up");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-2", async () =>
        {
            await ShowAsync(scene, Field(scene, "Currency"), "Currency", 2600);
            await scene.HighlightAsync(scene.Page.Locator("[name='ISite.SubscriptionRoleSettings.Roles[0].IsSelected']").First.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'card') or contains(@class,'mb-3')][1]"), "Default roles");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task MembershipsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/4mmbrplan0annual000000000a/Edit");
        await ScrollAsync(scene, 0, 3000);
        await scene.StartCaptureAsync();

        await scene.StepAsync("mem-1", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Days a membership lasts"), "Days a membership lasts");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("[name='MembershipPart.RoleNames']").First.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'card') or contains(@class,'mb-3')][1]"), "Roles members are in");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("mem-2", async () =>
        {
            await scene.NavigateAsync("/Admin/memberships");
            await ShowAsync(scene, Menu(scene, "/Admin/memberships"), "Commerce > Memberships", 2000);
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Every membership" }).First, pauseAfter: 2200);
            await scene.Page.Keyboard.PressAsync("Escape");
        });
    }
}
