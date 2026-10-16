using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Profile Builder video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    public const string ProfileTitle = "Contoso Dental (SMS)";

    // The instructions the session starts from. The price list makes the first reply quote a price, which is the
    // behavior the video corrects.
    public const string SystemMessage = """
        You are Riley, the text message assistant for Contoso Dental. You help patients book cleanings and checkups.

        Office hours: Monday to Friday, 8 AM to 5 PM.
        Prices without insurance: cleaning $120, checkup $95, X-rays $60.

        Be friendly and helpful.
        """;

    public const string OpeningMessage = "Greet the patient by first name, say you're Riley from Contoso Dental, and ask whether they'd like to book their six-month cleaning.";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-start", StartAsync),
        ("04-feedback", FeedbackAsync),
        ("05-replay", ReplayAsync),
        ("06-finish", FinishAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // ----- Preparing the site (not recorded) -----

    /// <summary>
    /// Enables the features, sets the default deployments, and creates the AI profile the video improves.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var feature in new[] { "CrestApps.OrchardCore.OpenAI.Azure", "CrestApps.OrchardCore.AI.Chat.Interactions.ProfileBuilder" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            var enable = page.Locator($"a[href$=\"/{feature}/Enable\"]");

            if (await enable.CountAsync() > 0)
            {
                await page.RunAndWaitForNavigationAsync(() => enable.First.ClickAsync());
                Console.WriteLine($"Enabled {feature}");
            }
        }

        await page.GotoAsync(BaseUrl + "/Admin/Settings/ai-settings");
        await page.Locator("select[name$='DefaultChatDeploymentName']").SelectOptionAsync(new SelectOptionValue { Label = "gpt-4.1-mini" });
        await page.Locator("select[name$='DefaultUtilityDeploymentName']").SelectOptionAsync(new SelectOptionValue { Label = "gpt-4.1-mini" });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button[type=submit].save, button[name='submit.Save']").First.ClickAsync());
        Console.WriteLine("Saved the default deployments");

        // The profile is made with the builder itself: a session with the instructions, saved as a new profile, then
        // the session deleted so the list starts empty.
        await page.GotoAsync(BaseUrl + "/Admin/ai/profile-builder");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button.create").ClickAsync());
        await WaitForChatAsync(page);
        await page.Locator("#ChatInteraction_ChatDeploymentName").SelectOptionAsync(new SelectOptionValue { Label = "gpt-4.1-mini" });
        var title = page.Locator("input[data-setting='title']");
        await title.FillAsync(ProfileTitle);
        await title.DispatchEventAsync("change");
        var system = page.Locator("textarea[data-setting='systemMessage']");
        await system.FillAsync(SystemMessage.Trim());
        await system.BlurAsync();
        var temperature = page.Locator("input[data-setting='temperature']");
        await temperature.FillAsync("0.7");
        await temperature.DispatchEventAsync("change");
        await page.WaitForTimeoutAsync(1500);
        await page.EvaluateAsync("() => document.querySelector('[data-chat-interaction-config]').__chatInteractionApp.flushPendingSave()");
        await page.WaitForTimeoutAsync(1500);

        var result = await page.EvaluateAsync<string>("""
            async ({ opening, title }) => {
              const config = JSON.parse(document.getElementById('profile-builder-panel').dataset.profileBuilderConfig);
              const post = (url, body) => fetch(url, { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': config.antiforgeryToken }, body: JSON.stringify(body) }).then(r => r.json());
              await post(config.urls.settings, { initialPrompt: opening, coachDeploymentName: '' });
              const saved = await post(config.urls.saveProfile, { updateExisting: false, name: 'ContosoDentalSms', displayText: title });
              const deleteUrl = config.urls.state.replace('/State/', '/Delete/');
              await fetch(deleteUrl, { method: 'POST', credentials: 'same-origin', headers: { 'RequestVerificationToken': config.antiforgeryToken } });
              return JSON.stringify(saved);
            }
            """, new { opening = OpeningMessage, title = ProfileTitle });

        Console.WriteLine($"Created the profile: {result}");
        await context.CloseAsync();
    }

    private static Task WaitForChatAsync(IPage page)
        => page.WaitForFunctionAsync("() => { const a = document.querySelector('[data-chat-interaction-config]')?.__chatInteractionApp; return a && a.connection && a.connection.state === 'Connected'; }");

    // ----- Helpers -----

    private static ILocator Panel(Scene scene) => scene.Page.Locator("#profile-builder-panel");

    private static ILocator LastReply(Scene scene) => scene.Page.Locator(".ai-chat-message-item").Last;

    // Opens the newest session, before the capture starts.
    private static async Task OpenSessionAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/ai/profile-builder");
        var href = await scene.Page.Locator("#profile-builder-sessions-list a").Filter(new() { HasTextString = "Open" }).First.GetAttributeAsync("href");
        await scene.OpenAsync(href.Replace(Scenes.BaseUrl, string.Empty));
        await CompactMenuAsync(scene);
        await WaitForChatAsync(scene.Page);
        await scene.PauseAsync(800);
    }

    private static async Task CompactMenuAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("() => { if (!document.body.classList.contains('left-sidebar-compact')) document.querySelector('.leftbar-compactor')?.click(); }");
        await Task.Delay(400);
    }

    private static Task WaitForReplyAsync(Scene scene, int expectedMessages)
        => scene.Page.WaitForFunctionAsync(
            """
            (count) => {
              const a = document.querySelector('[data-chat-interaction-config]').__chatInteractionApp;
              const last = a.messages[a.messages.length - 1];
              return a.messages.length === count && last.role === 'assistant' && !last.isStreaming && !!last.content
                && !!document.querySelector('.ai-chat-message-item:last-child .profile-builder-action');
            }
            """,
            expectedMessages,
            new PageWaitForFunctionOptions { Timeout = 120_000 });

    // ----- Clips -----

    private static async Task StartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profile-builder");

        // The menu is opened on Artificial Intelligence, so the item is visible from the start.
        await scene.Page.EvaluateAsync("() => { const link = document.querySelector('a[href$=\"/Admin/ai/profile-builder\"]'); link?.scrollIntoView({ block: 'center' }); }");
        await scene.StartCaptureAsync();

        var menuItem = scene.Page.Locator("#left-nav a[href$='/Admin/ai/profile-builder'], .left-sidebar a[href$='/Admin/ai/profile-builder']").First;

        await scene.StepAsync("start-1", async () =>
        {
            await scene.MoveToAsync(menuItem);
            await scene.HighlightAsync(menuItem, "Artificial Intelligence > Profile Builder");
            await scene.PauseAsync(2600);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#profile-builder-sessions-list"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("start-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".action-bar .btn-group").First, "New Session");
            await scene.PauseAsync(2400);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".dropdown-toggle-split"), pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show button").Filter(new() { HasTextString = "Improve an existing AI profile" }), pauseAfter: 900);
        });

        await scene.StepAsync("start-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#pb-profile-id"), ProfileTitle, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#pb-from-profile-modal button[type=submit]"));
            await WaitForChatAsync(scene.Page);
        });

        await scene.StepAsync("start-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".chat-parameters-panel"), "Settings");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("start-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".chat-content-area"), "Role-play");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("start-6", async () =>
        {
            await scene.HighlightAsync(Panel(scene), "Builder");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FeedbackAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenSessionAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("fb-1", async () =>
        {
            var input = scene.Page.Locator(".chat-user-prompt-input");
            await scene.TypeAsync(input, "Hi! Can I book a cleaning next week? How much is it without insurance?", delay: 35);
            await input.PressAsync("Enter");
            await WaitForReplyAsync(scene, 2);
        });

        await scene.StepAsync("fb-2", async () =>
        {
            await scene.HighlightAsync(LastReply(scene).Locator(".ai-chat-message-body > div").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fb-3", async () =>
        {
            await scene.ClickAsync(LastReply(scene).Locator(".profile-builder-action[data-builder-action=feedback]"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("#pb-selected-reply"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fb-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#pb-comment"), "Don't quote prices by text. Say the front desk confirms the cost when they confirm the visit, then ask which day works.", delay: 25);
        });

        await scene.StepAsync("fb-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#pb-send-regenerate"), pauseAfter: 200);

            // The coach rewrites the instructions, then the reply streams again: cut the wait.
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.Locator("#pb-status").Filter(new() { HasTextString = "Regenerating" }).WaitForAsync(new() { Timeout = 120_000 });
                await WaitForReplyAsync(scene, 2);
            }, leadMilliseconds: 1200);
        });

        await scene.StepAsync("fb-6", async () =>
        {
            var system = scene.Page.Locator("textarea[data-setting='systemMessage']");
            await system.ScrollIntoViewIfNeededAsync();
            await system.EvaluateAsync("element => { element.rows = 14; }");
            await scene.HighlightAsync(system, "Rewritten instructions");
            await scene.PauseAsync(3200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LastReply(scene).Locator(".ai-chat-message-body > div").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fb-7", async () =>
        {
            await scene.ClickAsync(LastReply(scene).Locator(".profile-builder-action[data-builder-action=approve]"), pauseAfter: 900);
            await scene.Page.Locator("#pb-status").Filter(new() { HasTextString = "Marked as a good reply" }).WaitForAsync();
        });

        await scene.StepAsync("fb-8", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#pb-feedback-list"), "Feedback so far");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReplayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenSessionAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("rev-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#pb-tab-revisions"), pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator("#pb-revision-list .profile-builder-item").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rev-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#pb-revision-list details summary").First, pauseAfter: 1600);
            await scene.ScrollAsync(0, 0);
            var restore = scene.Page.Locator("#pb-revision-list [data-builder-restore]").First;
            await scene.MoveToAsync(restore);
            await scene.HighlightAsync(restore, "Restore");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rep-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#pb-tab-replay"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator("#pb-replay"), pauseAfter: 200);
            await scene.OffCameraAsync(
                () => scene.Page.Locator("#pb-replay-results .profile-builder-item").First.WaitForAsync(new() { Timeout = 180_000 }),
                leadMilliseconds: 1500);
        });

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#pb-replay-results"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FinishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenSessionAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("fin-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#pb-tab-finish"), pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator("#pb-initial-prompt"), "Opening message");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fin-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#pb-coach-deployment"), "Coach deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fin-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#pb-copy-system-message"));
            await scene.PauseAsync(1400);
            await scene.ClearHighlightsAsync();
            var create = scene.Page.Locator("#pb-create-profile").Locator("xpath=..");
            await create.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(create);
            await scene.PauseAsync(1600);
            await scene.ClearHighlightsAsync();
            var update = scene.Page.Locator("#pb-update-profile");
            await update.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(update);
            await scene.PauseAsync(500);
            await scene.WaitForNavigationAsync(() => update.ClickAsync());
        });

        await scene.StepAsync("fin-4", async () =>
        {
            await scene.PauseAsync(600);
            var editor = scene.Page.Locator(".CodeMirror >> visible=true").First;

            if (await editor.CountAsync() > 0)
            {
                await editor.ScrollIntoViewIfNeededAsync();
                await scene.HighlightAsync(editor, "The instructions you built");
            }
        });

        await scene.ClearHighlightsAsync();
    }
}
