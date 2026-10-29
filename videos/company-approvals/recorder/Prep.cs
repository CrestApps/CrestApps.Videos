using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // Enables features by id, as the Features page does (POST with the antiforgery token), and prints the answer codes.
    private static async Task EnableFeaturesAsync(IPage page, params string[] features)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Features");

        foreach (var feature in features)
        {
            var status = await page.EvaluateAsync<int>(
                "async id => { const token = document.querySelector('input[name=__RequestVerificationToken]').value; const r = await fetch('/Admin/Features/' + id + '/Enable', { method: 'POST', credentials: 'include', headers: { RequestVerificationToken: token } }); return r.status; }",
                feature);
            Console.WriteLine($"Enabled {feature}: {status}");
        }
    }

    // Signs a fake buyer in, and saves the session as state-<user>.json in the workspace.
    private static async Task SaveSignInAsync(IBrowser browser, string user)
    {
        var buyer = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var buyerPage = await buyer.NewPageAsync();
        buyerPage.SetDefaultTimeout(60_000);
        await buyerPage.GotoAsync(BaseUrl + "/Login");
        await buyerPage.Locator("#LoginForm_UserName").FillAsync(user);
        await buyerPage.Locator("#LoginForm_Password").FillAsync(Password);
        await buyerPage.Locator("#LoginForm_Password").PressAsync("Enter");
        await buyerPage.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));
        await buyer.StorageStateAsync(new() { Path = Path.Combine(Root, $"state-{user}.json") });
        await buyer.CloseAsync();
    }

    // Runs a recipe of Tools > Recipes by its module's folder and file name, as its Run button does, and prints the answer code.
    private static async Task RunRecipeAsync(IPage page, string basePath, string fileName)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Recipes");
        var status = await page.EvaluateAsync<string>(
            "async args => { const token = document.querySelector('input[name=__RequestVerificationToken]').value; const r = await fetch('/Admin/Recipes/Execute?basePath=' + encodeURIComponent(args[0]) + '&fileName=' + encodeURIComponent(args[1]), { method: 'POST', credentials: 'include', headers: { RequestVerificationToken: token } }); const t = await r.text(); return r.status + ' ' + (t.match(/The recipe[^<]*/) || [''])[0]; }",
            new[] { basePath, fileName });
        Console.WriteLine($"Recipe {fileName}: {status}");
    }
}
