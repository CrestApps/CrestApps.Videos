# Bookings

**Feature:** `CrestApps.OrchardCore.Bookings` · **Length:** see `video.json`

Why a date is part of what you sell, the three kinds of booking (a date with seats, nights, a time), making a product a
booking, managing a tour's dates, nights and times, deposits, cancellation policies, requests and travelers' details, what
buyers see and the **Find a date** page, the bookings list and calendar, and managing a booking. The video shows the admin and
the storefront only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider: the buyers pay by **bank transfer**, which staff record as received.

1. Make the site the Point of Sale video describes (its steps 1 to 4: the **Online store** recipe and the sample store), and the
   fake customers the Customers and Companies video makes.
2. Sign the recorder in, then prepare the site in three passes:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/bookings/recorder
   export VIDEO_ROOT=<repo>/videos/bookings VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare                                  # the Bookings sample recipe, bank transfer at checkout, a sign-in for jamie and dana
   VIDEO_PREPARE_STAGE=book <bin>/Recorder prepare         # three bookings: the tour (Jamie Chen), the massage (Dana Wu) and the room (a guest)
   VIDEO_PREPARE_STAGE=receive <bin>/Recorder prepare      # records each bank transfer as received, so the bookings are confirmed
   ```
3. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/bookings`) and record the clips. `04-dates` adds a
   departure to the tour, and `07-buyer` adds the tour to the cart, so restore the copy before recording `07-buyer` again.
