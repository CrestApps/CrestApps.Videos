"""Sets up two states a tidy demo site never reaches by itself (run it while the site is stopped).

Usage: python make-states.py <App_Data folder>

- Marks the child tenant "Failing Co" of the firm as failed, as if its setup had not finished.
- Removes the folder of the parent "lakeside", so its child tenant is left without a parent (an orphan).
- Makes the child tenant "Summit Realty" an ordinary tenant behind the firm's back, as a platform administrator can, so the firm sees it as changed by the platform.
"""
import json
import shutil
import sqlite3
import sys
from pathlib import Path

app_data = Path(sys.argv[1])
connection = sqlite3.connect(app_data / "Sites" / "firm" / "OrchardCore.db")
cursor = connection.cursor()

for document_id, content in cursor.execute('select Id, Content from "TenantHierarchy_Document" where Content like \'%"Slug":"failing-co"%\'').fetchall():
    cursor.execute('update "TenantHierarchy_Document" set Content = ? where Id = ?', (content.replace('"Status":"Ready"', '"Status":"Failed"'), document_id))
    cursor.execute('update "TenantHierarchy_ChildTenantEntryIndex" set Status = ? where DocumentId = ?', ("Failed", document_id))

connection.commit()
connection.close()
shutil.rmtree(app_data / "Sites" / "lakeside", ignore_errors=True)
settings_path = app_data / "Sites" / "u_1proe7uh5s" / "appsettings.json"
settings = json.loads(settings_path.read_text(encoding="utf-8"))
settings.pop("TenantHierarchy", None)
settings["Features"] = [feature for feature in settings.get("Features", []) if "TenantHierarchy" not in feature]
settings_path.write_text(json.dumps(settings, indent=2), encoding="utf-8")
print("Marked Failing Co as failed and removed the lakeside parent, and made Summit Realty an ordinary tenant")
