using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");

    // Opens a product from the list of its content type, as an editor does.
    private static async Task OpenProductAsync(Scene scene, string type, string title)
    {
        await scene.NavigateAsync($"/Admin/Contents/ContentItems/{type}");
        await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, title));
    }

    // ----- Clips -----

    private static async Task ProductAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Tour");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prod-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Alpine Lakes Tour"));
            await Field(scene, "BookingPart_Kind").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 100);
            await scene.HighlightAsync(Field(scene, "BookingPart_Kind").First, "What's booked");
        });

        await ClearAsync(scene);

        await scene.StepAsync("prod-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "BookingPart_Capacity"), "Capacity");
        });

        await ClearAsync(scene);

        await scene.StepAsync("prod-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "BookingPart_PricePerPerson"), "The price is per person");
        });

        await ClearAsync(scene);

        await scene.StepAsync("prod-4", async () =>
        {
            await scene.HighlightAsync(Field(scene, "BookingPart_Occupancies_0__Label"), "Options");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "BookingPart_Occupancies_1__PriceAdjustment"), "A single supplement");
        });

        await ClearAsync(scene);

        await scene.StepAsync("prod-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Manage dates").First, "Manage dates");
        });
    }

    private static async Task DatesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Tour");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dates-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Alpine Lakes Tour"));
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/bookings/Dates']").First);
            await scene.HighlightAsync(scene.Page.Locator("table").First, "Date, booked, capacity, price");
        });

        await ClearAsync(scene);

        await scene.StepAsync("dates-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "NewDate"), "2027-03-17", delay: 60);
            await scene.TypeAsync(Field(scene, "NewCapacity"), "12", delay: 90);
            await scene.TypeAsync(Field(scene, "NewPrice"), "1150", delay: 70);
            await scene.HighlightAsync(Field(scene, "NewPrice"), "Its own price");
        });

        await ClearAsync(scene);

        await scene.StepAsync("dates-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add date" }));
            await scene.HighlightAsync(scene.Page.Locator("table").First);
        });
    }

    private static async Task KindsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Room");
        await scene.StartCaptureAsync();

        await scene.StepAsync("kind-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Lakeview Room"));
            await Field(scene, "BookingPart_MinNights").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 100);
            await scene.HighlightAsync(Field(scene, "BookingPart_MinNights"), "Fewest nights");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "BookingPart_MaxNights"), "Most nights");
        });

        await ClearAsync(scene);

        await scene.StepAsync("kind-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "BookingPart_WeekendPrice"), "The weekend price");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "BookingPart_DaysAhead"), "Days ahead");
        });

        await ClearAsync(scene);

        await scene.StepAsync("kind-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Appointment");
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Sports Massage, 60 minutes"));
            await Field(scene, "BookingPart_Hours_0__Opens").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(Field(scene, "BookingPart_Hours_1__Open"), "Days open");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "BookingPart_Hours_1__Opens"), "Opens");
            await scene.PauseAsync(2000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "BookingPart_SlotMinutes"), "Minutes a slot");
        });
    }

    private static async Task RulesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Tour");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rule-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Alpine Lakes Tour"));
            await Field(scene, "BookingPart_DepositPercent").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 100);
            await scene.HighlightAsync(Field(scene, "BookingPart_DepositPercent"), "Deposit (%)");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "BookingPart_BalanceDueDaysBefore"), "Rest due (days before)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("rule-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "BookingPart_CancellationTiers_0__DaysBefore"), "A step of the policy");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "BookingPart_CancellationTiers_1__FeePercent"), "Fee (%)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("rule-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "BookingPart_RequestToBook"), "Confirm each booking before it's paid");
        });

        await ClearAsync(scene);

        await scene.StepAsync("rule-4", async () =>
        {
            await scene.HighlightAsync(Field(scene, "BookingPart_AskForTravelers"), "Ask for each traveler's details");
        });
    }

    private static async Task BuyerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/tours/alpine-lakes-tour");
        await scene.StartCaptureAsync();

        await scene.StepAsync("buy-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("select[name='details[booking.date]']"), "Date");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("select[name='details[booking.occupancy]']"), "Option");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Pay a 15% deposit now").First, "The deposit");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Canceled 60 or more days before").First, "The cancellation policy");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buy-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[name='details[booking.date]']"), "Tuesday, February 16, 2027 (12 left, $1,150)", pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator("select[name='details[booking.date]']"), "A date with its own price");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buy-3", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, pauseAfter: 2500);
            await scene.NavigateAsync("/cart");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a,button").Filter(new() { HasTextString = "Checkout" }).First);
            await scene.Page.Locator("input[name$='FullName.Text']").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='FullName.Text']").First, "Each traveler");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buy-4", async () =>
        {
            await scene.NavigateAsync("/bookings");
            await scene.ClickAsync(scene.Page.GetByLabel("From", new() { Exact = true }), pauseAfter: 300);
            await scene.Page.GetByLabel("From", new() { Exact = true }).FillAsync("2026-12-18");
            await scene.ClickAsync(scene.Page.GetByLabel("To", new() { Exact = true }), pauseAfter: 300);
            await scene.Page.GetByLabel("To", new() { Exact = true }).FillAsync("2026-12-21");
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true }).Last);
        });
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/bookings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/bookings"), "Commerce > Bookings");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Jamie Chen"), "A booking");
        });

        await ClearAsync(scene);

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select button.dropdown-toggle").First, pauseAfter: 3500);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByPlaceholder("Search by buyer or product"), "Search");
        });

        await ClearAsync(scene);

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Calendar"));
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Next month" }).First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Next month" }).First);
        });
    }

    private static async Task DetailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/bookings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("det-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Jamie Chen").GetByRole(AriaRole.Link, new() { Name = "View" }));
            await scene.HighlightAsync(scene.Page.GetByText("Travelers", new() { Exact = true }).First, "Travelers");
        });

        await ClearAsync(scene);

        await scene.StepAsync("det-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).First, "Complete");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "No-show" }).First, "No-show");
        });

        await ClearAsync(scene);

        await scene.StepAsync("det-3", async () =>
        {
            await scene.Page.GetByText("the policy keeps").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("the policy keeps").First, "What the policy keeps");
        });

        await ClearAsync(scene);

        await scene.StepAsync("det-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("What the policy gives back").First, "Refund");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Cancel the booking" }).First, "Cancel the booking");
        });
    }
}
