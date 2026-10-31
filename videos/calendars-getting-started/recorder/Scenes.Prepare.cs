using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    /// <summary>
    /// Seeds a new site (set up with the Blank recipe, site name Contoso, time zone America/New_York, user admin) before
    /// recording, outside the capture: turns on the calendar features, adds the fake teams (roles) and people, lets every
    /// signed-in person into the admin, makes Maria a manager (who may see others' schedules), and adds the calendars with
    /// their events (see <see cref="CalendarSeed"/>).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var feature in new[] { "CrestApps.OrchardCore.Calendars", "CrestApps.OrchardCore.Calendars.Teams", "CrestApps.OrchardCore.Calendars.Resources", "OrchardCore.Deployment" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            var enable = page.Locator($"a[href$=\"/{feature}/Enable\"]");

            if (await enable.CountAsync() > 0)
            {
                await page.RunAndWaitForNavigationAsync(() => enable.First.ClickAsync());
            }
        }

        foreach (var role in new[] { "Sales Team", "Support Team", "Facilities", "Managers" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Roles/Create");
            await page.Locator("#RoleName").FillAsync(role);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).Last.ClickAsync());
        }

        // Managers may see anyone's schedule; everyone who is signed in may open the admin.
        foreach (var (role, permission) in new[] { ("Managers", "ListCalendarEvents"), ("Authenticated", "AccessAdminPanel") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Roles/Edit/" + role);
            await page.Locator($"[id='Checkbox.{permission}']").CheckAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        }

        var users = new (string Name, string[] Roles)[]
        {
            ("jamie.chen", ["Sales Team"]), ("maria.lopez", ["Sales Team", "Managers"]), ("sam.patel", ["Sales Team"]),
            ("alex.kim", ["Support Team"]), ("priya.shah", ["Support Team"]), ("dana.reyes", ["Facilities"]),
        };

        foreach (var (name, roles) in users)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Users/Create");
            await page.Locator("[name='User.UserName']").FillAsync(name);
            await page.Locator("[name='User.Email']").FillAsync(name + "@example.com");
            await page.Locator("[name='User.Password']").FillAsync("Password1!");
            await page.Locator("[name='User.PasswordConfirmation']").FillAsync("Password1!");

            foreach (var role in roles)
            {
                var index = await page.Locator("input[type=hidden][name$='.Role']").EvaluateAllAsync<int>($"els => els.findIndex(e => e.value === '{role}')");
                await page.Locator($"#User_Roles_{index}__IsSelected").CheckAsync();
            }

            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        }

        // The admin's personal calendar is made when the calendar is first opened.
        await CalendarSeed.RunAsync(page, BaseUrl);
        await context.CloseAsync();
    }
}
