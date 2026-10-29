using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private const string PlanPath = "/Admin/Contents/ContentItems/4mmbrplan0annual000000000a/Edit";

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task SaveAsync(Scene scene, ILocator button)
    {
        await scene.ClickAsync(button, pauseAfter: 500);

        var confirm = scene.Page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First;

        try
        {
            await confirm.WaitForAsync(new() { Timeout = 2500 });
            await scene.ClickAsync(confirm, pauseAfter: 500);
        }
        catch (TimeoutException)
        {
        }

        await scene.OffCameraAsync(() => scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle));
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(2500));
    }

    // Signs the demo customer in on the scene's page, off camera.
    private static async Task SignedInAsync(Scene scene)
    {
        await scene.OffCameraAsync(async () =>
        {
            await scene.Page.GotoAsync(BaseUrl + "/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.Page.Locator("#LoginForm_UserName").FillAsync("jamie");
            await scene.Page.Locator("#LoginForm_Password").FillAsync("Password1!");
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#LoginForm_Password").PressAsync("Enter"), new() { WaitUntil = WaitUntilState.NetworkIdle });
        });
    }

    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(PlanPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pln-1", async () =>
        {
            var duration = Fld(scene, "SubscriptionPart.BillingDuration");
            await duration.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(duration.Locator("xpath=ancestor::div[contains(@class,'ocat') or contains(@class,'card') or contains(@class,'mb-3')][last()]"), "Billing duration, cycles and trial days");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pln-2", async () =>
        {
            var roles = Fld(scene, "SubscriptionEntitlementPart.Roles[0].IsSelected");
            await roles.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(roles.Locator("xpath=ancestor::div[contains(@class,'ocat') or contains(@class,'card') or contains(@class,'mb-3')][last()]"), "Member access");
            await scene.PauseAsync(5000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/subscription-agreements");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mng-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/subscription-agreements"), "Subscriptions > Agreements", 2200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('Manage')").First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("mng-2", async () =>
        {
            var pause = scene.Page.Locator("button").Filter(new() { HasTextString = "Pause billing" }).First;
            await pause.ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(pause.Locator("xpath=ancestor::form[1]").Locator("[name='reason']"), "Away for the holidays", delay: 45);
            await SaveAsync(scene, pause);
            await scene.HighlightAsync(scene.Page.GetByText("Paused").First, "Paused");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Resume" }).First);
        });

        await scene.StepAsync("mng-3", async () =>
        {
            var cancel = scene.Page.Locator("button").Filter(new() { HasTextString = "Cancel subscription" }).First;
            await cancel.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(Fld(scene, "atPeriodEnd").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Keep access until the paid period ends");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(cancel.Locator("xpath=ancestor::form[1]").Locator("[name='reason']"), "Moving abroad", delay: 45);
            await SaveAsync(scene, cancel);
            var history = scene.Page.GetByText("History").First;
            await history.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(history.Locator("xpath=ancestor::div[2]"), "History");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task MyPlansAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await SignedInAsync(scene);
        await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/Admin/my-subscriptions", new() { WaitUntil = WaitUntilState.NetworkIdle }));
        await scene.StartCaptureAsync();

        await scene.StepAsync("mpl-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li, tr, .card").Filter(new() { HasTextString = "Annual Membership" }).Last, "My plans");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("mpl-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Cancel" }).First, "Cancel a plan");
            await scene.PauseAsync(4500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task MembershipsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/memberships");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mem-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/memberships"), "Commerce > Memberships", 2200);
            await scene.HighlightAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Active" }).First, "Active, Expired, Ended early");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("mem-2", async () =>
        {
            var end = scene.Page.Locator("button, a").Filter(new() { HasTextString = "End now" }).First;
            await scene.HighlightAsync(end, "End now");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, end);
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }
}
