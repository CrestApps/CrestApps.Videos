# Fundraising

**Feature:** `CrestApps.OrchardCore.Fundraising` · **Length:** see `video.json`

Why a donation campaign differs from a shop, how receipts, a campaign and donors fit together, how to set up your
receipts, start a campaign (goal, suggested amounts, least donation, end date, tax receipts), how donors give and name
someone to honor, how donations appear as orders, and what a donor sees under **Donations** in their account. The video shows
the admin and the storefront only: no code or configuration files. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names and organization), so
it needs no mask script. Nothing reaches a payment or messaging provider: the donors give by **bank transfer**, and the
transfers are recorded as received by staff.

1. Make the site the Point of Sale video describes (its steps 1 to 4: the **Online store** recipe and the sample store), and the
   fake customers the Customers and Companies video makes.
2. Sign the recorder in, then prepare the site in three passes:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/fundraising/recorder
   export VIDEO_ROOT=<repo>/videos/fundraising VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare                                  # the Fundraising sample recipe, bank transfer at checkout, a sign-in for each donor
   VIDEO_PREPARE_STAGE=donate <bin>/Recorder prepare       # four donations: Jamie Chen and Dana Wu (signed in), and two guests
   VIDEO_PREPARE_STAGE=receive <bin>/Recorder prepare      # records each bank transfer as received, so the gifts count toward the goal
   ```

   Riley Park and Morgan Lee buy for a company (see the Customers and Companies video), so they are not donors here.
3. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/fundraising`) and record the clips in order.
   `03-receipts` saves the organization, tax ID and statement that `07-account` shows on the receipt.
