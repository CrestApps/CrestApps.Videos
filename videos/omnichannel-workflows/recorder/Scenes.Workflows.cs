using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private const string TextWorkflow = "Text customers the dialer could not reach";

    private const string RunsWorkflow = "Look up agent numbers";

    private static ILocator LabelBox(Scene scene, string label)
        => scene.Page.Locator($"xpath=//label[starts-with(normalize-space(),'{label}')]/parent::*").First;

    // Opens the editor of a workflow from the list, so a clip does not depend on its numeric id.
    private static async Task OpenWorkflowAsync(Scene scene, string name)
    {
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator($"a:has-text('{name}')").First.ClickAsync());
        await scene.Page.WaitForSelectorAsync("button:has-text('Add Task')", new() { Timeout = 30000 });
        await scene.PauseAsync(1200);
    }

    // Add Task or Add Event, then a category; leaves the modal open.
    private static async Task OpenPickerAsync(Scene scene, string button, string category, int pauseAfter = 1000)
    {
        await scene.ClickAsync(scene.Page.Locator($"button:has-text('{button}')").First, pauseAfter: 900);
        await scene.ClickAsync(scene.Page.Locator($".modal.show a[href='#{category}']").First, pauseAfter: pauseAfter);
    }

    // Opens the settings form of a task from the picker.
    private static async Task AddTaskFormAsync(Scene scene, string category, string activity)
    {
        await OpenPickerAsync(scene, "Add Task", category, 600);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($".modal.show a[href*='{activity}/Add']").First);
        await scene.PauseAsync(500);
    }

    private static async Task CloseModalAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.Locator(".modal.show .btn-close").First, pauseAfter: 700);
    }

    // Makes a drop-down show its options as a list, since a headless browser cannot draw it open.
    private static async Task ShowOptionsAsync(Scene scene, string selector, int rows)
        => await scene.Page.EvaluateAsync($"() => {{ const s = document.querySelector(\"{selector}\"); s.size = {rows}; }}");

    private static async Task HighlightForAsync(Scene scene, ILocator target, string caption, int ms)
    {
        await scene.HighlightAsync(target, caption);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cre-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Workflows/Types"), "Design > Workflows");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var create = scene.Page.Locator("a:has-text('Create Workflow')").First;
            await scene.HighlightAsync(create, "Create Workflow");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(create);
        });

        await scene.StepAsync("cre-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), TextWorkflow, delay: 50);
            await scene.HighlightAsync(LabelBox(scene, "Name"), "Name");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").First);
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("cre-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button:has-text('Add Event')").First, "Add Event");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Event')").First, pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator(".modal.show a[href='#contact-center']").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".modal.show a[href='#omnichannel-c-r-m']").First, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show a[href='#contact-center']").First, "Contact Center");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".modal.show a[href='#omnichannel-c-r-m']").First, "Omnichannel CRM");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task EventAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene, TextWorkflow);
        await scene.StartCaptureAsync();

        await scene.StepAsync("evt-1", async () =>
        {
            await OpenPickerAsync(scene, "Add Event", "contact-center", 1200);
            await HighlightForAsync(scene, scene.Page.Locator(".modal.show .col.activity:visible").Filter(new() { HasTextString = "Contact Center Event" }).First, "Contact Center Event", 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='ContactCenterEvent/Add']").First);
            await scene.HighlightAsync(LabelBox(scene, "Event type"), "Event type");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("evt-2", async () =>
        {
            await ShowOptionsAsync(scene, "select[name*=EventType]", 13);
            await scene.PauseAsync(900);

            foreach (var group in new[] { "Interaction", "Activity", "Routing & queues", "Agent", "Offer", "Dialer", "Callback", "Call", "Shared voicemail", "Recording", "Supervision", "Secure capture" })
            {
                await scene.Page.EvaluateAsync($"() => {{ const s = document.querySelector(\"select[name*=EventType]\"); const g = [...s.querySelectorAll(\"optgroup\")].find(x => x.label === \"{group}\"); s.scrollTop = g.offsetTop - s.offsetTop - 4; }}");
                await scene.PauseAsync(850);
            }
        });

        await scene.StepAsync("evt-3", async () =>
        {
            await scene.Page.Locator("select[name*=EventType]").SelectOptionAsync(new SelectOptionValue { Label = "Dialer attempt completed" });
            await scene.Page.EvaluateAsync("() => { const s = document.querySelector(\"select[name*=EventType]\"); const o = s.options[s.selectedIndex]; s.scrollTop = o.offsetTop - s.offsetTop - 60; }");
            await scene.PauseAsync(2500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").First);
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("evt-4", async () =>
        {
            await HighlightForAsync(scene, scene.Page.GetByText("Matched", new() { Exact = true }).First, "Matched", 2800);
            await HighlightForAsync(scene, scene.Page.GetByText("Ignored", new() { Exact = true }).First, "Ignored", 2800);
        });
    }

    private static async Task PickerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene, TextWorkflow);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pick-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Add Task')").First, pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator(".modal.show a[href='#contact-center']").First, pauseAfter: 1500);
        });

        await scene.StepAsync("pick-2", async () =>
        {
            foreach (var task in new[] { "Enqueue Activity", "Find Agent Numbers", "Schedule Callback", "Schedule Dialer Retry", "Send Text Message", "Set Agent Presence", "Start Call Recording", "Place Call or Send Message", "Stop Call Recording", "Hand Off to Live Agent" })
            {
                await HighlightForAsync(scene, scene.Page.Locator(".modal.show .col.activity:visible").Filter(new() { HasTextString = task }).First, task, 850);
            }
        });
    }

    private static async Task SmsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene, TextWorkflow);
        await scene.StartCaptureAsync();

        await scene.StepAsync("sms-1", async () =>
        {
            await OpenPickerAsync(scene, "Add Task", "contact-center", 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='SendTextMessageTask/Add']").First);
            await scene.TypeAsync(scene.Page.Locator("#SendTextMessageTask_From"), "+12125550100", delay: 28);
            await HighlightForAsync(scene, LabelBox(scene, "From"), "From", 600);
            await scene.TypeAsync(scene.Page.Locator("#SendTextMessageTask_To"), "{{ Workflow.Input.Data.PhoneNumber }}", delay: 10);
            await HighlightForAsync(scene, LabelBox(scene, "To"), "To", 900);
        });

        await scene.StepAsync("sms-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#SendTextMessageTask_Body"), "Sorry we missed you. Reply to this text and we will call you back.", delay: 16);
            await HighlightForAsync(scene, LabelBox(scene, "Message"), "Message", 1200);
            await HighlightForAsync(scene, LabelBox(scene, "Agent"), "Agent", 2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").First);
            await scene.PauseAsync(1000);
        });

        await scene.StepAsync("sms-3", async () =>
        {
            var task = scene.Page.Locator(".activity:not(.activity-start)").Filter(new() { HasTextString = "Send Text Message" }).First;
            var box = await task.BoundingBoxAsync();
            await scene.MoveToAsync(task, -60, -20);
            await scene.Page.Mouse.DownAsync();
            await scene.Page.Mouse.MoveAsync((float)(box!.X + 60), (float)(box.Y + 40), new() { Steps = 4 });
            await scene.Page.Mouse.MoveAsync((float)(box.X + 480), (float)(box.Y + 140), new() { Steps = 30 });
            await scene.Page.Mouse.UpAsync();
            await scene.PauseAsync(900);

            var label = await scene.Page.GetByText("Matched", new() { Exact = true }).First.BoundingBoxAsync();
            var endpoints = scene.Page.Locator(".jtk-endpoint");
            double best = double.MaxValue, fx = 0, fy = 0;

            for (var i = 0; i < await endpoints.CountAsync(); i++)
            {
                var e = await endpoints.Nth(i).BoundingBoxAsync();
                var d = Math.Abs((e!.Y + e.Height / 2) - (label!.Y + label.Height / 2)) + Math.Abs(e.X - label.X) / 20;

                if (d < best)
                {
                    best = d;
                    fx = e.X + e.Width / 2;
                    fy = e.Y + e.Height / 2;
                }
            }

            var to = await task.BoundingBoxAsync();
            await scene.Page.Mouse.MoveAsync((float)fx, (float)fy, new() { Steps = 12 });
            await scene.Page.Mouse.DownAsync();
            await scene.Page.Mouse.MoveAsync((float)(to!.X + to.Width / 2), (float)(to.Y + to.Height / 2), new() { Steps = 40 });
            await scene.Page.Mouse.UpAsync();
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button:has-text('Save')").Last);
            await scene.PauseAsync(2500);
        });
    }

    private static async Task<string> HrefAsync(Scene scene, string activity)
        => (await scene.Page.Locator($"a[href*='{activity}/Add']").First.GetAttributeAsync("href"))!;

    private static async Task FormsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene, TextWorkflow);
        var place = await HrefAsync(scene, "StartOmnichannelActivityTask");
        var presence = await HrefAsync(scene, "SetAgentPresenceTask");
        var find = await HrefAsync(scene, "FindAgentNumbersTask");
        var enqueue = await HrefAsync(scene, "EnqueueActivityTask");
        var handoff = await HrefAsync(scene, "TransferToAgentTask");
        var callback = await HrefAsync(scene, "ScheduleCallbackTask");
        var retry = await HrefAsync(scene, "ScheduleDialerRetryTask");
        var record = await HrefAsync(scene, "StartCallRecordingTask");
        await scene.StartCaptureAsync();

        await scene.StepAsync("frm-1", async () =>
        {
            await scene.NavigateAsync(place);
            await HighlightForAsync(scene, LabelBox(scene, "Activity"), "Activity", 3500);
        });

        await scene.StepAsync("frm-2", async () =>
        {
            await scene.NavigateAsync(presence);
            await ShowOptionsAsync(scene, "#SetAgentPresenceTask_Status", 9);
            await HighlightForAsync(scene, LabelBox(scene, "Status"), "Status", 3500);
            await HighlightForAsync(scene, LabelBox(scene, "Reason"), "Reason", 1200);
        });

        await scene.StepAsync("frm-3", async () =>
        {
            await scene.NavigateAsync(find);
            await HighlightForAsync(scene, LabelBox(scene, "User name"), "User name", 4000);
        });

        await scene.StepAsync("frm-4", async () =>
        {
            await scene.NavigateAsync(enqueue);
            await ShowOptionsAsync(scene, "#EnqueueActivityTask_Priority", 6);
            await HighlightForAsync(scene, LabelBox(scene, "Priority"), "Priority", 4000);
        });

        await scene.StepAsync("frm-5", async () =>
        {
            await scene.NavigateAsync(handoff);
            await HighlightForAsync(scene, LabelBox(scene, "Reason"), "Reason", 1800);
            await HighlightForAsync(scene, LabelBox(scene, "Summary"), "Summary", 1800);
            await HighlightForAsync(scene, LabelBox(scene, "Queue"), "Queue", 1500);
        });

        await scene.StepAsync("frm-6", async () =>
        {
            await scene.NavigateAsync(callback);
            await HighlightForAsync(scene, LabelBox(scene, "Destination"), "Destination", 1300);
            await HighlightForAsync(scene, LabelBox(scene, "Delay (minutes)"), "Delay (minutes)", 1800);
            await HighlightForAsync(scene, LabelBox(scene, "Campaign"), "Campaign, Queue, Contact", 1200);
        });

        await scene.StepAsync("frm-7", async () =>
        {
            await scene.NavigateAsync(retry);
            await HighlightForAsync(scene, LabelBox(scene, "Delay in minutes"), "Delay in minutes", 4000);
        });

        await scene.StepAsync("frm-8", async () =>
        {
            await scene.NavigateAsync(record);
            await HighlightForAsync(scene, LabelBox(scene, "Interaction"), "Interaction", 2500);
        });
    }

    private static async Task CrmAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenWorkflowAsync(scene, TextWorkflow);
        var convert = await HrefAsync(scene, "ConvertLeadTask");
        await scene.StartCaptureAsync();

        await scene.StepAsync("crm-1", async () =>
        {
            await OpenPickerAsync(scene, "Add Event", "omnichannel-c-r-m", 1000);
            await HighlightForAsync(scene, scene.Page.Locator(".modal.show .col.activity:visible").Filter(new() { HasTextString = "Lead Converted" }).First, "Lead Converted", 3500);
        });

        await scene.StepAsync("crm-2", async () =>
        {
            await scene.NavigateAsync(convert);
            await HighlightForAsync(scene, LabelBox(scene, "Lead"), "Lead", 1300);
            await ShowOptionsAsync(scene, "#ConvertLeadTask_AccountMode", 3);
            await HighlightForAsync(scene, LabelBox(scene, "Account"), "Account", 1100);
            await HighlightForAsync(scene, scene.Page.Locator("label:has-text('Create an opportunity')").First.Locator("xpath=ancestor::*[1]"), "Create an opportunity", 1000);
            await ShowOptionsAsync(scene, "#ConvertLeadTask_OpportunityContentType", 3);
            await HighlightForAsync(scene, LabelBox(scene, "Opportunity type"), "Opportunity type", 1000);
            await ShowOptionsAsync(scene, "#ConvertLeadTask_OpenActivities", 2);
            await HighlightForAsync(scene, LabelBox(scene, "Other open activities"), "Other open activities", 1500);
        });
    }

    private static async Task RunsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($"a:has-text('{RunsWorkflow}')").First);
            await scene.Page.WaitForSelectorAsync("button:has-text('Add Task')", new() { Timeout = 30000 });
            await scene.PauseAsync(800);
            await HighlightForAsync(scene, scene.Page.Locator(".activity.activity-start").First, "Agent presence changed", 1700);
            await HighlightForAsync(scene, scene.Page.Locator(".activity:not(.activity-start)").Filter(new() { HasTextString = "Find Agent Numbers" }).First, "Find Agent Numbers", 1700);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.telephony-soft-phone__toggle").First, pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator("[data-contact-center-presence-toggle]").First, pauseAfter: 800);
            await scene.ClickAsync(scene.Page.Locator("[data-presence-status='Available']").First, pauseAfter: 1500);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.telephony-soft-phone__toggle").First, pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn[href*='/Instances/Index']").First);
            await scene.PauseAsync(800);
            await HighlightForAsync(scene, scene.Page.Locator("span.badge:has-text('Finished')").First, "Status", 2500);
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Workflow/Details/']").First);
            await scene.PauseAsync(1000);
            await HighlightForAsync(scene, scene.Page.Locator("a:has-text('Restart'), button:has-text('Restart')").First, "Restart", 1000);
            await HighlightForAsync(scene, scene.Page.Locator("a:has-text('Delete'), button:has-text('Delete')").First, "Delete", 1000);
            await scene.NavigateAsync("/Admin/Settings/WorkflowTrimmingSettings");
            await scene.PauseAsync(500);
            await HighlightForAsync(scene, LabelBox(scene, "Retention period"), "Retention period", 1800);
        });
    }
}
