using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Customers, Guests and Privacy video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-merge", MergeAsync),
        ("04-retention", RetentionAsync),
        ("05-details", DetailsAsync),
        ("06-field", FieldAsync),
        ("07-staff", StaffAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string state = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, state ?? StatePath, Path.GetDirectoryName(directory), durations);

    private static ILocator Save(IPage page) => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");

    // Fills the Contact step of a checkout and goes on to the Shipping step.
    private static async Task FillContactAsync(IPage page, string name, string email)
    {
        await page.Locator("#Contact_Name").FillAsync(name);
        await page.Locator("#Contact_Email").FillAsync(email);
    }
}
