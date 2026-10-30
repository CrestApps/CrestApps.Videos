using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task ModesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("mod-1", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_TargetType")), "Route to");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("mod-2", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_TargetQueueId")), "Target queue");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_DistributionMode")), "Queue distribution");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("mod-3", async () =>
        {
            await scene.NavigateAsync("/Admin/messaging");
            await scene.Page.WaitForSelectorAsync("#messaging-available-toggle", new() { Timeout = 30000 });
            await scene.HighlightAsync(scene.Page.Locator("#messaging-available-toggle").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Available");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PersonAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.ClickAsync(Conversation(scene, "0116"), pauseAfter: 1200);
        await scene.StartCaptureAsync();

        await scene.StepAsync("per-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[title^='Claim']").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("button[title^='Hand']").First, pauseAfter: 1200);
        });

        await scene.StepAsync("per-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".modal.show label:has-text('A person')").First, "A person");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".modal.show button:has-text('Select a person')").First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .dropdown-menu.show, .modal.show .popover").First, "Only people who can use the workspace");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".modal.show .dropdown-menu.show, .modal.show .popover").Locator("text=jamie.chen").First, pauseAfter: 800);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("per-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#messaging-transfer-note"), "Wants to book a bike fitting. Please offer Saturday morning.", delay: 35);
            await scene.HighlightAsync(scene.Page.Locator("#messaging-transfer-note"), "Note for the recipient");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("per-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".modal.show button:has-text('Transfer')").Last, pauseAfter: 3000);
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.Locator("text=Transferred from").First, "Transferred");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task QueueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.ClickAsync(Conversation(scene, "0117"), pauseAfter: 1200);
        await scene.StartCaptureAsync();

        await scene.StepAsync("que-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[title^='Claim']").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("button[title^='Hand']").First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator(".modal.show label:has-text('Back to a queue')").First, pauseAfter: 1200);
        });

        await scene.StepAsync("que-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".modal.show button:has-text('Select a queue')").First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator(".modal.show .dropdown-menu.show, .modal.show .popover").Locator("text=Support").First, pauseAfter: 800);
            await scene.TypeAsync(scene.Page.Locator("#messaging-transfer-note"), "The customer will call back tomorrow.", delay: 35);
            await scene.ClickAsync(scene.Page.Locator(".modal.show button:has-text('Transfer')").Last, pauseAfter: 3000);
        });

        await scene.StepAsync("que-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("text=Transferred from").First, "Back in the queue");
        });

        await scene.ClearHighlightsAsync();
    }
}
