# Scheduling: Resources and Appointment Types for Bookable Time

**YouTube:** not published yet · **Feature:** `CrestApps.OrchardCore.Scheduling`

Why Scheduling exists, how to add the people, rooms and equipment that take appointments (staff member, user, calendar, time zone, working hours and days off), how to describe an appointment type (length, free time before and after, resources, which one takes it, places per appointment, booking rules, the public booking page address), and where appointments appear. Each task chapter is also assembled as a short video (`"short"` in the storyboard).

The title, description, tags and chapters YouTube shows are in `video.json`; `scheduling-people-rooms-appointment-types.vtt` and `scheduling-people-rooms-appointment-types.srt` are its
captions.

## Recording it again

It was recorded on a fresh Contoso Clinic site (Blank recipe) with the Calendars, Scheduling, Scheduling Self-Service and Workflows features on and three fake users (`maya.chen`, `omar.hassan`, `lena.ortiz`), behind a dead proxy. Clips create the first two resources and the first appointment type on camera, so start from a site without any.

Record the clips in one take from that starting state (`Recorder record all`), after `narrate.py`. Fake people only
(Contoso Clinic, `example.com` emails, 555 phone numbers), and a dead outbound proxy so nothing is ever sent.
