# Customers and Companies

**Feature:** `CrestApps.OrchardCore.Customers` (with Store Credit, Loyalty Points, Referrals and Checkout details), `CrestApps.OrchardCore.Addresses` (address suggestions) and `CrestApps.OrchardCore.Companies` · **Length:** see `video.json`

Why a store needs more than a list of emails, how customers, accounts and companies fit together, and how to find, add
and edit a customer, keep an address book, give store credit and loyalty points, set up checkout, privacy, address
suggestions, rewards and referrals, answer a privacy request, what a signed-in customer sees in **My Account**, create a
price list, add a company with buyers, payment terms, locations and a purchase order number, how approvals and budgets
work, and punchout. The video shows the admin and the storefront only: no code or configuration files. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, `555` phone numbers, made-up
company names), so it needs no mask script. Nothing reaches a payment or messaging provider.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5375`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9` so nothing reaches a provider.
2. Set the site up with the **Online store** recipe, site name **Contoso**, SQLite, user `admin` and password `Password1!`.
3. Delete the **Shop** product listing that the setup recipe made (**Content > Product Listings**), then run the
   **Product listing: online store sample** recipe under **Tools > Recipes**. It adds the sample store with two sample customers.
   (Both recipes make a listing at `/shop`, so the second one fails while the first one's listing exists.)
4. Sign the recorder in, then let it enable the features, make the fake buyers, customers and a first company:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/customers-companies/recorder
   export VIDEO_ROOT=<repo>/videos/customers-companies VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` also saves a sign-in for the buyer Jamie Chen (`state-jamie.json`, git-ignored) that the **My Account** clip uses.
5. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/customers-companies`) and record the clips in
   order. Clips change the site: `06-credit` gives Jamie Chen the credit and points that `09-account` shows, `10-pricelist`
   creates the **Wholesale** list that `11-company` selects, and `08-privacy` erases a customer, so restore the copy
   before recording a clip again.
