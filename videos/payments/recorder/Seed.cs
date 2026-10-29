using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// Helpers that seed the demo site outside the capture: turn features on, and fill in and submit an admin form
/// the way a person would, so the saved data is exactly what the editor produces.
/// </summary>
public static class Seed
{
    public static async Task<IPage> NewPageAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = Scenes.StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);

        return page;
    }

    public static async Task EnableAsync(IPage page, params string[] features)
    {
        foreach (var feature in features)
        {
            await page.GotoAsync(Scenes.BaseUrl + "/Admin/Features");
            var enable = page.Locator($"a[href$=\"/{feature}/Enable\"]");

            if (await enable.CountAsync() > 0)
            {
                await page.RunAndWaitForNavigationAsync(() => enable.First.ClickAsync());
                Console.WriteLine($"Enabled {feature}");
            }
        }
    }

    public static async Task DisableAsync(IPage page, params string[] features)
    {
        foreach (var feature in features)
        {
            await page.GotoAsync(Scenes.BaseUrl + "/Admin/Features");
            var disable = page.Locator($"a[href$=\"/{feature}/Disable\"]");

            if (await disable.CountAsync() > 0)
            {
                await disable.First.ClickAsync();
                await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Yes", Exact = true }).ClickAsync());
                Console.WriteLine($"Disabled {feature}");
            }
        }
    }

    /// <summary>
    /// Opens a page, sets the named fields of the form that holds the submit button, and submits it with that button.
    /// A value of "true" or "false" sets a check box; anything else is the value of the field.
    /// </summary>
    public static async Task SubmitAsync(IPage page, string path, Dictionary<string, string> values, string submitName = "submit.Publish")
    {
        await page.GotoAsync(Scenes.BaseUrl + path);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.RunAndWaitForNavigationAsync(async () =>
        {
            await page.EvaluateAsync("""
                ([values, submitName]) => {
                    const button = document.querySelector(submitName.startsWith('css:') ? submitName.slice(4) : '[name="' + submitName + '"]');
                    const form = button.closest('form');
                    for (const [name, value] of Object.entries(values)) {
                        const fields = [...form.querySelectorAll('[name="' + name + '"]')].filter(e => e.type !== 'hidden' || !form.querySelector('[name="' + name + '"]:not([type=hidden])'));
                        if (!fields.length) { throw new Error('No field ' + name); }
                        for (const field of fields) {
                            if (field.type === 'checkbox') { field.checked = value === 'true'; }
                            else { field.value = value; }
                            field.dispatchEvent(new Event('input', { bubbles: true }));
                            field.dispatchEvent(new Event('change', { bubbles: true }));
                        }
                    }
                    form.requestSubmit(button);
                }
                """, new object[] { values, submitName });
        });

        var errors = await page.Locator(".validation-summary-errors, .text-danger.field-validation-error, .alert-danger").AllInnerTextsAsync();

        if (errors.Any(text => !string.IsNullOrWhiteSpace(text)))
        {
            throw new InvalidOperationException($"{path} did not save: {string.Join(" | ", errors)}");
        }
    }

    /// <summary>
    /// Creates and publishes a content item of a type from its admin editor.
    /// </summary>
    public static Task CreateAsync(IPage page, string contentType, Dictionary<string, string> values)
        => SubmitAsync(page, $"/Admin/Contents/ContentTypes/{contentType}/Create", values);

    /// <summary>
    /// The value of a media field's hidden input for pictures already in the media library.
    /// </summary>
    public static string Pictures(params string[] paths)
        => JsonSerializer.Serialize(paths.Select(path => new { path, isRemoved = false, isNew = false, mediaText = "", anchor = new { x = 0.5, y = 0.5 }, attachedFileName = "" }));
}
