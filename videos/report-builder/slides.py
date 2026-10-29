"""The slides of the Report Builder video."""
from brand import MARGIN, W, bullets, draw_code, note_cards, slide_base

WHY = [
    ("1", "Data lives in many places", "Content items, users, calls, activities, AI chats and messages"),
    ("2", "Built-in reports are fixed", "They answer the questions someone thought of in advance"),
    ("3", "Open to any data", "More sources keep coming, and developers can plug in their own"),
    ("check", "Build your own reports", "Pick data sets, join, group, total, chart, and share"),
]

CONCEPTS = [
    ("1", "Data sources and data sets", "Contents, Users, Contact Center... each with data sets and fields"),
    ("2", "Reports", "Data sets, columns and filters, shown as tables, charts, metrics, pivots"),
    ("3", "Views", "Saved, reusable data sets that can refresh on a schedule"),
    ("check", "Permissions everywhere", "Sources show what you may read; reports run with the owner's access"),
]

JOIN_IDEA = [
    ("1", "Rows side by side", "Each activity sits next to the user it is assigned to"),
    ("2", "Matched on columns", "The activity's Assigned to column equals the user's ID"),
    ("check", "Fields of both", "Columns, filters and charts use the fields of either data set"),
]

JOIN_KINDS = [
    ("1", "Inner join", "Only rows that match on both sides: the activities that have a user"),
    ("2", "Left join", "All rows before: every activity, with empty user columns when none matches"),
    ("3", "Right join", "All rows of this data set: every user, even one without activities"),
    ("4", "Full join", "All rows of both sides: everything from both data sets"),
]

SOURCE = """
public sealed class HelpDeskDataSource : IDataSource
{
    public static string Name => "HelpDesk";

    public static LocalizationSource DisplayName { get; } =
        LocalizationSource.Create<HelpDeskDataSource>("Help desk"); // and Description

    public async Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(
        DataSourceContext context, CancellationToken cancellationToken = default)
        => await CanReadAsync(context) ? [Describe()] : [];

    public async Task<DataSetSchema> GetSchemaAsync(
        string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        // The fields of the data set: their names, labels and types.
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(
        DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // The rows, a batch at a time, with only the fields the report asks for.
    }
}
"""

STARTUP = """
[RequireFeatures("OrchardCore.DataPipelines.Core")]
public sealed class DataSourcesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddScoped<InvoicesReportDataSet>()
            .AddDataSource<BillingReportDataSource>();
    }
}
"""

TYPES = ("HelpDeskDataSource", "IDataSource", "LocalizationSource", "DataSetDescriptor", "DataSourceContext",
         "DataSetSchema", "DataBatch", "DataSourceQuery", "CancellationToken", "Task", "IReadOnlyList", "IAsyncEnumerable",
         "EnumeratorCancellation", "DataSourcesStartup", "StartupBase", "IServiceCollection", "RequireFeatures",
         "InvoicesReportDataSet", "BillingReportDataSource")


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Answers without writing a query")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("concepts-"):
        image = slide_base(chapter_title, label, "Sources, reports and views")
        bullets(image, CONCEPTS, active={int(step_id[-1]) - 1})
        return image
    if step_id == "join-1":
        image = slide_base(chapter_title, label, "Rows side by side, matched on columns")
        bullets(image, JOIN_IDEA)
        return image
    if step_id in ("join-2", "join-3"):
        image = slide_base(chapter_title, label, "The join type: which rows it keeps")
        bullets(image, JOIN_KINDS, active={0, 1} if step_id == "join-2" else {2, 3})
        return image
    if step_id == "ext-1":
        image = slide_base(chapter_title, label, "1. Implement IDataSource")
        draw_code(image, (MARGIN, 225, W - MARGIN, 1060), "HelpDeskDataSource.cs", "csharp", SOURCE, types=TYPES, size=22)
        return image
    if step_id == "ext-2":
        image = slide_base(chapter_title, label, "2. Register it")
        draw_code(image, (MARGIN, 240, 1150, 720), "Startup.cs", "csharp", STARTUP, types=TYPES, size=24)
        note_cards(image, [
            ("Records", "Derive from RecordDataSource"),
            ("Data sets", "One RecordDataSet each"),
            ("Security", "Read only what each may see"),
        ], (1190, 240, W - MARGIN, 0))
        return image
    raise SystemExit(f"No slide for {step_id}")
