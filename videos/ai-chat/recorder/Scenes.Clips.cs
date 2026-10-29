using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers of the clips -----

    // A collapsible card of an editor or of the settings, found by the title of its header.
    private static ILocator Card(Scene scene, string title)
        => scene.Page.Locator($"xpath=//div[contains(@class,'card') and ./div[contains(@class,'card-header')][normalize-space()='{title}']]");

    // The row of a field, found by its label.
    private static ILocator Field(Scene scene, string label)
        => scene.Page.Locator($"xpath=//div[contains(@class,'ocat-wrapper')][.//label[contains(normalize-space(),'{label}')]][not(.//div[contains(@class,'ocat-wrapper')])]").First;

    private static ILocator Tab(Scene scene, string text) => scene.Page.Locator("a.nav-link").Filter(new() { HasTextString = text }).First;

    // Scrolls smoothly until the element sits near the top of the screen.
    private static async Task ScrollToAsync(Scene scene, ILocator target, double top = 150)
    {
        var current = await target.EvaluateAsync<double>("e => e.getBoundingClientRect().top");
        await scene.ScrollAsync(0, current - top);
        await scene.PauseAsync(500);
    }

    private static ILocator Row(Scene scene, string title)
        => scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = title }).First;

    private static ILocator RowActions(Scene scene, string title) => Row(scene, title).Locator("a.dropdown-toggle, button.dropdown-toggle").First;

    private static ILocator ChatPrompt(Scene scene) => scene.Page.Locator("#AIChatSession_UserPrompt");

    private static ILocator ChatSend(Scene scene) => scene.Page.Locator("#AIChatSession_SendButton");

    // Sends a message on the chat page, and lets the answer be written.
    private static async Task AskAsync(Scene scene, string question, int waitMilliseconds)
    {
        await scene.TypeAsync(ChatPrompt(scene), question, delay: 32);
        await scene.ClickAsync(ChatSend(scene), pauseAfter: 300);
        // The send arrow turns into a stop button while the answer is written.
        await scene.Page.WaitForSelectorAsync("#AIChatSession_SendButton .fa-stop", new() { Timeout = 15000 });
        await scene.Page.WaitForSelectorAsync("#AIChatSession_SendButton .fa-arrow-up", new() { Timeout = 60000 });
        await scene.PauseAsync(waitMilliseconds);
    }

    // Makes the profile answer from the shop's facts, and name each chat after the first question (not recorded).
    private static async Task FixProfileAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/ai/profiles");
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".list-group-item a:has-text('Edit')").First.ClickAsync());

        if ((await page.EvaluateAsync<string>("() => document.getElementById('AIProfile_SystemMessage').value")).Contains("Contoso Bikes"))
        {
            await context.CloseAsync();
            return;
        }

        var lines = new[]
        {
            "You are the assistant on the website of Contoso Bikes, a bicycle shop. Answer questions about orders, repairs and the shop in short, friendly answers.",
            "",
            "Facts about the shop:",
            "- Opening hours: Monday to Saturday 9:00 to 18:00. Closed on Sundays.",
            "- Repairs: a basic tune-up takes two working days and costs 60 dollars. A flat tire repair takes one day.",
            "- Returns: unused bikes can be returned within 30 days.",
            "- We sell city, mountain and electric bikes.",
            "",
            "If you do not know something, say so and offer to have the team call back. Never ask for payment details.",
        };
        var text = string.Join(((char)10).ToString(), lines);
        await page.EvaluateAsync("text => { const cm = document.getElementById('AIProfile_SystemMessage').parentElement.querySelector('.CodeMirror').CodeMirror; cm.setValue(text); cm.save(); document.getElementById('AIProfile_SystemMessage').value = text; }", text);
        await page.Locator("#AIProfile_TitleType").SelectOptionAsync(new SelectOptionValue { Label = "Set the first prompt as the title" });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("Fixed the profile");
        await context.CloseAsync();
    }

    // ----- Seed data (not recorded) -----

    private static async Task SeedChatsAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/ai/profiles");
        var path = await page.Locator(".list-group-item .dropdown-menu a:has-text('New chat')").First.GetAttributeAsync("href");
        var history = path.Replace("/chat/session/", "/chat/history/");

        await page.GotoAsync(BaseUrl + history);

        if (!(await page.Locator("body").InnerTextAsync()).Contains("2 items"))
        {
            foreach (var question in new[] { "What is your return policy?", "Can you fix a flat tire on Friday?" })
            {
                await page.GotoAsync(BaseUrl + path);
                await page.Locator("#AIChatSession_UserPrompt").FillAsync(question);
                await page.Locator("#AIChatSession_SendButton").ClickAsync();
                await Task.Delay(14000);
                Console.WriteLine("Asked: " + question);
            }
        }

        await context.CloseAsync();
    }

    // ----- Clips -----

    private static async Task ChatAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("chat-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/profiles"), "Artificial Intelligence > Profiles");
            await scene.PauseAsync(2400);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(RowActions(scene, "Contoso website assistant"), pauseAfter: 700);
            var newChat = Row(scene, "Contoso website assistant").Locator(".dropdown-menu a:has-text('New chat')").First;
            await scene.HighlightAsync(newChat, "New chat");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(newChat);
        });

        await scene.StepAsync("chat-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("text=Hi! Ask me about Contoso Bikes").First, "Welcome message");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('New Chat'), button:has-text('New Chat')").First, "New Chat");
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('Chat History'), button:has-text('Chat History')").First, "Chat History");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("chat-3", async () =>
        {
            await AskAsync(scene, "Are you open on the weekend?", 600);
        });

        await scene.StepAsync("chat-4", async () =>
        {
            await AskAsync(scene, "How long does a basic tune-up take?", 600);
        });

        await scene.StepAsync("chat-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('Chat History'), button:has-text('Chat History')").First, "Chat History");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('Chat History'), button:has-text('Chat History')").First);
        });
    }

    private static async Task HistoryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // Reached from the list, so the menu shows Profiles as the current item.
        await scene.OpenAsync("/Admin/ai/profiles");
        await RowActions(scene, "Contoso website assistant").ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Contoso website assistant").Locator(".dropdown-menu a:has-text('View chat history')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("hist-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Are you open on the weekend"), "Newest first");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Are you open on the weekend").Locator(".badge").First, "Status");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hist-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box, input[placeholder='History search']").First, "return", delay: 90);
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item a:has-text('View'), .list-group-item .btn-success").First, "View");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hist-3", async () =>
        {
            await scene.ClickAsync(RowActions(scene, "return policy"), pauseAfter: 700);
            await scene.HighlightAsync(Row(scene, "return policy").Locator(".dropdown-menu"), "Delete this chat");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('Delete Chat History'), button:has-text('Delete Chat History')").First, "Delete all of your chats");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hist-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a:has-text('New Chat'), button:has-text('New Chat')").First, "New Chat");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('New Chat'), button:has-text('New Chat')").First);
        });
    }

    private static async Task WidgetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("aw-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/ai-settings"), "Settings > Artificial Intelligence");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Card(scene, "Admin Widget"), 200);
            await scene.HighlightAsync(Card(scene, "Admin Widget"), "Admin Widget");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("aw-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#ISite_AIChatAdminWidgetSettings_ProfileId"), "Contoso website assistant", pauseAfter: 1200);
            await scene.HighlightAsync(Card(scene, "Admin Widget"), "Chat profile, history and color");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("aw-3", async () =>
        {
            await scene.NavigateAsync("/Admin/ai/profiles");
            await scene.HighlightAsync(scene.Page.Locator("#AiAdminWidget_Toggle"), "Open AI Assistant");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#AiAdminWidget_Toggle"), pauseAfter: 900);
        });

        await scene.StepAsync("aw-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AiAdminWidget_Prompt"), "When can I pick up my bike?", delay: 40);
            await scene.ClickAsync(scene.Page.Locator("#AiAdminWidget_Send"), pauseAfter: 3500);
            await scene.HighlightAsync(scene.Page.Locator(".ai-chat-widget-header-actions"), "New chat and Chat history");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("aw-5", async () =>
        {
            await scene.DragAsync(scene.Page.Locator(".ai-chat-widget-header").First, -380, -60);
            await scene.PauseAsync(1200);
            await scene.DragAsync(scene.Page.Locator(".ai-chat-widget-header").First, 380, 60);
        });
    }

    private static async Task WebsiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Layers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("web-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Layers"), "Design > Widgets");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("h2:has-text('Zones'), h3:has-text('Zones')").First, "Zones");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("h2:has-text('Layers'), h3:has-text('Layers')").First, "Layers");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("web-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('Edit')").First);
            await scene.HighlightAsync(scene.Page.Locator("text=Rules").First, "Rules");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("text=Boolean condition").First, "Boolean condition, True");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("web-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('Widgets and Layers')").First);
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Widget'), a:has-text('Add Widget')").First, pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a:has-text('Artificial Intelligence Chat')").First, "Artificial Intelligence Chat");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a:has-text('Artificial Intelligence Chat')").First);
        });

        await scene.StepAsync("web-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#LayerMetadata_Title"), "Website assistant", delay: 40);
            await scene.SelectAsync(scene.Page.Locator("#LayerMetadata_LayerMetadata_Layer"), "Always", pauseAfter: 700);
            await scene.SelectAsync(scene.Page.Locator("#AIProfilePart_ProfileId"), "Contoso website assistant", pauseAfter: 700);
            await scene.HighlightAsync(Field(scene, "Total history chat to show"), "Total history chat to show");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Publish')").First);
        });

        await scene.StepAsync("web-5", async () =>
        {
            await scene.NavigateAsync("/");
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator(".ai-chat-widget-toggle, [id$='_Toggle']").First, "Chat button");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("web-6", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".ai-chat-widget-toggle, [id$='_Toggle']").First, pauseAfter: 1200);
            await scene.TypeAsync(scene.Page.Locator(".ai-chat-widget-container textarea:visible").First, "Do you sell electric bikes?", delay: 40);
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.PauseAsync(3500);
        });
    }

    private static async Task EnginesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("eng-1", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Default Orchestrator"), 200);
            await scene.HighlightAsync(Card(scene, "Default Orchestrator"), "Default Orchestrator");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("eng-2", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Claude"), 150);
            await scene.SelectAsync(scene.Page.Locator("select[name$='ClaudeSettings.AuthenticationType']"), "API key", pauseAfter: 1200);
            await scene.HighlightAsync(Card(scene, "Claude"), "Claude");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("eng-3", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Copilot"), 150);
            await scene.SelectAsync(scene.Page.Locator("select[name$='CopilotSettings.AuthenticationType']"), "API key (BYOK)", pauseAfter: 1200);
            await scene.HighlightAsync(Card(scene, "Copilot"), "Copilot");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("eng-4", async () =>
        {
            await scene.NavigateAsync("/Admin/ai/profiles");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Contoso website assistant").Locator("a:has-text('Edit')").First);
            await scene.HighlightAsync(Field(scene, "Orchestrator"), "Orchestrator");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#orchestrator-selector"), "Claude", pauseAfter: 1500);
            await scene.HighlightAsync(Card(scene, "General"), "Claude chooses its own model");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProtectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prot-1", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Prompt Security"), 110);
            await scene.HighlightAsync(scene.Page.Locator("#ISite_PromptSecurityOptions_EnableInjectionDetection").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Enable injection detection");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Blocking threshold"), "Blocking threshold");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prot-2", async () =>
        {
            await ScrollToAsync(scene, Field(scene, "Maximum messages per window"), 150);
            await scene.HighlightAsync(Field(scene, "Maximum messages per window"), "Maximum messages per window");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Anonymous message rate-limit tiers"), "Tiers for visitors who are not signed in");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Maximum anonymous sessions per window"), "New chats a visitor can start");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prot-3", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Visitor Identity"), 110);
            await scene.HighlightAsync(Field(scene, "Visitor cookie name"), "Visitor cookie name");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Cookie lifetime"), "Cookie lifetime");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prot-4", async () =>
        {
            await scene.NavigateAsync("/Admin/ai/profiles");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Contoso website assistant").Locator("a:has-text('Edit')").First);
            await ScrollToAsync(scene, Card(scene, "Prompt Security"), 110);
            await scene.HighlightAsync(Field(scene, "Maximum messages per window"), "Overrides the site's limit");
        });

        await scene.ClearHighlightsAsync();
    }
}
