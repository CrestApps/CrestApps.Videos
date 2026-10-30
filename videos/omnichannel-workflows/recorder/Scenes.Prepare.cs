using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // The workflow of the "Runs" chapter, made with the same screens as the on-camera one, outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
            DeviceScaleFactor = Scene.Scale,
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        await page.GotoAsync(BaseUrl + "/Admin/Workflows/Types");

        if (await page.Locator($"a:has-text('{RunsWorkflow}')").CountAsync() == 0)
        {
            await page.RunAndWaitForNavigationAsync(() => page.Locator("a:has-text('Create Workflow')").First.ClickAsync());
            await page.Locator("#Name").FillAsync(RunsWorkflow);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button:has-text('Save')").First.ClickAsync());

            await page.Locator("button:has-text('Add Event')").First.ClickAsync();
            await page.Locator(".modal.show a[href='#contact-center']").First.ClickAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator(".modal.show a[href*='ContactCenterEvent/Add']").First.ClickAsync());
            await page.Locator("select[name*=EventType]").SelectOptionAsync(new SelectOptionValue { Label = "Agent presence changed" });
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button:has-text('Save')").First.ClickAsync());

            await page.Locator("button:has-text('Add Task')").First.ClickAsync();
            await page.Locator(".modal.show a[href='#contact-center']").First.ClickAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator(".modal.show a[href*='FindAgentNumbersTask/Add']").First.ClickAsync());
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button:has-text('Save')").First.ClickAsync());
            await page.WaitForTimeoutAsync(1000);

            await ArrangeAndConnectAsync(page, "Find Agent Numbers", "Matched");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button:has-text('Save')").Last.ClickAsync());
            Console.WriteLine("Seeded " + RunsWorkflow);
        }

        await context.CloseAsync();
    }

    // Moves the task node away from the event, then drags from the event's outcome to it. Used by Prepare and the clips.
    private static async Task ArrangeAndConnectAsync(IPage page, string taskTitle, string outcome)
    {
        var task = page.Locator(".activity:not(.activity-start)").Filter(new() { HasTextString = taskTitle }).First;
        var box = await task.BoundingBoxAsync();
        await page.Mouse.MoveAsync((float)(box!.X + 30), (float)(box.Y + 20));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync((float)(box.X + 60), (float)(box.Y + 40), new() { Steps = 4 });
        await page.Mouse.MoveAsync((float)(box.X + 480), (float)(box.Y + 140), new() { Steps = 30 });
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(800);

        var label = await page.GetByText(outcome, new() { Exact = true }).First.BoundingBoxAsync();
        var endpoints = page.Locator(".jtk-endpoint");
        var count = await endpoints.CountAsync();
        double bestDistance = double.MaxValue;
        double fromX = 0, fromY = 0;

        for (var i = 0; i < count; i++)
        {
            var e = await endpoints.Nth(i).BoundingBoxAsync();

            if (e is null)
            {
                continue;
            }

            var distance = Math.Abs((e.Y + e.Height / 2) - (label!.Y + label.Height / 2)) + Math.Abs(e.X - label.X) / 20;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                fromX = e.X + e.Width / 2;
                fromY = e.Y + e.Height / 2;
            }
        }

        var to = await task.BoundingBoxAsync();
        await page.Mouse.MoveAsync((float)fromX, (float)fromY, new() { Steps = 10 });
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync((float)(to!.X + to.Width / 2), (float)(to.Y + to.Height / 2), new() { Steps = 40 });
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(1000);
    }
}
