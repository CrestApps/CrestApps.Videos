using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers of the clips -----

    // A collapsible card of an editor, found by the title of its header.
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

    private static ILocator Editor(Scene scene) => scene.Page.Locator(".CodeMirror:visible").First;

    // ----- Clips -----

    private static async Task StartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("start-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/profiles"), "Artificial Intelligence > Profiles");
            await scene.PauseAsync(2600);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.create"), "Add Profile");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1200);
        });

        await scene.StepAsync("start-2", async () =>
        {
            var blank = scene.Page.Locator("#newProfileModal [data-profile-picker-card][data-category='__blank']");
            await scene.HighlightAsync(blank, "Blank profile");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-card]").Filter(new() { HasTextString = "Website assistant" }).First, "A ready-made starting point");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("start-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-category='Talk to people']"), pauseAfter: 2500);
            await scene.ClickAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-category='']"), pauseAfter: 800);
            await scene.TypeAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-filter]"), "summar", delay: 90);
            await scene.PauseAsync(1800);
            await scene.Page.Locator("#newProfileModal [data-profile-picker-filter]").FillAsync(string.Empty);
        });

        await scene.StepAsync("start-4", async () =>
        {
            var locked = scene.Page.Locator("#newProfileModal [data-profile-picker-card]").Filter(new() { HasTextString = "Answers from your docs" }).First;
            await scene.HighlightAsync(locked, "Needs a feature that is not on");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("start-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-card]").Filter(new() { HasTextString = "Website assistant" }).Locator("a.btn").First);
            await scene.Page.Locator("#DisplayText").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#DisplayText"), "Contoso website assistant", delay: 35);
            await scene.HighlightAsync(Section(scene.Page.Locator("#Name")), "Technical name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("start-6", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ChatDeploymentName")), "Chat deployment");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Create profile')"));
        });

        await scene.StepAsync("start-7", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("#AIProfile_DisplayText"), "Everything below is optional");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // Reached from the list, so the menu shows Profiles as the current item.
        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Contoso website assistant").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("ed-1", async () =>
        {
            await scene.HighlightAsync(Card(scene, "General").Locator(".card-header"), "General");
            await scene.PauseAsync(3200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Profile type"), "Profile type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-2", async () =>
        {
            await ScrollToAsync(scene, Field(scene, "Profile type"), 120);
            await scene.HighlightAsync(Field(scene, "Welcome message"), "Welcome message");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_AddInitialPrompt")), "Start the conversation automatically");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-3", async () =>
        {
            await ScrollToAsync(scene, Section(scene.Page.Locator("#AIProfile_TitleType")), 200);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_IsOnAdminMenu")), "Show on admin menu");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_TitleType")), "Title type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-4", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Deployments & Interactions"), 110);
            await scene.HighlightAsync(Card(scene, "Deployments & Interactions"), "Deployments & Interactions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-5", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Instructions"), 110);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_PersonaId")), "AI persona");
            await scene.PauseAsync(3300);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Editor(scene), "System instructions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-6", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Parameters"), 110);
            await scene.HighlightAsync(Field(scene, "Max response"), "Max response");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Temperature"), "Temperature");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Field(scene, "Past messages included"), 300);
            await scene.HighlightAsync(Field(scene, "Past messages included"), "Past messages included");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-7", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Prompt Security"), 110);
            await scene.HighlightAsync(Field(scene, "Maximum messages per window"), "Maximum messages per window");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Maximum anonymous sessions per window"), "Anonymous sessions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-8", async () =>
        {
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.ClickAsync(Tab(scene, "Capabilities"), pauseAfter: 1000);
            await scene.HighlightAsync(Tab(scene, "Capabilities"), "Capabilities");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 700, steps: 25);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ed-9", async () =>
        {
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.ClickAsync(Tab(scene, "Data Processing & Metrics"), pauseAfter: 1000);
            await scene.HighlightAsync(Field(scene, "Session inactivity timeout"), "Session inactivity timeout");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#AIProfile_EnableDataExtraction, [name$='EnableDataExtraction']").First.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Data extraction");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("man-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-card]").Filter(new() { HasTextString = "Background summarizer" }).Locator("a.btn").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Create profile')"));
            await scene.HighlightAsync(Field(scene, "Profile type"), "Utility");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('AI Profiles')").First);
            await scene.HighlightAsync(Row(scene, "Background summarizer").Locator(".badge").First, "Utility");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(RowActions(scene, "Contoso website assistant"), pauseAfter: 900);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-3", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Contoso website assistant").Locator(".dropdown-menu"), "Chat profile");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(RowActions(scene, "Background summarizer"), pauseAfter: 600);
            await scene.HighlightAsync(Row(scene, "Background summarizer").Locator(".dropdown-menu"), "Utility profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-4", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(RowActions(scene, "Contoso website assistant"), pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Contoso website assistant").Locator(".dropdown-menu a:has-text('Clone')").First);
            await scene.HighlightAsync(scene.Page.Locator("#AIProfile_DisplayText"), "Copy of the original");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('AI Profiles')").First);
            await scene.HighlightAsync(Row(scene, "Contoso website assistant").Locator("a:has-text('Edit')").First, "Edit");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Row(scene, "Contoso website assistant").Locator("input[type=checkbox]").First, pauseAfter: 500);
            await scene.ClickAsync(Row(scene, "Background summarizer").Locator("input[type=checkbox]").First, pauseAfter: 800);
        });
    }

    private static async Task PersonasAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/personas");
        await scene.StartCaptureAsync();

        await scene.StepAsync("per-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/personas"), "Artificial Intelligence > Personas");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("per-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIPersona_DisplayText"), "Friendly bike guide", delay: 40);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIPersona_Name")), "Technical name");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#AIPersona_Description"), "A warm, patient guide for Contoso Bikes customers.", delay: 18);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("per-3", async () =>
        {
            await ScrollToAsync(scene, Field(scene, "Persona instructions"), 110);
            await scene.TypeAsync(Editor(scene), "You are warm, patient and upbeat. You explain things simply, avoid jargon, and always offer a next step.", delay: 22);
            await scene.HighlightAsync(Field(scene, "Persona instructions"), "Persona instructions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("per-4", async () =>
        {
            await ScrollToAsync(scene, Field(scene, "Assistant name"), 130);
            await scene.TypeAsync(scene.Page.Locator("#AIPersona_AssistantName"), "Casey", delay: 60);
            await scene.HighlightAsync(Field(scene, "Assistant name"), "Assistant name");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Language"), "Language");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Field(scene, "Voice"), "Voice");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Field(scene, "Avatar URL"), "Avatar URL");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("per-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/ai/profiles"));
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Contoso website assistant").Locator("a:has-text('Edit')").First);
            await ScrollToAsync(scene, Card(scene, "Instructions"), 110);
            await scene.SelectAsync(scene.Page.Locator("#AIProfile_PersonaId"), "Friendly bike guide", pauseAfter: 800);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_PersonaId")), "AI persona");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("per-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/ai/personas"));
        });
    }

    private static async Task TemplatesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/templates");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tpl-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/templates"), "Artificial Intelligence > Templates");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1000);
        });

        await scene.StepAsync("tpl-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#modalAddTemplate .modal-body"), "Available Sources");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddTemplate a[href$='/create/Profile']"));
        });

        await scene.StepAsync("tpl-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIProfileTemplate_DisplayText"), "Contoso helpdesk", delay: 35);
            await scene.TypeAsync(scene.Page.Locator("#AIProfileTemplate_Description").First, "The approved starting point for every internal helpdesk assistant.", delay: 15);
            await scene.TypeAsync(scene.Page.Locator("#AIProfileTemplate_Category"), "Company standards", delay: 35);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfileTemplate_IsListable")), "Listable");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("tpl-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#AIProfileTemplate_ProfileType"), "Chat", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfileTemplate_ProfileType")), "Profile type");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("a.nav-link").Filter(new() { HasTextString = "Capabilities" }).First.Locator("xpath=ancestor::ul[1]"), "The same tabs as a profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("tpl-5", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Instructions"), 110);
            await scene.TypeAsync(Editor(scene), "Answer only from approved company documents. If unsure, say so and offer to open a ticket.", delay: 8);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("tpl-6", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Contoso helpdesk").Locator("a:has-text('Create profile')").First, "Create profile");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Contoso helpdesk").Locator("a:has-text('Create profile')").First);
        });

        await scene.StepAsync("tpl-7", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a:has-text('Back')").First);
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-category='Company standards']"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("#newProfileModal [data-profile-picker-card]:visible").Filter(new() { HasTextString = "Contoso helpdesk" }).First, "Your template");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PromptsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/templates");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pr-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddTemplate a[href$='/create/SystemPrompt']"));
        });

        await scene.StepAsync("pr-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#SystemPromptTemplate_DisplayText, [name$='DisplayText']").First, "Company tone", delay: 35);
            await ScrollToAsync(scene, Field(scene, "System prompt"), 150);
            await scene.TypeAsync(Editor(scene), "Write in a warm, plain voice. Use short sentences and never use slang.", delay: 15);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("pr-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/ai/profiles"));
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Contoso website assistant").Locator("a:has-text('Edit')").First);
            var add = scene.Page.Locator("button:has-text('Add prompt template')");
            await ScrollToAsync(scene, add, 300);
            await scene.HighlightAsync(add, "Prompt templates");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(add, pauseAfter: 900);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pr-4", async () =>
        {
            var menu = scene.Page.Locator("button:has-text('Add prompt template')").Locator("xpath=following-sibling::*[contains(@class,'dropdown-menu')][1]");
            await scene.HighlightAsync(menu, "Grouped by category");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(menu.Locator("text=Company tone").First, pauseAfter: 900);
            // Folds the template's own settings away, so the list shows only the names.
            await scene.ClickAsync(scene.Page.Locator("span.fw-semibold:visible").Filter(new() { HasTextString = "Company tone" }).First, pauseAfter: 600);
        });

        await scene.StepAsync("pr-5", async () =>
        {
            var card = scene.Page.Locator("button:has-text('Add prompt template')").Locator("xpath=ancestor::div[contains(@class,'ocat-end')][1]");
            await scene.HighlightAsync(card, "Added before your instructions");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }
}
