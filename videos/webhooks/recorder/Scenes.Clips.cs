using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static async Task DumpAsync(Scene scene, string label)
    {
        Console.WriteLine(label + ": " + scene.Page.Url + " :: " + (await scene.Page.Locator("body").InnerTextAsync()).Replace("\n", " | ")[..Math.Min(900, (await scene.Page.Locator("body").InnerTextAsync()).Length)]);
        Console.WriteLine(label + " LINKS: " + string.Join(" ; ", await scene.Page.Locator("a, button").EvaluateAllAsync<string[]>("els => els.map(e => e.textContent.trim().slice(0, 25) + '[' + (e.getAttribute('href') || '') + ']').filter(x => /eliver|secret|ning|test|again|Edit|Delete|Filter/i.test(x))")));
    }

    // ----- Clips -----

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/commerce/webhooks");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/commerce/webhooks"), "Commerce > Webhooks");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add webhook" }).First);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "Name"), "Warehouse", delay: 60);
            await scene.TypeAsync(Field(scene, "Url"), "https://hooks.contoso.example/store", delay: 35);
            await scene.HighlightAsync(Field(scene, "Url"), "The receiver's address");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "IsEnabled"), "On");
        });

        await ClearAsync(scene);

        await scene.StepAsync("add-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "AllEvents"), "Every event, those added later too");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator("label[for='event-order-paid']"), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("label[for='event-order-canceled']"), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("label[for='event-shipment-shipped']"), pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator("label[for='event-order-paid']"), "Choose at least one event");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
            File.WriteAllText(Path.Combine(Root, "webhook-id.txt"), scene.Page.Url.Split('/').Last());
            await scene.PauseAsync(2500);
        });
    }

    private static string WebhookId => File.ReadAllText(Path.Combine(Root, "webhook-id.txt")).Trim();

    private static async Task DeliveriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("del-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item.item").Filter(new() { HasTextString = "Awaiting payment" }).Locator("a[href*='/Admin/orders/']").First);
            await scene.TypeAsync(scene.Page.GetByLabel("Why"), "Out of stock", delay: 50);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Cancel order" }), pauseAfter: 3000);
            await scene.HighlightAsync(scene.Page.GetByText("Canceled", new() { Exact = true }).First, "An order is canceled");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
        });

        await scene.StepAsync("del-2", async () =>
        {
            await scene.NavigateAsync("/Admin/commerce/webhooks/Deliveries/" + WebhookId);
            await scene.HighlightAsync(Row(scene, "order.canceled"), "Event, status, answer, attempts and next try");
            await scene.PauseAsync(5000);
            await ClearAsync(scene);
        });

        await scene.StepAsync("del-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send a test event" }), "Send a test event");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send a test event" }), pauseAfter: 6000);
        });

        await scene.StepAsync("del-4", async () =>
        {
            await scene.HighlightAsync(Row(scene, "order.canceled").GetByRole(AriaRole.Button, new() { Name = "Send again" }), "Send again");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(Row(scene, "order.canceled").GetByRole(AriaRole.Button, new() { Name = "Send again" }), pauseAfter: 5000);
        });
    }

    private static async Task SecretAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/commerce/webhooks/Edit/" + WebhookId);
        await scene.StartCaptureAsync();

        await scene.StepAsync("sec-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Signing secret").First, "The signing secret");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "New secret" }), "New secret");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "New secret" }), pauseAfter: 1500);

            if (await scene.Page.Locator("#modalOkButton").CountAsync() > 0)
            {
                await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            }

            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("sec-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Delete" }).First, "Delete removes the webhook and its log");
            await scene.PauseAsync(3000);
        });
    }
}
