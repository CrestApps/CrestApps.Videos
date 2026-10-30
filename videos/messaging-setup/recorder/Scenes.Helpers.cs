using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static ILocator Conversation(Scene scene, string digits)
        => scene.Page.Locator("a.list-group-item").Filter(new() { HasTextString = digits }).First;

    private static async Task OpenInboxAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/messaging");
        await scene.Page.WaitForSelectorAsync("a.list-group-item", new() { Timeout = 30000 });
        await scene.PauseAsync(1500);
    }
}
