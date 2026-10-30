using Microsoft.Playwright;

namespace Recorder;

// Scratch helper: runs the commands of C:/cavideo/comms-g/explore.txt in a signed-in page. Not committed.
public static partial class Scenes
{
    public static async Task ExploreAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
            DeviceScaleFactor = Scene.Scale,
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(20_000);

        foreach (var raw in File.ReadAllLines(Environment.GetEnvironmentVariable("EXPLORE_FILE") ?? "C:/cavideo/comms-g/explore.txt"))
        {
            var line = raw.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var space = line.IndexOf(' ');
            var cmd = space < 0 ? line : line[..space];
            var arg = space < 0 ? "" : line[(space + 1)..];
            var parts = arg.Split(" | ");

            try
            {
                switch (cmd)
                {
                    case "goto":
                        await page.GotoAsync(BaseUrl + arg, new() { WaitUntil = WaitUntilState.NetworkIdle });
                        break;
                    case "click":
                        await page.Locator(arg).First.ClickAsync();
                        await Task.Delay(700);
                        break;
                    case "clicknav":
                        await page.RunAndWaitForNavigationAsync(() => page.Locator(arg).First.ClickAsync());
                        await Task.Delay(500);
                        break;
                    case "fill":
                        await page.Locator(parts[0]).First.FillAsync(parts[1]);
                        break;
                    case "type":
                        await page.Locator(parts[0]).First.PressSequentiallyAsync(parts[1]);
                        break;
                    case "select":
                        await page.Locator(parts[0]).First.SelectOptionAsync(new SelectOptionValue { Label = parts[1] });
                        break;
                    case "tick":
                        await page.Locator(parts[0]).First.SetCheckedAsync(parts[1] == "1");
                        break;
                    case "eval":
                        Console.WriteLine(await page.EvaluateAsync<System.Text.Json.JsonElement>(arg));
                        break;
                    case "drag":
                    {
                        var box = await page.Locator(parts[0]).First.BoundingBoxAsync();
                        float sx = box.X + 40; float sy = box.Y + 20;
                        await page.Mouse.MoveAsync(sx, sy);
                        await page.Mouse.DownAsync();
                        await page.Mouse.MoveAsync(sx + float.Parse(parts[1]), sy + float.Parse(parts[2]), new() { Steps = 20 });
                        await page.Mouse.UpAsync();
                        await Task.Delay(600);
                        break;
                    }
                    case "link":
                    {
                        var a = await page.Locator(parts[0]).First.BoundingBoxAsync();
                        var b = await page.Locator(parts[1]).First.BoundingBoxAsync();
                        await page.Mouse.MoveAsync(a.X + a.Width / 2, a.Y + a.Height / 2);
                        await page.Mouse.DownAsync();
                        await page.Mouse.MoveAsync(b.X + b.Width / 2, b.Y + b.Height / 2, new() { Steps = 25 });
                        await page.Mouse.UpAsync();
                        await Task.Delay(800);
                        break;
                    }
                    case "wait":
                        await Task.Delay(int.Parse(arg));
                        break;
                    case "url":
                        Console.WriteLine(page.Url);
                        break;
                    case "shot":
                        await page.ScreenshotAsync(new() { Path = $"C:/cavideo/comms-g/shots/{parts[0]}.png", FullPage = parts.Length > 1 });
                        break;
                    default:
                        Console.WriteLine("? " + line);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"FAILED {line}: {e.Message.Split('\n')[0]}");
            }
        }

        await context.CloseAsync();
    }
}
