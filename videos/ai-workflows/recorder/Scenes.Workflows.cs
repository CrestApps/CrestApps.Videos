using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private const string WorkflowName = "Summarize closed chats";

    private static ILocator LabelBox(Scene scene, string label)
        => scene.Page.Locator($"xpath=//label[starts-with(normalize-space(),'{label}')]/parent::*").First;

    // Opens the editor of the workflow from the list, so the clip does not depend on its numeric id.
    private static async Task OpenWorkflowAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator($"a:has-text('{WorkflowName}')").First.ClickAsync());
        await scene.Page.WaitForSelectorAsync("button:has-text('Add Task')", new() { Timeout = 30000 });
        await scene.PauseAsync(1200);
    }

    private static async Task CreateWorkflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cre-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Workflows/Types"), "Design > Workflows");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var create = scene.Page.Locator("a:has-text('Create Workflow')").First;
            await scene.HighlightAsync(create, "Create Workflow");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(create);
            await scene.TypeAsync(scene.Page.Locator("#Name"), WorkflowName, delay: 55);
            await scene.HighlightAsync(LabelBox(scene, "Name"), "Name");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").First);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Add Event')").First, "Add Event");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task EventAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("evt-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Event')").First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator(".modal.show a[href='#a-i-chat']").First, pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .col.activity:has-text('AI Chat Session')").First, "AI Chat events");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("evt-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='AIChatSessionClosedEvent/Add']").First);
            await scene.HighlightAsync(LabelBox(scene, "Profile"), "Profile");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").First);
            await scene.PauseAsync(1200);
        });
    }

    private static async Task TaskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("tsk-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Task')").First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator(".modal.show a[href='#artificial-intelligence']").First, pauseAfter: 1000);
            var card = scene.Page.Locator(".modal.show .col.activity:has-text('AI Completion using Profile')").First;
            await scene.HighlightAsync(card, "AI Completion using Profile");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='AICompletionFromProfileTask/Add']").First);
        });

        await scene.StepAsync("tsk-2", async () =>
        {
            await scene.Page.Locator("#AICompletionFromProfileTask_ProfileId").SelectOptionAsync(new SelectOptionValue { Label = "Contoso website assistant" });
            await scene.HighlightAsync(LabelBox(scene, "Profiles"), "Profiles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#AICompletionFromProfileTask_ResultPropertyName"), "AI-Summary", delay: 70);
            await scene.HighlightAsync(LabelBox(scene, "Result property name"), "Result property name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("tsk-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".CodeMirror").First, pauseAfter: 400);
            await scene.Page.Keyboard.TypeAsync("Summarize the chat in one short sentence.", new() { Delay = 40 });
            await scene.HighlightAsync(LabelBox(scene, "Prompt template"), "Prompt template");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("text=Failed").First, "Failed");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("text=Drew Blank").First, "Drew Blank");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task ConnectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("con-1", async () =>
        {
            var task = scene.Page.Locator(".activity:has-text('AI Completion')").First;
            await scene.MoveToAsync(task, -60, -20);
            await scene.Page.Mouse.DownAsync();
            var box = await task.BoundingBoxAsync();
            await scene.Page.Mouse.MoveAsync((float)(box!.X + 20), (float)(box.Y + 20), new() { Steps = 4 });
            await scene.Page.Mouse.MoveAsync((float)(box.X + 380), (float)(box.Y + 140), new() { Steps = 30 });
            await scene.Page.Mouse.UpAsync();
            await scene.PauseAsync(1200);

            var from = await scene.Page.Locator(".jtk-endpoint").First.BoundingBoxAsync();
            var to = await task.BoundingBoxAsync();
            await scene.Page.Mouse.MoveAsync((float)(from!.X + 8), (float)(from.Y + 8), new() { Steps = 10 });
            await scene.Page.Mouse.DownAsync();
            await scene.Page.Mouse.MoveAsync((float)(to!.X + to.Width / 2), (float)(to.Y + to.Height / 2), new() { Steps = 40 });
            await scene.Page.Mouse.UpAsync();
            await scene.PauseAsync(2000);
        });

        await scene.StepAsync("con-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("text=Failed").First, "Failed");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("text=Drew Blank").First, "Drew Blank");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").Last);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task DirectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("dir-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Task')").First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator(".modal.show a[href='#artificial-intelligence']").First, pauseAfter: 1000);
            var card = scene.Page.Locator(".modal.show .col.activity:has-text('AI Completion using Direct Config')").First;
            await scene.HighlightAsync(card, "AI Completion using Direct Config");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='AICompletionWithConfigTask/Add']").First);
            await scene.HighlightAsync(scene.Page.Locator(".nav-tabs").First, "Content and Capabilities");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".nav-tabs .nav-link:has-text('Capabilities'), .nav-tabs button:has-text('Capabilities'), .nav-tabs a:has-text('Capabilities')").First, pauseAfter: 3500);
            await scene.ClickAsync(scene.Page.Locator(".nav-tabs .nav-link:has-text('Content'), .nav-tabs button:has-text('Content'), .nav-tabs a:has-text('Content')").First, pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('Cancel')").First);
        });
    }
}
