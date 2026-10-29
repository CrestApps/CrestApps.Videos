# Invoices and Receipts

**Feature:** `CrestApps.OrchardCore.Transactions` · **Length:** see `video.json`

Why invoices and receipts, branding them, sending an invoice and printing it or copying its pay link, the customer paying from the link, recording a payment and the numbered receipt, and refund notices, the test payment badge and the Base url. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site (its steps 1 and 2) and run `Recorder setup`, then `Recorder prepare` (turns on Receipts and Send Payment Receipts, and gives Dana a bank-transfer order that is still waiting for its money: an outstanding transaction).
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/invoices-receipts`) and record the clips in order: `03-brand`, `04-invoice`, `05-pay`, `06-receipt`. `04-invoice` sends the invoice and saves its pay link to `pay-link.txt`, `05-pay` opens that link, and `06-receipt` records the payment, so restore the copy before recording them again.
