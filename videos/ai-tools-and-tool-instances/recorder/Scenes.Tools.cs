using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private const string ProfileEdit = "/Admin/ai/profile/edit/4s2qrp3ppxekpszq87x662rgd8";

    private static ILocator PermissionRow(Scene scene, string permission)
        => scene.Page.Locator($"tr:has(input[id='Checkbox.{permission}'])").First;

    private static ILocator LabelBox(Scene scene, string label)
        => scene.Page.Locator($"xpath=//label[starts-with(normalize-space(),'{label}')]/parent::*").First;

    private static async Task OpenCapabilitiesAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.Locator("a[href='#tab-capabilities']"), pauseAfter: 1200);
    }

    private static async Task ToolsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ProfileEdit);
        await scene.StartCaptureAsync();

        var search = scene.Page.Locator("input.tool-search-input");

        await scene.StepAsync("tol-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a[href='#tab-capabilities']"), "Capabilities");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await OpenCapabilitiesAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("input.tool-search-input").Locator("xpath=ancestor::div[1]"), "Tools");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, scene.Page.Locator("#selectAll_Content-Management"), 300);
            await scene.HighlightAsync(scene.Page.Locator("h3:has-text('Content Management'), h4:has-text('Content Management'), h5:has-text('Content Management')").First, "A category");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("tol-2", async () =>
        {
            await ScrollToAsync(scene, search, 300);
            await scene.TypeAsync(search, "content item", delay: 80);
            await scene.HighlightAsync(search, "Search tools");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("input[id^='tool_Content-Management']:visible").First, pauseAfter: 1200);
            await search.FillAsync(string.Empty);
            await scene.HighlightAsync(scene.Page.Locator("#selectAllTools").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Select All Tools");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("tol-3", async () =>
        {
            await scene.TypeAsync(search, "delete", delay: 80);
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator("div.form-check:has(label:has-text('Delete Content Item'))").First, "Really acts");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await search.FillAsync(string.Empty);
            await ScrollToAsync(scene, SaveButton(scene), 600);
            await scene.HighlightAsync(SaveButton(scene), "Save");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RolesToolsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Edit/Editor");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rol-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Roles/Index"), "Access Control > Roles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, PermissionRow(scene, "AccessAnyAITool"), 300);
            await scene.HighlightAsync(PermissionRow(scene, "AccessAnyAITool"), "Access any AI tool");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rol-2", async () =>
        {
            await ScrollToAsync(scene, PermissionRow(scene, "AccessAITool_searchForContentItems"), 300);
            await scene.HighlightAsync(PermissionRow(scene, "AccessAITool_searchForContentItems"), "One tool");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("input[id='Checkbox.AccessAITool_searchForContentItems']").First, pauseAfter: 1000);
        });
    }

    private static async Task InstanceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/tool-instances");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ins-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/tool-instances"), "Artificial Intelligence > Tool Instances");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Tool Instance'), a:has-text('Add Tool Instance')").First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .modal-content").First, "Available Sources");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("ins-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='create/http-api-request']").First);
            await scene.TypeAsync(scene.Page.Locator("#AIToolInstance_Name"), "Order lookup", delay: 50);
            await scene.HighlightAsync(LabelBox(scene, "Name"), "Name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ins-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIToolInstance_Description"), "Looks up the status of a customer's order by order number.", delay: 25);
            await scene.HighlightAsync(LabelBox(scene, "Description"), "Description");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ins-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIToolInstance_BaseUrl"), "https://orders.contoso.example/api/orders", delay: 25);
            await scene.HighlightAsync(LabelBox(scene, "Base URL"), "Base URL");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(LabelBox(scene, "HTTP method"), "HTTP method");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#AIToolInstance_AuthenticationType").SelectOptionAsync(new SelectOptionValue { Label = "API Key" });
            await scene.PauseAsync(700);
            await ScrollToAsync(scene, LabelBox(scene, "Authentication type"), 250);
            await scene.HighlightAsync(LabelBox(scene, "Authentication type"), "Authentication type");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#AIToolInstance_ApiKeyHeaderName").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#AIToolInstance_ApiKeyHeaderName"), "X-Api-Key", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#AIToolInstance_ApiKey"), "demo-only-key", delay: 50);
            await scene.HighlightAsync(LabelBox(scene, "API key"), "A secret");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ins-5", async () =>
        {
            await ScrollToAsync(scene, SaveButton(scene), 600);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Row(scene, "Order lookup"), "Your tool instance");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UseInstanceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ProfileEdit);
        await scene.StartCaptureAsync();

        await scene.StepAsync("usi-1", async () =>
        {
            await OpenCapabilitiesAsync(scene);
            var box = scene.Page.Locator("div.form-check:has(label:has-text('Order lookup'))").First;
            await ScrollToAsync(scene, box, 300);
            await scene.HighlightAsync(box, "Tool Instances");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(box.Locator("input[type=checkbox]").First, pauseAfter: 1200);
            await ScrollToAsync(scene, SaveButton(scene), 600);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }
}
