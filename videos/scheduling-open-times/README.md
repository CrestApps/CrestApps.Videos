# Scheduling: How Open Times Are Worked Out

**YouTube:** not published yet · **Feature:** `CrestApps.OrchardCore.Scheduling`

How open times are worked out: working hours and days off, busy time on a resource's calendar, the free time before and after appointments, the free time that already booked appointments keep clear, the booking limits (minimum notice, how far ahead, most per day, places in a class), and what to check when no times appear. Each task chapter is also assembled as a short video (`"short"` in the storyboard).

The title, description, tags and chapters YouTube shows are in `video.json`; `scheduling-open-times.vtt` and `scheduling-open-times.srt` are its
captions.

## Recording it again

It was recorded on the Contoso Clinic demo site with the resources and appointment types of the first Scheduling video, plus Omar Hassan, Lena Ortiz, a yoga studio, an ultrasound machine and four more appointment types, behind a dead proxy. The clips book one appointment (Jordan Blake) and block one hour on Dr. Maya Chen's calendar, so restore the starting site before a new take.

Record the clips in one take from that starting state (`Recorder record all`), after `narrate.py`. Fake people only
(Contoso Clinic, `example.com` emails, 555 phone numbers), and a dead outbound proxy so nothing is ever sent.
