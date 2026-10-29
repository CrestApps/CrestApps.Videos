"""Turns one loaded activity of the demo site into a finished AI voice call with a short, fake transcript (fake data only).

usage: seed-review.py <App_Data dir>   (the site must be stopped)
"""
import json
import sqlite3
import sys

db = sqlite3.connect(sys.argv[1] + "/Sites/Default/OrchardCore.db")

profile = db.execute("select Id, Content from AI_Document where Type like '%AIProfile,%' order by Id limit 1").fetchone()
profile_doc = json.loads(profile[1])
profile_id = profile_doc["ItemId"]
activity = db.execute("select Id, Content from Omnichannel_Document where Type like '%OmnichannelActivity,%' order by Id asc limit 1").fetchone()
doc = json.loads(activity[1])
records = json.loads(db.execute("select Content from Document where Type like '%OmnichannelDisposition%'").fetchone()[0])["Records"]
disposition = next(r for r in records.values() if r["Name"] == "Call Back Later")

session_id = "4s7yb3n2vcckhx1b48z8t9w5aa"
now = "2026-10-29T15:40:00Z"
doc.update({
    "Status": "Completed", "InteractionType": "Automated", "Source": "Automatic", "AIProfileId": profile_id,
    "AISessionId": session_id, "DispositionId": disposition["ItemId"], "DispositionedBy": "AIAgent",
    "DispositionedByAIProfileId": profile_id, "CompletedUtc": now, "Attempts": 1,
    "Notes": "Casey is interested in a spring test drive and asked to be called back on Thursday afternoon.",
})
db.execute("update Omnichannel_Document set Content=? where Id=?", (json.dumps(doc, separators=(",", ":")), activity[0]))
db.execute("update Omnichannel_OmnichannelActivityIndex set Status=3, InteractionType=1, Source='Automatic', AIProfileName=?, DispositionId=?, CompletedUtc=?, Attempts=1 where DocumentId=?",
           (profile_doc["Name"], disposition["ItemId"], "2026-10-29 15:40:00", activity[0]))

session = {"SessionId": session_id, "ProfileId": profile_id, "Title": "Spring lead call", "UserId": "", "Documents": [], "CreatedUtc": "2026-10-29T15:36:00Z",
           "LastActivityUtc": now, "ClosedAtUtc": now, "Status": "Closed", "ExtractedData": {}, "PostSessionResults": {}, "PostSessionProcessingStatus": "None",
           "PostSessionProcessingAttempts": 0, "IsPostSessionTasksProcessed": True, "IsAnalyticsRecorded": False, "IsConversionGoalsEvaluated": False, "Properties": {}}
db.execute("insert into AI_Document (Id, Type, Content, Version) values (?,?,?,1)",
           (50001, "CrestApps.Core.AI.Models.AIChatSession, CrestApps.Core.AI.Abstractions", json.dumps(session, separators=(",", ":"))))
db.execute("insert into AI_AIChatSessionIndex (DocumentId, SessionId, ProfileId, UserId, Status, LastActivityUtc) values (50001,?,?,?,?,?)",
           (session_id, profile_id, "", 1, "2026-10-29 15:40:00"))
db.execute("insert into Omnichannel_OminchannelActivityAIChatSessionIndex (DocumentId, SessionId, ActivityId) values (?,?,?)", (activity[0], session_id, doc["ItemId"]))

lines = [
    ("assistant", "Hi, this is Contoso calling about your recent inquiry. Do you have a quick minute?"),
    ("user", "Sure, go ahead."),
    ("assistant", "Great. You asked about a spring test drive. Which model are you interested in, and when would suit you?"),
    ("user", "The compact SUV. Thursday afternoon would work, but can you call me back then?"),
    ("assistant", "Of course. I will note Thursday afternoon, and someone will call you back. Thank you, and have a good day."),
]
for index, (role, text) in enumerate(lines):
    prompt = {"SessionId": session_id, "Role": role, "Content": text, "IsGeneratedPrompt": False, "CreatedUtc": f"2026-10-29T15:{36 + index}:00Z",
              "ItemId": f"4qfzhyj7j3xb142r9bhrmtvj{index:02d}", "Properties": {}}
    doc_id = 50010 + index
    db.execute("insert into AI_Document (Id, Type, Content, Version) values (?,?,?,1)",
               (doc_id, "CrestApps.Core.AI.Models.AIChatSessionPrompt, CrestApps.Core.AI.Abstractions", json.dumps(prompt, separators=(",", ":"))))
    db.execute("insert into AI_AIChatSessionPromptIndex (DocumentId, ItemId, SessionId, Role) values (?,?,?,?)", (doc_id, prompt["ItemId"], session_id, role))
db.commit()
print("seeded activity", doc["ItemId"], "profile", profile_id)
