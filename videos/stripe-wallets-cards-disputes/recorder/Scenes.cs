using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Stripe video, one method per "clip" of storyboard.json. Nothing is sent to Stripe: the keys typed are made up.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-keys", KeysAsync),
        ("05-cards", CardsAsync),
    ];

    /// <summary>Nothing to prepare: the site is the one the Shipping video prepared, with the Stripe feature on.</summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task KeysAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/stripe");
        await scene.StartCaptureAsync();

        await scene.StepAsync("key-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/stripe"), "Settings > Payments > Stripe", 2200);
            await scene.HighlightAsync(Field(scene, "Test Publishable Key"), "Test keys first");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("key-2", async () =>
        {
            await ReplaceAsync(scene, Fld(scene, "ISite.StripeSettings.TestPublishableKey"), "pk_test_demo_not_a_real_key", 40);
            await ReplaceAsync(scene, Fld(scene, "ISite.StripeSettings.TestPrivateSecret"), "sk_test_demo_not_a_real_key", 40);
            await scene.ClickAsync(SaveButton(scene), pauseAfter: 500);
            await scene.OffCameraAsync(() => scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle));
            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(2500));
        });

        await scene.StepAsync("key-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Verifies the saved key").First.Locator("xpath=ancestor::div[1]"), "Status: Connect");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Enable Production"), "Enable Production");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("summary").First, "Advanced: the webhook signing secret");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CardsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/account/payment-methods");
        await scene.StartCaptureAsync();

        await scene.StepAsync("crd-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1, h2").First.Locator("xpath=.."), "My account: Payment methods");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("crd-2", async () =>
        {
            await scene.PauseAsync(5000);
        });

        await scene.StepAsync("crd-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Payment methods" }).First, "Payment methods");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();
    }
}
