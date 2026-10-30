using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // The wrapper of a form field of the broadcast page, found by its label.
    private static ILocator BroadcastField(Scene scene, string label)
        => scene.Page.Locator($"xpath=//label[contains(normalize-space(),'{label}')]/parent::*").First;

    private static async Task PickRecipientAsync(Scene scene, string search)
    {
        var box = scene.Page.Locator(".dropdown-menu.show input[type=search]").First;
        await box.FillAsync(string.Empty);
        await scene.TypeAsync(box, search, delay: 90);
        await scene.PauseAsync(1200);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a[role=option]").First, pauseAfter: 700);
    }

    private static async Task BroadcastAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/messaging/broadcasts");

        // The site sends a queued broadcast in a sweep that runs at the same second of every minute. Start the capture
        // so that the Queue click lands a few seconds before it, and the list can show Completed without a long wait.
        if (int.TryParse(Environment.GetEnvironmentVariable("VIDEO_SWEEP_SECOND"), out var sweep))
        {
            var target = ((sweep - 7) % 60 + 60) % 60;

            while (DateTime.Now.Second != target)
            {
                await Task.Delay(200);
            }
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("bro-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/messaging/broadcasts"), "Messaging > Broadcasts");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var add = scene.Page.Locator("a:has-text('New broadcast'), button:has-text('New broadcast')").First;
            await scene.HighlightAsync(add, "New broadcast");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(add);
        });

        await scene.StepAsync("bro-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Weekend sale", delay: 45);
            await scene.HighlightAsync(BroadcastField(scene, "Name"), "Name");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(BroadcastField(scene, "Send from"), "Send from");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("bro-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Search contacts')").First, pauseAfter: 800);
            await PickRecipientAsync(scene, "Avery");
            await PickRecipientAsync(scene, "Blake");
            await PickRecipientAsync(scene, "Emery");
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(BroadcastField(scene, "Recipients"), "Recipients");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("bro-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#RecipientsText"), "+14155550140", delay: 70);
            await scene.HighlightAsync(BroadcastField(scene, "Additional addresses"), "Additional addresses");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("bro-5", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Body"), "Weekend sale at Contoso Bikes: show this text in the shop for 10 percent off.", delay: 28);
            await scene.HighlightAsync(BroadcastField(scene, "Message"), "Message");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Queue broadcast')").First);
        });

        await scene.StepAsync("bro-6", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Weekend sale"), "Queued");
            await scene.PauseAsync(14000);
            await scene.ClearHighlightsAsync();

            for (var i = 0; i < 12; i++)
            {
                if (await Row(scene, "Weekend sale").InnerTextAsync() is { } text && text.Contains("Completed"))
                {
                    break;
                }

                await scene.PauseAsync(3000);
                await scene.NavigateAsync("/Admin/messaging/broadcasts");
            }

            await scene.HighlightAsync(Row(scene, "Weekend sale"), "Completed");
        });

        await scene.ClearHighlightsAsync();
    }
}
