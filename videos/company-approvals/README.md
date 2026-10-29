# Company Approvals and Pay on Account

**Feature:** `CrestApps.OrchardCore.Companies` · **Length:** see `video.json`

Why companies buy differently, setting a company's terms (order limit, deposit, net days, credit limit, monthly budget, purchase order), the buyer's checkout and asking for approval, the company admin's approval, paying a deposit and putting the rest on account, what the company owes and recording its payment, quotes with a purchase order, and punchout. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Make the Point of Sale video's store, and the Customers and Companies video's fake buyers (riley, an admin, and morgan, a buyer, of Northwind Traders).
2. Sign the recorder in, build it with `-p:VideoScenes=<repo>/videos/company-approvals/recorder`, and run `Recorder setup`, then `Recorder prepare` (turns on SMTP and bank transfer, takes quote requests, signs riley and morgan in).
3. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/company-approvals`) and record the clips in order: `03-terms`, `04-buyer`, `05-approve`, `06-pay`, `07-owed`, `08-quote`. Each one builds on the one before, so restore the copy before recording them again.
