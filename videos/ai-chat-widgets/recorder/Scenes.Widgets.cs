using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static ILocator PermissionRow(Scene scene, string permission)
        => scene.Page.Locator($"tr:has(input[id='Checkbox.{permission}'])").First;

    private static ILocator LabelBox(Scene scene, string label)
        => scene.Page.Locator($"xpath=//label[starts-with(normalize-space(),'{label}')]/parent::*").First;

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
            await scene.TypeAsync(search, "Admin Widget", delay: 80);
            await scene.PauseAsync(1800);
            await scene.HighlightAsync(scene.Page.GetByText("AI Chat Admin Widget", new() { Exact = true }).First, "AI Chat Admin Widget");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fea-2", async () =>
        {
            await search.FillAsync(string.Empty);
            await scene.TypeAsync(search, "Widgets", delay: 90);
            await scene.PauseAsync(2000);
            await scene.HighlightAsync(scene.Page.Locator("text=OrchardCore.Layers").First, "Layers");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("text=OrchardCore.Widgets").First, "Widgets");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AdminSettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("adm-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/ai-settings"), "Settings > Artificial Intelligence");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Card(scene, "Admin Widget"), 200);
            await scene.HighlightAsync(Card(scene, "Admin Widget"), "Admin Widget");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#ISite_AIChatAdminWidgetSettings_ProfileId").SelectOptionAsync(new SelectOptionValue { Label = "Contoso website assistant" });
            await scene.PauseAsync(600);
            await scene.HighlightAsync(LabelBox(scene, "Chat profile"), "Chat profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("adm-2", async () =>
        {
            await scene.HighlightAsync(LabelBox(scene, "Max history sessions"), "Max history sessions");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "Primary color"), "Primary color");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, SaveButton(scene), 600);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task UseAdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.WaitForSelectorAsync("#AiAdminWidget_Toggle", new() { Timeout = 30000 });
        await scene.StartCaptureAsync();

        await scene.StepAsync("usg-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#AiAdminWidget_Toggle"), "Open AI Assistant");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#AiAdminWidget_Toggle"), pauseAfter: 1500);
        });

        await scene.StepAsync("usg-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AiAdminWidget_Prompt"), "Do you repair electric bikes?", delay: 40);
            await scene.ClickAsync(scene.Page.Locator("#AiAdminWidget_Send"), pauseAfter: 400);
            await scene.PauseAsync(10000);
        });

        await scene.StepAsync("usg-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#AiAdminWidget_NewChat"), "New chat");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#AiAdminWidget_ShowHistory"), pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator("#AiAdminWidget_HistoryPanel"), "Chat history");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#AiAdminWidget_CloseHistory"), pauseAfter: 800);

            var box = await scene.Page.Locator("#AiAdminWidget_Container").BoundingBoxAsync();

            if (box is not null)
            {
                var x = box.X + (box.Width / 2) - 60;
                var y = box.Y + 28;
                await scene.Page.Mouse.MoveAsync(x, y);
                await scene.Page.Mouse.DownAsync();
                await scene.Page.Mouse.MoveAsync(x - 320, y - 40, new() { Steps = 25 });
                await scene.Page.Mouse.UpAsync();
                await scene.PauseAsync(1500);
            }
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
            await scene.HighlightAsync(scene.Page.Locator("text=Always").First, "A layer that is always on");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("web-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Widget')").First, pauseAfter: 900);
            var item = scene.Page.Locator("a:has-text('Artificial Intelligence Chat')").First;
            await scene.HighlightAsync(item, "Artificial Intelligence Chat");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(item);
        });

        await scene.StepAsync("web-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#LayerMetadata_Title"), "Website assistant", delay: 45);
            await scene.Page.Locator("#LayerMetadata_LayerMetadata_Layer").SelectOptionAsync(new SelectOptionValue { Label = "Always" });
            await scene.HighlightAsync(LabelBox(scene, "The layer"), "The layer");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#AIProfilePart_ProfileId").SelectOptionAsync(new SelectOptionValue { Label = "Contoso website assistant" });
            await scene.HighlightAsync(LabelBox(scene, "Profile"), "Profile");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#AIProfilePart_TotalHistory"), "5", delay: 80);
            await scene.HighlightAsync(LabelBox(scene, "Total history chat to show"), "Total history chat to show");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Publish')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Edit/Anonymous");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rol-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Roles/Index"), "Access Control > Roles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, PermissionRow(scene, "QueryAIProfile_ContosoWebsiteAssistant"), 300);
            await scene.HighlightAsync(PermissionRow(scene, "QueryAIProfile_ContosoWebsiteAssistant"), "Query AI profile - Contoso website assistant");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("input[id='Checkbox.QueryAIProfile_ContosoWebsiteAssistant']").First, pauseAfter: 900);
        });

        await scene.StepAsync("rol-2", async () =>
        {
            await scene.HighlightAsync(PermissionRow(scene, "QueryAnyAIProfile"), "Not this one");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, SaveButton(scene), 600);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task VisitorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/contoso-bikes");
        await scene.Page.WaitForSelectorAsync("button[title='Open AI Chat']", new() { Timeout = 30000 });
        await scene.StartCaptureAsync();

        var toggle = scene.Page.Locator("button[title='Open AI Chat']").First;

        await scene.StepAsync("vis-1", async () =>
        {
            await scene.HighlightAsync(toggle, "Open AI Chat");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(toggle, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("[id$='_Container']").First, "AI Chat");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("vis-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[id$='_UserPrompt']").First, "What time do you open on Saturday?", delay: 40);
            await scene.ClickAsync(scene.Page.Locator("[id$='_SendButton']").First, pauseAfter: 400);
            await scene.PauseAsync(11000);
        });
    }
}
