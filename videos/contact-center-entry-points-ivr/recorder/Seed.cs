using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// Seeds the demo site outside the capture (addresses, hours, queues, voice media). Fake data only.
/// </summary>
public static class Seed
{
    public static async Task<(IBrowserContext Context, IPage Page)> OpenAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = Scenes.StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        return (context, page);
    }

    public static async Task SaveAsync(IPage page)
        => await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());

    /// <summary>Sets a (bootstrap-select) drop-down by the text of an option.</summary>
    public static async Task PickAsync(IPage page, string selectId, string text)
    {
        await page.EvaluateAsync(@"([id, text]) => {
            const s = document.getElementById(id);
            const o = [...s.options].find(o => o.text.trim().toLowerCase().includes(text.toLowerCase()));
            if (!o) throw new Error('No option ' + text + ' in ' + id);
            s.value = o.value;
            if (window.jQuery && jQuery.fn.selectpicker) { jQuery(s).selectpicker('refresh'); }
            s.dispatchEvent(new Event('change', { bubbles: true }));
        }", new object[] { selectId, text });
    }

    public static async Task EnableFeaturesAsync(IPage page, params string[] features)
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

    public static async Task AddressAsync(IPage page, string name, string number, bool voice, bool sms, string description = null)
    {
        await page.GotoAsync(Scenes.BaseUrl + "/Admin/omnichannel/channel-endpoints/create/PhoneNumber");
        await page.Locator("#OmnichannelChannelEndpoint_DisplayText").FillAsync(name);
        await page.Locator("#OmnichannelChannelEndpoint_Value").FillAsync(number);

        if (voice)
        {
            await page.Locator("#OmnichannelChannelEndpoint_Capabilities_0").CheckAsync();
        }

        if (sms)
        {
            await page.Locator("#OmnichannelChannelEndpoint_Capabilities_1").CheckAsync();
        }

        if (description != null)
        {
            await page.Locator("#OmnichannelChannelEndpoint_Description").FillAsync(description);
        }

        await SaveAsync(page);
        Console.WriteLine($"Address {name}");
    }

    public static async Task BusinessHoursAsync(IPage page, string name, string description, string timeZone)
    {
        await page.GotoAsync(Scenes.BaseUrl + "/Admin/contact-center/business-hours/create");
        await page.Locator("#BusinessHoursCalendar_Name").FillAsync(name);
        await page.Locator("#BusinessHoursCalendar_Description").FillAsync(description);
        await PickAsync(page, "BusinessHoursCalendar_TimeZoneId", timeZone);

        // Days 0..6 start on Sunday; open Monday to Friday, 9 to 5.
        for (var day = 0; day < 7; day++)
        {
            var open = page.Locator($"#BusinessHoursCalendar_Days_{day}__IsOpen");
            var weekday = day is >= 1 and <= 5;

            if (await open.IsCheckedAsync() != weekday)
            {
                await open.SetCheckedAsync(weekday);
            }

            if (weekday)
            {
                await page.Locator($"#BusinessHoursCalendar_Days_{day}__OpenTime").FillAsync("09:00");
                await page.Locator($"#BusinessHoursCalendar_Days_{day}__CloseTime").FillAsync("17:00");
            }
        }

        await SaveAsync(page);
        Console.WriteLine($"Hours {name}");
    }

    public static async Task QueueAsync(IPage page, string name, string description)
    {
        await page.GotoAsync(Scenes.BaseUrl + "/Admin/contact-center/queues/create");
        await page.Locator("#ActivityQueue_Name").FillAsync(name);
        await page.Locator("#ActivityQueue_Description").FillAsync(description);
        await SaveAsync(page);
        Console.WriteLine($"Queue {name}");
    }

    public static async Task VoiceMediaAsync(IPage page, string name, string description, string file)
    {
        await page.GotoAsync(Scenes.BaseUrl + "/Admin/contact-center/voice-media/create");
        await page.Locator("#VoiceMediaItem_Name").FillAsync(name);
        await page.Locator("#VoiceMediaItem_Description").FillAsync(description);
        await page.Locator("#VoiceMediaItem_Audio").SetInputFilesAsync(file);
        await SaveAsync(page);
        Console.WriteLine($"Voice media {name}");
    }

    public static async Task PickManyAsync(IPage page, string selectId, params string[] texts)
    {
        await page.EvaluateAsync(@"([id, texts]) => {
            const s = document.getElementById(id);
            for (const o of s.options) { o.selected = texts.some(t => o.text.trim().toLowerCase().startsWith(t.toLowerCase())); }
            if (window.jQuery && jQuery.fn.selectpicker) { jQuery(s).selectpicker('refresh'); }
            s.dispatchEvent(new Event('change', { bubbles: true }));
        }", new object[] { selectId, texts });
    }

    public static async Task SkillAsync(IPage page, string name, string description)
    {
        await page.GotoAsync(Scenes.BaseUrl + "/Admin/contact-center/skills/create");
        await page.Locator("input[name$='.Name']").First.FillAsync(name);
        await page.Locator("textarea[name$='.Description']").First.FillAsync(description);
        await SaveAsync(page);
        Console.WriteLine($"Skill {name}");
    }

    public static async Task EntitlementAsync(IPage page, string user, string[] queues)
    {
        await page.GotoAsync(Scenes.BaseUrl + "/Admin/contact-center/agent-entitlements/create");
        await page.Locator("button[data-id='agent-entitlement-user-selector']").ClickAsync();
        await page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = user }).First.ClickAsync();
        await PickManyAsync(page, "AllowedQueueIds", queues);
        await SaveAsync(page);
        Console.WriteLine($"Entitlement {user}");
    }
}
