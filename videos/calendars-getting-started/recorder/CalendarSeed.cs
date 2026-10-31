using System.Text.Json;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// Seeds the demo site of the Calendars videos (outside the capture): the Company calendar, a personal calendar for
/// each fake person, a calendar for two teams, their events for two weeks, and two shared categories. The users and the
/// teams (roles "Sales Team", "Support Team", "Facilities") are created first, in the admin screens. It posts a
/// "Calendars" recipe step to Tools > JSON Import.
/// </summary>
public static class CalendarSeed
{
    private const string Tz = "America/New_York";

    public static async Task RunAsync(IPage page, string baseUrl)
    {
        // The people: user name -> id (the calendar's user search lists them).
        await page.GotoAsync(baseUrl + "/Admin/calendar");
        var usersJson = await page.EvaluateAsync<string>("async () => await (await fetch('/Admin/api/calendars/users/search?q=')).text()");
        var users = JsonDocument.Parse(usersJson).RootElement.EnumerateArray()
            .ToDictionary(item => item.GetProperty("text").GetString()!.Split(' ')[0], item => item.GetProperty("value").GetString()!);
        var adminCalendar = await page.EvaluateAsync<string>("() => JSON.parse(document.getElementById('calendar-view').dataset.config).writableCalendars[0]");

        object E(string id, string type, string title, string start, string end, string who = null, string[] attendees = null,
            string category = null, string recurrence = null, bool allDay = false, string location = null, string visibility = null)
            => new
            {
                itemId = id, eventType = type, title, start, end, allDay, timeZoneId = Tz, status = "Confirmed", location,
                categoryId = category, visibility,
                reminderMinutes = Array.Empty<int>(),
                recurrence = recurrence is null ? null : new { rule = recurrence },
                attendees = (attendees ?? []).Select((name, index) => new
                {
                    userId = users[name],
                    name,
                    role = index == 0 && who is not null ? "Organizer" : "Required",
                    response = index == 0 ? "Accepted" : "NeedsAction",
                }).ToArray(),
            };

        object Personal(string id, string name, string person, string color, object[] events)
            => new { itemId = id, name, type = "Personal", color, timeZoneId = Tz, ownerKind = "User", ownerId = users[person], visibility = "Private", Events = events };

        var calendars = new List<object>
        {
            new
            {
                itemId = "companycalendar01", name = "Company", type = "General", color = "#1971c2", timeZoneId = Tz, ownerKind = "User", ownerId = users["admin"], visibility = "Public",
                description = "Company-wide events",
                Events = new object[]
                {
                    E("allhands0000001", "Meeting", "All hands", "2026-10-05T09:00:00", "2026-10-05T10:00:00", recurrence: "FREQ=WEEKLY;BYDAY=MO"),
                    E("thanksgiving001", "Holiday", "Thanksgiving", "2026-11-26T00:00:00", "2026-11-27T00:00:00", allDay: true, category: "holidays0000001"),
                    E("planning0000001", "Meeting", "Quarterly planning", "2026-11-04T13:00:00", "2026-11-04T14:30:00", who: "admin", attendees: ["admin", "jamie.chen", "maria.lopez", "sam.patel"], location: "Boardroom"),
                    E("training0000001", "Meeting", "Security training", "2026-11-11T14:00:00", "2026-11-11T15:00:00", category: "training000001"),
                },
            },
            new
            {
                itemId = adminCalendar, name = "admin", type = "Personal", color = "#2f9e44", timeZoneId = Tz, ownerKind = "User", ownerId = users["admin"], visibility = "Private",
                Events = new object[]
                {
                    E("adminbudget0001", "Focus", "Review the budget", "2026-11-03T10:00:00", "2026-11-03T11:00:00"),
                    E("adminoneone0001", "Meeting", "One-to-one with Jamie", "2026-11-04T11:00:00", "2026-11-04T11:30:00", who: "admin", attendees: ["admin", "jamie.chen"]),
                    E("admindentist001", "Event", "Dentist", "2026-11-05T15:00:00", "2026-11-05T16:00:00", visibility: "Private"),
                },
            },
            Personal("jamiecalendar001", "jamie.chen", "jamie.chen", "#1c7ed6", [
                E("jamieclient0001", "Meeting", "Client call: Northwind", "2026-11-03T10:00:00", "2026-11-03T11:00:00"),
                E("jamieproposal01", "Focus", "Proposal writing", "2026-11-04T09:00:00", "2026-11-04T12:00:00"),
                E("jamiedemo000001", "Meeting", "Demo preparation", "2026-11-05T13:00:00", "2026-11-05T15:00:00"),
            ]),
            Personal("mariacalendar001", "maria.lopez", "maria.lopez", "#d6336c", [
                E("mariapartner001", "Event", "Partner lunch", "2026-10-28T12:00:00", "2026-10-28T13:30:00"),
                E("mariaprep000001", "Focus", "Pricing preparation", "2026-10-29T14:00:00", "2026-10-29T15:30:00"),
                E("mariapipeline01", "Meeting", "Pipeline review", "2026-11-02T14:00:00", "2026-11-02T15:00:00"),
                E("marialunch00001", "Event", "Customer lunch", "2026-11-03T12:00:00", "2026-11-03T13:30:00"),
                E("mariaworkshop01", "Meeting", "Pricing workshop", "2026-11-05T10:00:00", "2026-11-05T12:00:00"),
            ]),
            Personal("samcalendar00001", "sam.patel", "sam.patel", "#f08c00", [
                E("samdemo00000001", "Meeting", "Product demo", "2026-11-04T10:00:00", "2026-11-04T11:00:00"),
                E("samooo000000001", "OutOfOffice", "Out of office", "2026-11-05T00:00:00", "2026-11-06T00:00:00", allDay: true),
                E("samfollowups001", "Focus", "Follow-up calls", "2026-11-03T14:00:00", "2026-11-03T16:00:00"),
            ]),
            Personal("alexcalendar0001", "alex.kim", "alex.kim", "#0c8599", [
                E("alexrota0000001", "Meeting", "Support rota", "2026-11-03T09:00:00", "2026-11-03T12:00:00"),
                E("alexescalation1", "Meeting", "Escalation review", "2026-11-04T14:00:00", "2026-11-04T15:00:00"),
            ]),
            Personal("priyacalendar001", "priya.shah", "priya.shah", "#7048e8", [
                E("priyatraining01", "Meeting", "Product training", "2026-11-04T13:00:00", "2026-11-04T15:00:00"),
                E("priyacustomer01", "Meeting", "Customer call", "2026-11-03T10:00:00", "2026-11-03T11:00:00"),
            ]),
            new
            {
                itemId = "salesteam0000001", name = "Sales Team", type = "Team", color = "#0c8599", timeZoneId = Tz, ownerKind = "Role", ownerId = "Sales Team", visibility = "Private",
                Events = new object[]
                {
                    E("salesstandup001", "Meeting", "Sales stand-up", "2026-10-05T09:30:00", "2026-10-05T09:45:00", recurrence: "FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR"),
                    E("salesreview0001", "Meeting", "Pipeline review", "2026-11-02T14:00:00", "2026-11-02T15:00:00", who: "maria.lopez", attendees: ["maria.lopez", "sam.patel", "jamie.chen"]),
                    E("salesmilestone1", "Milestone", "Quarter-end target", "2026-11-13T00:00:00", "2026-11-14T00:00:00", allDay: true),
                },
            },
            new
            {
                itemId = "supportteam00001", name = "Support Team", type = "Team", color = "#e8590c", timeZoneId = Tz, ownerKind = "Role", ownerId = "Support Team", visibility = "Private",
                Events = new object[]
                {
                    E("supportoncall01", "OnCall", "On call", "2026-11-02T08:00:00", "2026-11-02T16:00:00", who: "alex.kim", attendees: ["alex.kim"]),
                    E("supportqueue001", "Meeting", "Queue review", "2026-11-04T11:00:00", "2026-11-04T12:00:00", who: "priya.shah", attendees: ["priya.shah", "alex.kim"]),
                },
            },
        };

        var recipe = JsonSerializer.Serialize(new
        {
            steps = new object[]
            {
                new
                {
                    name = "Calendars",
                    Categories = new object[]
                    {
                        new { itemId = "holidays0000001", name = "Company holiday", color = "#e03131", ownerId = (string)null },
                        new { itemId = "training000001", name = "Training", color = "#7048e8", ownerId = (string)null },
                    },
                    Calendars = calendars,
                },
            },
        }, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });

        await page.GotoAsync(baseUrl + "/Admin/DeploymentPlan/Import/Json");
        var result = await page.EvaluateAsync<string>("async (json) => { const f = document.getElementById('Json_Form') ?? document.querySelector('form[action*=Json]'); const data = new FormData(f); data.set('Json', json); const r = await fetch(f.action, { method: 'POST', body: data }); return r.status + ' ' + r.url; }", recipe);
        Console.WriteLine("Seed: " + result);
    }
}
