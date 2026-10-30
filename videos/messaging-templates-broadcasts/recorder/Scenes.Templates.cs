using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task TemplateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/messaging/templates");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tem-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/messaging/templates"), "Messaging > Templates");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var add = scene.Page.Locator("a:has-text('Add template'), button:has-text('Add template')").First;
            await scene.HighlightAsync(add, "Add template");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(add);
        });

        await scene.StepAsync("tem-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#MessageTemplate_Name"), "Opening hours", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("#MessageTemplate_Name"), "Name");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#MessageTemplate_Body"), "We are open Monday to Saturday from 9:00 to 18:00, and closed on Sundays.", delay: 28);
            await scene.HighlightAsync(scene.Page.Locator("#MessageTemplate_Body"), "Body");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("tem-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(Row(scene, "Opening hours"), "Your template");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task InsertAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.ClickAsync(Conversation(scene, "Casey Dunn"), pauseAfter: 1500);
        await scene.StartCaptureAsync();

        var canned = scene.Page.Locator("select").Filter(new() { HasTextString = "canned response" }).First;

        await scene.StepAsync("ins-1", async () =>
        {
            await scene.HighlightAsync(canned, "Insert a canned response");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ins-2", async () =>
        {
            await canned.SelectOptionAsync(new SelectOptionValue { Label = "Opening hours" });
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("textarea[placeholder^='Reply']").First, "A draft you can change");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task StopAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.ClickAsync(Conversation(scene, "Emery Fox"), pauseAfter: 1500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("stp-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("text=This conversation is closed").First, "Closed");
        });

        await scene.ClearHighlightsAsync();
    }
}
