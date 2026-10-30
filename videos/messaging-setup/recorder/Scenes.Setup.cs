using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // The wrapper of a form field, found by the start of its label.
    private static ILocator LabelBox(Scene scene, string label)
        => scene.Page.Locator($"xpath=//label[starts-with(normalize-space(),'{label}')]/parent::*").First;

    // The row of a permission, found by the id of its checkbox.
    private static ILocator PermissionRow(Scene scene, string permission)
        => scene.Page.Locator($"tr:has(input[id='Checkbox.{permission}'])").First;

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        var search = scene.Page.Locator("input[type=search], input[placeholder*='earch']").First;

        await scene.StepAsync("fea-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Features"), "Tools > Features");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(search, "SMS Messaging", delay: 80);
            await scene.PauseAsync(1800);
            await scene.HighlightAsync(scene.Page.GetByText("SMS Messaging Channel", new() { Exact = true }).First, "SMS Messaging Channel");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fea-2", async () =>
        {
            await search.FillAsync(string.Empty);
            await scene.TypeAsync(search, "Routed Distribution", delay: 70);
            await scene.PauseAsync(1800);
            await scene.HighlightAsync(scene.Page.GetByText("Omnichannel Messaging Routed Distribution", new() { Exact = true }).First, "Optional");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProviderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/sms");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pro-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/sms"), "Settings > Communication > SMS");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Default SMS provider"), "Default SMS provider");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pro-2", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Twilio"), pauseAfter: 1000);
            await scene.HighlightAsync(LabelBox(scene, "Enable Twilio Provider"), "Enable Twilio Provider");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Phone Number"), "Phone Number");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Account SID"), "Account SID");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Auth Token"), "Auth Token");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pro-3", async () =>
        {
            await scene.HighlightAsync(LabelBox(scene, "Webhook URL"), "Webhook URL");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/sms/test");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tst-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/sms/test"), "Tools > Testing > SMS Test");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#Provider").SelectOptionAsync(new SelectOptionValue { Label = "Log" });
            await scene.HighlightAsync(LabelBox(scene, "Provider"), "Provider");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#PhoneNumber"), "+14155550134", delay: 80);
            await scene.HighlightAsync(LabelBox(scene, "Phone Number"), "Phone Number");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Send')").First, pauseAfter: 1500);
            await scene.PauseAsync(3000);
        });
    }

    private static async Task NumbersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/channel-endpoints");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Support line").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("num-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/channel-endpoints"), "Omnichannel Addresses");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Phone number"), "Phone number");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label:has-text('Text messages (SMS)')").First.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Text messages");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("num-2", async () =>
        {
            await ScrollToAsync(scene, LabelBox(scene, "Provider"), 300);
            await scene.HighlightAsync(LabelBox(scene, "Provider"), "Provider");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("num-3", async () =>
        {
            await ScrollToAsync(scene, LabelBox(scene, "Agents who text"), 300);
            await scene.HighlightAsync(LabelBox(scene, "Agents who text"), "Agents who text from this number");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EntryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Support texts").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("ent-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/contact-center/entry-points"), "Inbound entry points");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Numbers"), "Numbers");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, LabelBox(scene, "Route to"), 300);
            await scene.HighlightAsync(LabelBox(scene, "Route to"), "Route to");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ent-2", async () =>
        {
            await scene.HighlightAsync(LabelBox(scene, "Target queue"), "Target queue");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Queue distribution"), "Queue distribution");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ent-3", async () =>
        {
            await ScrollToAsync(scene, LabelBox(scene, "Auto-reply"), 300);
            await scene.HighlightAsync(LabelBox(scene, "Auto-reply"), "Auto-reply");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Business hours calendar"), "Business hours calendar");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, LabelBox(scene, "Closed auto-reply"), 300);
            await scene.HighlightAsync(LabelBox(scene, "Closed auto-reply"), "Closed auto-reply");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Messaging agent").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("rol-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Roles/Index"), "Access Control > Roles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, PermissionRow(scene, "UseMessagingWorkspace"), 300);
            await scene.HighlightAsync(PermissionRow(scene, "UseMessagingWorkspace"), "Use the messaging workspace");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow(scene, "ViewOwnMessagingConversations"), "Own conversations");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow(scene, "ViewQueueMessagingConversations"), "Own and queues' unclaimed");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow(scene, "ViewAllMessagingConversations"), "All conversations");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rol-2", async () =>
        {
            await scene.HighlightAsync(PermissionRow(scene, "ManageMessaging"), "Manage the messaging workspace");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, PermissionRow(scene, "SendGroupMessages"), 300);
            await scene.HighlightAsync(PermissionRow(scene, "SendGroupMessages"), "Send group messages");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BaseUrlAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/general");
        await scene.StartCaptureAsync();

        await scene.StepAsync("bas-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/general"), "Settings > General");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Base url"), "Base URL");
        });

        await scene.ClearHighlightsAsync();
    }
}
