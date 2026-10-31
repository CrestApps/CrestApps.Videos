# Scheduling: Online Booking and the Waitlist

**YouTube:** not published yet · **Feature:** `CrestApps.OrchardCore.Scheduling`

Why online booking, turning it on (the Scheduling Self-Service feature, Customers can book it themselves and the Booking page address), what the customer sees on the public booking page and the Your appointment page (book, move, cancel), types that wait for confirmation, the waitlist (add, waiting, notified when a time frees up, remove), and telling the customer by email with a workflow. Each task chapter is also assembled as a short video (`"short"` in the storyboard).

The title, description, tags and chapters YouTube shows are in `video.json`; `scheduling-online-booking-and-waitlist.vtt` and `scheduling-online-booking-and-waitlist.srt` are its
captions.

## Recording it again

It was recorded on the Contoso Clinic demo site after the first three Scheduling videos, with the Blog theme as the public theme (so the public pages are styled), behind a dead proxy. The clips book a physiotherapy session for Casey Morgan from the public page, move it, request an ultrasound scan, add Taylor Reed to the waitlist and cancel Casey's appointment, so restore the starting site before a new take. The public clips are recorded without signing in.

Record the clips in one take from that starting state (`Recorder record all`), after `narrate.py`. Fake people only
(Contoso Clinic, `example.com` emails, 555 phone numbers), and a dead outbound proxy so nothing is ever sent.
