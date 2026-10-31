# Scheduling: Book, Confirm, Move and Cancel Appointments

**YouTube:** not published yet · **Feature:** `CrestApps.OrchardCore.Scheduling`

The statuses of an appointment, booking an appointment for a customer, the appointment's page (details, the customer's manage link, confirm, took place, no-show, move to another time, cancel with a reason), the appointments list (search by customer and status filter), and how appointments appear on the calendar. Each task chapter is also assembled as a short video (`"short"` in the storyboard).

The title, description, tags and chapters YouTube shows are in `video.json`; `scheduling-book-and-manage.vtt` and `scheduling-book-and-manage.srt` are its
captions.

## Recording it again

It was recorded on the Contoso Clinic demo site after the first two Scheduling videos (all resources and appointment types, one booked consultation for Jordan Blake and a blocked hour on Dr. Maya Chen's calendar), behind a dead proxy. The clips book an ultrasound for Riley Brooks, confirm it, move it and cancel it, so restore the starting site before a new take.

Record the clips in one take from that starting state (`Recorder record all`), after `narrate.py`. Fake people only
(Contoso Clinic, `example.com` emails, 555 phone numbers), and a dead outbound proxy so nothing is ever sent.
