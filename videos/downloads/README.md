# Digital Downloads

**Feature:** `CrestApps.OrchardCore.Downloads` · **Length:** see `video.json`

Why a file you sell must be kept private, how upload, payment and download fit together, how to set up a digital product (its
files and limits), the site-wide limits, what buyers see (their account, their order and the download page), and how to take a
link back, give downloads back and copy a buyer's link. The video shows the admin and the storefront only: no code or
configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider: the two paid orders are register sales paid in cash.

1. Make the site the Point of Sale video describes (its steps 1 to 4: the **Online store** recipe, the sample store, the registers and
   the till-app sign-in), and the fake customers the Customers and Companies video makes.
2. Sign the recorder in and prepare the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/downloads/recorder
   export VIDEO_ROOT=<repo>/videos/downloads VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the Customers and Orders APIs, runs the **Digital downloads sample** recipe (two sample ebooks with their PDF
   files), and saves a sign-in for the fake buyer Jamie Chen (`state-jamie.json`, git-ignored).
3. Run `python videos/downloads/seed.py`: it sells the two ebooks to Jamie Chen, and one to Riley Park, at a register, paid in cash,
   so there are orders with downloads. It needs `pip install requests`.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/downloads`) and record the clips. `06-manage` takes
   back a link and gives it back, so the order ends as it began.
