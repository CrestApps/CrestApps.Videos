using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Payments video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-stripe", StripeAsync),
        ("04-offline", OfflineAsync),
        ("05-transactions", TransactionsAsync),
        ("06-reminders", RemindersAsync),
        ("07-payments", PaymentsAsync),
        ("08-webhooks", WebhooksAsync),
    ];

    /// <summary>
    /// Turns on the features the video shows (not recorded). The site is the one the Orders video prepared:
    /// four fake orders, one of them awaiting a bank transfer.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(
            page,
            "CrestApps.OrchardCore.Commerce.Webhooks",
            "CrestApps.OrchardCore.Transactions.Notification",
            "CrestApps.OrchardCore.Transactions.Receipts");

        await page.Context.CloseAsync();
    }
}
