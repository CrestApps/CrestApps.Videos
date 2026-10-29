using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    // ----- Clips -----

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/installment-plans");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/installment-plans"), "Settings > Installment Plans");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "ISite_InstallmentPlanSettings_DefaultCollectionMethod"), "Collect scheduled payments by");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "ISite_InstallmentPlanSettings_RetryDays"), "Retry a declined card after (days)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Offer for balances from").First, "Installments at checkout");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await Field(scene, "ISite_InstallmentPlanSettings_CheckoutInstallmentCounts").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "ISite_InstallmentPlanSettings_CheckoutInstallmentCounts"), "3, 6", delay: 90);
            await scene.HighlightAsync(Field(scene, "ISite_InstallmentPlanSettings_CheckoutInstallmentCounts"), "Numbers of payments offered");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "ISite_InstallmentPlanSettings_CheckoutMinimumAmount"), "Smallest balance");
        });
    }

    private static async Task NewPlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/installment-plans");
        await scene.StartCaptureAsync();

        await scene.StepAsync("new-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/installment-plans"), "Subscriptions > Installment Plans");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "New plan"));
        });

        await scene.StepAsync("new-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label[for='customer-new']"), pauseAfter: 600);
            await scene.TypeAsync(Field(scene, "NewCustomerName"), "Taylor Reed", delay: 60);
            await scene.TypeAsync(Field(scene, "NewCustomerEmail"), "taylor.reed@example.com", delay: 40);
            await scene.HighlightAsync(Field(scene, "NewCustomerEmail"), "A new customer, with their email");
        });

        await ClearAsync(scene);

        await scene.StepAsync("new-3", async () =>
        {
            await scene.TypeAsync(Field(scene, "Title"), "Kitchen remodel", delay: 50);
            await Field(scene, "TotalAmount").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "TotalAmount"), "1200", delay: 80);
            await Field(scene, "DownPaymentAmount").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "DownPaymentAmount"), "300", delay: 80);
            await Field(scene, "InstallmentCount").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "InstallmentCount"), "6", delay: 80);
            await scene.SelectAsync(Field(scene, "Frequency"), "Every month", pauseAfter: 600);
            await Field(scene, "FirstDueDate").FillAsync(DateTime.Today.AddDays(30).ToString("yyyy-MM-dd"));
            await scene.HighlightAsync(Field(scene, "FirstDueDate"), "First payment due");
        });

        await ClearAsync(scene);

        await scene.StepAsync("new-4", async () =>
        {
            await scene.Page.GetByText("Schedule", new() { Exact = true }).Last.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.GetByText("Schedule", new() { Exact = true }).Last, "Every payment, its date and amount");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("label[for='collection-auto']"), "Charge the card on each due date");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("label[for='collection-invoice']"), "Invoice the customer for each payment");
        });

        await ClearAsync(scene);

        await scene.StepAsync("new-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Continue to the down payment" }));
            await scene.PauseAsync(2500);
            Console.WriteLine("AFTER CREATE: " + scene.Page.Url + " :: " + (await scene.Page.Locator("main, body").First.InnerTextAsync()).Replace("\n", " | ")[..600]);
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/installment-plans");
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Kitchen remodel" }).First, "Status, total, received and next payment");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Kitchen remodel" }).First.GetByRole(AriaRole.Link).First);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Summary", new() { Exact = true }).First, "Status, customer, total and what is outstanding");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Payments", new() { Exact = true }).First, "One row per payment");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("History", new() { Exact = true }).First, "Everything that happened");
        });

        await ClearAsync(scene);

        await scene.StepAsync("manage-3", async () =>
        {
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Reason (optional)"), "The customer changed their mind", delay: 40);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Cancel the plan" }), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            await scene.PauseAsync(2000);
        });
    }
}
