using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    // ----- Clips -----

    private static async Task MergeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers?Options.Filter=All");
        await scene.StartCaptureAsync();

        await scene.StepAsync("merge-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/customers"), "Commerce > Customers");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Avery Stone"), "A guest customer");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Casey Rowe"), "A guest who made an account: Merged");
        });

        await ClearAsync(scene);

        await scene.StepAsync("merge-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Casey Rowe"));
            await scene.Page.GetByText("This guest's orders and addresses moved to").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("This guest's orders and addresses moved to"), "Moved to the account");
        });

        await ClearAsync(scene);

        await scene.StepAsync("merge-3", async () =>
        {
            await scene.NavigateAsync("/Admin/customers?Options.Filter=All");
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "casey"));
            await scene.Page.GetByText("Merged guest checkouts").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("Merged guest checkouts"), "Merged guest checkouts");
        });
    }

    private static async Task RetentionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("keep-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/customers"), "Settings > Commerce > Customers");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "ISite_CustomerPrivacySettings_InactiveGuestMonths"), "Erase inactive guests after (months)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("keep-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "ISite_CustomerPrivacySettings_GuestOrderMonths"), "Anonymize guest orders after (months)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("keep-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "ISite_CustomerCheckoutSettings_BillingAddress"), "Billing address at checkout");
        });
    }

    private static async Task DetailsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/Edit/CheckoutDetails");
        await scene.StartCaptureAsync();

        await scene.StepAsync("details-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Heading).First, "Design > Content Types > Checkout details");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add Field" }).First);
        });

        await scene.StepAsync("details-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "DisplayName"), "Gift message", delay: 60);
            await scene.ClickAsync(scene.Page.Locator("label[for='Fields_11_']"), pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator("label[for='Fields_11_']"), "Text Field");
        });

        await ClearAsync(scene);

        await scene.StepAsync("details-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save & Configure" }));
            await scene.PauseAsync(800);
            await scene.ClickAsync(Field(scene, "CheckoutDetails_Giftmessage_TextFieldSettingsDriver_Required"), pauseAfter: 600);
            await scene.HighlightAsync(Field(scene, "CheckoutDetails_Giftmessage_TextFieldSettingsDriver_Required"), "Required");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
        });
    }

    private static async Task FieldAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("guest"));

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("field-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, pauseAfter: 1500);
            await scene.NavigateAsync("/cart");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Checkout" }).First);
            await FillContactAsync(scene.Page, "Sam Carter", "sam.carter@example.test");
            await scene.Page.GetByLabel("Gift message").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.GetByLabel("Gift message"), "Gift message");
        });

        await ClearAsync(scene);

        await scene.StepAsync("field-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#checkout-submit"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".text-danger, .field-validation-error, .alert-danger").First, "It is required");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.TypeAsync(scene.Page.GetByLabel("Gift message"), "Happy birthday, Alex!", delay: 50);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
        });
    }

    private static async Task StaffAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/my-account");
        await scene.StartCaptureAsync();

        await scene.StepAsync("staff-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/my-account"), "Commerce > My Account");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "FirstName"), "Name");
        });
    }
}
