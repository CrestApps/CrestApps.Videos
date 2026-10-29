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

}
