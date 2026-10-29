using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers of the messaging clips -----

    private static ILocator Conversation(Scene scene, string digits)
        => scene.Page.Locator("a.list-group-item").Filter(new() { HasTextString = digits }).First;

    private static ILocator FilterButton(Scene scene) => scene.Page.Locator("button:has(.fa-filter), button:has(.fa-filter-list)").First;

    private static ILocator FavoritesStar(Scene scene) => scene.Page.Locator("a[title='Your favorite customers']").First;

    private static ILocator PencilButton(Scene scene) => scene.Page.Locator("a.btn-primary.btn-sm").First;

    private static ILocator Reply(Scene scene) => scene.Page.Locator("textarea[placeholder^='Reply']").First;

    private static async Task OpenInboxAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/messaging");
        await scene.Page.WaitForSelectorAsync("a.list-group-item", new() { Timeout = 30000 });
        await scene.PauseAsync(1500);
    }

    // Simulates a text from a customer on the demo site (not recorded).
    private static void ReceiveText(string from, string text)
    {
        Process.Start(new ProcessStartInfo("python", $"C:/cavideo/ai-1/post_sms.py {from} \"{text}\"") { CreateNoWindow = true, UseShellExecute = false });
    }

    // ----- Clips -----

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/messaging"), "Messaging > Inbox");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("a.list-group-item").First, "Newest first");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("open-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[placeholder='Search']").First, "0113", delay: 150);
            await scene.PauseAsync(2000);
            await scene.Page.Locator("input[placeholder='Search']").First.FillAsync(string.Empty);
            await scene.PauseAsync(800);
            await scene.ClickAsync(FilterButton(scene), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "All, My conversations, Unassigned");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("open-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HighlightAsync(scene.Page.Locator("#messaging-available-toggle").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Available");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("open-4", async () =>
        {
            await scene.HighlightAsync(FavoritesStar(scene), "Favorites");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PencilButton(scene), "New conversation");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReplyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("rep-1", async () =>
        {
            await scene.ClickAsync(Conversation(scene, "0111"), pauseAfter: 1800);
            await scene.HighlightAsync(scene.Page.Locator("text=In the Support shared inbox").First, "Where it sits");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button[title^='Claim']").First, "Claim");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[title^='Claim']").First, pauseAfter: 1500);
        });

        await scene.StepAsync("rep-3", async () =>
        {
            await scene.TypeAsync(Reply(scene), "Good news, your repair is ready.", delay: 40);
            await scene.Page.Keyboard.PressAsync("Shift+Enter");
            await scene.Page.Keyboard.TypeAsync("You can pick it up until 18:00.", new() { Delay = 40 });
            await scene.PauseAsync(1500);
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("button.btn-dark.btn-sm").First.ClickAsync());
        });

        await scene.StepAsync("rep-4", async () =>
        {
            await scene.PauseAsync(3500);
            await scene.HighlightAsync(scene.Page.Locator("text=Queued").Last, "Queued for the provider");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CloseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("cls-1", async () =>
        {
            await scene.ClickAsync(Conversation(scene, "0114"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Close')").First, "Close");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Close')").First, pauseAfter: 1800);
        });

        await scene.StepAsync("cls-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Reopen')").First, "Reopen");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Reopen')").First, pauseAfter: 1500);
        });

        await scene.StepAsync("cls-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Mark spam')").First, "Mark spam");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FavoritesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("fav-1", async () =>
        {
            await scene.ClickAsync(Conversation(scene, "0113"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("button[title^='Add this customer']").First, "Add to favorites");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[title^='Add this customer']").First, pauseAfter: 1500);
        });

        await scene.StepAsync("fav-2", async () =>
        {
            await scene.ClickAsync(FavoritesStar(scene), pauseAfter: 1800);
            await scene.HighlightAsync(scene.Page.Locator("a.list-group-item").First, "Your favorite");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("fav-3", async () =>
        {
            await scene.ClickAsync(Conversation(scene, "0113"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator("button[title^='Attach']").First, "Attach pictures");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(scene.Page.Locator("button[title^='Attach']").First);
            await scene.PauseAsync(2500);
        });
    }

    private static async Task NoticeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // The inbox was opened before, so the account is tied to the queue.
        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();

        await scene.StepAsync("not-1", async () =>
        {
            await scene.Page.Locator("#left-nav button.messaging, #left-nav .messaging").First.ClickAsync();
            await scene.HighlightAsync(Menu(scene, "/Admin/messaging"), "Unread conversations waiting for you");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("not-2", async () =>
        {
            ReceiveText("+14155550115", "Is the shop open on Monday?");
            await scene.PauseAsync(7000);
        });

        await scene.StepAsync("not-3", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/messaging"), "Count of waiting conversations");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task NewConversationAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenInboxAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("new-1", async () =>
        {
            await scene.ClickAsync(PencilButton(scene), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("text=From").First, "From: the number you send from");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("new-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("input[placeholder^='Search contacts']").First, "To");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("input[placeholder^='Addresses not in your contacts']").First, "+14155550199", delay: 70);
            await scene.HighlightAsync(scene.Page.Locator("input[placeholder^='Addresses not in your contacts']").First, "Other");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("new-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("textarea[placeholder^='Write your message']").First, "Hello! Your helmet order has arrived.", delay: 40);
            await scene.PauseAsync(1200);
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Send')").Last, pauseAfter: 2000);
        });
    }
}
