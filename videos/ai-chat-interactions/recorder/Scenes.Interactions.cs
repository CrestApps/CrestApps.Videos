using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers of the interaction clips -----

    // One setting of the panel beside the chat, found by its label.
    private static ILocator Setting(Scene scene, string label)
        => scene.Page.Locator($"xpath=//div[contains(@class,'mb-3')][./label[contains(normalize-space(),'{label}')]]").First;

    private static ILocator Prompt(Scene scene) => scene.Page.Locator("#ChatInteraction_UserPrompt");

    private static ILocator Send(Scene scene) => scene.Page.Locator("#ChatInteraction_SendButton");

    // Sends a message of the interaction, and lets the answer be written.
    private static async Task AskAsync(Scene scene, string question)
    {
        await scene.TypeAsync(Prompt(scene), question, delay: 32);
        await scene.ClickAsync(Send(scene), pauseAfter: 300);
        await scene.Page.WaitForSelectorAsync("#ChatInteraction_SendButton .fa-stop", new() { Timeout = 15000 });
        await scene.Page.WaitForSelectorAsync("#ChatInteraction_SendButton .fa-arrow-up", new() { Timeout = 60000 });
        await scene.PauseAsync(600);
    }

    // ----- Clips -----

    private static async Task StartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/chat-interactions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("start-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/chat-interactions"), "Artificial Intelligence > Chat Interactions");
            await scene.PauseAsync(2400);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('New Chat')").First, "New Chat");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('New Chat')").First);
        });

        await scene.StepAsync("start-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".nav-tabs").First, "Settings");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#ChatInteraction_Title"), "Draft a product description", delay: 35);
            await scene.HighlightAsync(Setting(scene, "Title"), "Title");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("start-3", async () =>
        {
            await AskAsync(scene, "Write a short description of a city bike for our website.");
        });

        await scene.StepAsync("start-4", async () =>
        {
            var collapse = scene.Page.Locator("button.chat-panel-toggle").First;
            await scene.HighlightAsync(collapse, "Collapse");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(collapse, pauseAfter: 1500);
            var show = scene.Page.Locator("button:has-text('Configurations'), a:has-text('Configurations')").First;
            await scene.HighlightAsync(show, "Configurations");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(show, pauseAfter: 1000);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // Reached from the list, so the menu shows Chat Interactions as the current item.
        await scene.OpenAsync("/Admin/ai/chat-interactions");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Draft a product description").Locator("a:has-text('Chat')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HighlightAsync(Setting(scene, "Orchestrator"), "Orchestrator");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-2", async () =>
        {
            await scene.HighlightAsync(Setting(scene, "Chat deployment"), "Chat deployment");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Setting(scene, "Utility deployment"), "Utility deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-3", async () =>
        {
            await scene.HighlightAsync(Setting(scene, "AI persona"), "AI persona");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ChatInteraction_SystemMessage"), "You are a copy editor. Answer in British English, in one short sentence.", delay: 28);
            await scene.HighlightAsync(Setting(scene, "System instructions"), "System instructions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-5", async () =>
        {
            await scene.HighlightAsync(Setting(scene, "Prompt templates"), "Prompt templates");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, scene.Page.Locator("#ChatInteraction_MaxTokens"), 300);
            await scene.HighlightAsync(Setting(scene, "Max response"), "Max response");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Setting(scene, "Temperature"), "Temperature");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Setting(scene, "Past messages"), "Past messages");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-6", async () =>
        {
            await AskAsync(scene, "Rewrite: Our bikes are the best in the city.");
        });
    }

    private static async Task CapabilitiesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/chat-interactions");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Draft a product description").Locator("a:has-text('Chat')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("cap-1", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Capabilities"), pauseAfter: 1200);
            await scene.HighlightAsync(Tab(scene, "Capabilities"), "Capabilities");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cap-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#selectAllTools").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Select All Tools");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[placeholder='Search tools...']"), "Search tools");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("input[placeholder='Search tools...']"), "current date", delay: 90);
            await scene.PauseAsync(1200);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cap-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("input[id^='tool_Utilities']").First, pauseAfter: 900);
            await AskAsync(scene, "What day of the week is it today?");
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/chat-interactions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("man-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Draft a product description"), "Your interaction");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box, input[type=search]").First, "product", delay: 80);
            await scene.HighlightAsync(scene.Page.Locator("button[title='Search'], button[aria-label='Search']").First, "Search");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#search-box, input[type=search]").First.FillAsync(string.Empty);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Draft a product description").Locator("a:has-text('Chat')").First);
        });

        await scene.StepAsync("man-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('New Chat')").First, pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "Empty Chat, or Chat with Preset Settings");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("man-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Clear History'), a:has-text('Clear History')").First, "Clear History");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Clear History'), a:has-text('Clear History')").First, pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show, .swal2-popup, .bootbox").First, "Asks you to confirm");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("button:has-text('Cancel')").First.ClickAsync();
        });

        await scene.StepAsync("man-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('Chat Interactions')").Last);
            await scene.ClickAsync(RowActions(scene, "Repair checklist"), pauseAfter: 800);
            await scene.HighlightAsync(Row(scene, "Repair checklist").Locator(".dropdown-menu").First, "Delete");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(Row(scene, "Repair checklist").Locator("input[type=checkbox]").First, pauseAfter: 900);
        });
    }

    private static async Task SiteSettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/ai-settings"), "Settings > Artificial Intelligence");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Card(scene, "Chat Interactions"), 200);
            await scene.HighlightAsync(Card(scene, "Chat Interactions"), "Chat Interactions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("site-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Chat mode"), "Chat mode");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("site-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("xpath=//label[contains(.,'Enable text-to-speech playback')]/ancestor::div[contains(@class,'form-check')][1]").First, "Enable text-to-speech playback");
        });

        await scene.ClearHighlightsAsync();
    }
}
