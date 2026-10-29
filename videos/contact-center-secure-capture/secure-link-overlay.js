// VIDEO_INIT_SCRIPT for the Secure data capture video.
//
// "Collect data securely" in the agent workspace shows the one-time customer link with window.prompt(). A native
// browser dialog is not part of the page, so a headless recording (and a screenshot) never contains it: Playwright
// closes it on its own and the film shows nothing happening. This script replaces ONLY that prompt with an in-page
// dialog that looks like one (the message, the link in a selectable box, an OK button), so it can be filmed.
//
//   $env:VIDEO_INIT_SCRIPT = "<path to this file>"
//
// The link is also stored in window.__secureCaptureLink, so the recorder can read it and open the customer page:
//   var link = await Page.EvaluateAsync<string>("() => window.__secureCaptureLink");
// Every other prompt (for example the "reason for pausing" one) is left alone.
(function () {
    var nativePrompt = window.prompt;

    window.prompt = function (message, value) {
        if (!/secure link/i.test(String(message || ''))) {
            return nativePrompt.apply(window, arguments);
        }

        window.__secureCaptureLink = value;

        var existing = document.getElementById('demo-secure-link-dialog');

        if (existing) {
            existing.remove();
        }

        var backdrop = document.createElement('div');
        backdrop.id = 'demo-secure-link-dialog';
        backdrop.setAttribute('role', 'dialog');
        backdrop.setAttribute('aria-modal', 'true');
        backdrop.style.cssText = 'position:fixed;inset:0;z-index:2147483000;background:rgba(0,0,0,.35);display:flex;align-items:flex-start;justify-content:center;padding-top:72px;';

        var card = document.createElement('div');
        card.style.cssText = 'background:#fff;color:#1b1b1f;border-radius:10px;box-shadow:0 12px 40px rgba(0,0,0,.35);width:min(640px,92vw);padding:22px 24px;font:16px/1.4 system-ui,-apple-system,"Segoe UI",Roboto,sans-serif;';

        var title = document.createElement('div');
        title.textContent = String(message);
        title.style.cssText = 'font-weight:600;margin-bottom:12px;';

        var box = document.createElement('input');
        box.type = 'text';
        box.readOnly = true;
        box.value = value || '';
        box.style.cssText = 'width:100%;box-sizing:border-box;padding:9px 10px;border:1px solid #8a8a93;border-radius:6px;font:14px ui-monospace,Consolas,monospace;';

        var actions = document.createElement('div');
        actions.style.cssText = 'display:flex;justify-content:flex-end;margin-top:16px;';

        var ok = document.createElement('button');
        ok.type = 'button';
        ok.textContent = 'OK';
        ok.setAttribute('data-demo-secure-link-ok', '');
        ok.style.cssText = 'background:#0a5cd6;color:#fff;border:0;border-radius:6px;padding:8px 26px;font-size:15px;cursor:pointer;';
        ok.addEventListener('click', function () { backdrop.remove(); });

        actions.appendChild(ok);
        card.appendChild(title);
        card.appendChild(box);
        card.appendChild(actions);
        backdrop.appendChild(card);
        (document.body || document.documentElement).appendChild(backdrop);
        box.focus();
        box.select();
        box.scrollLeft = 0;

        // A native prompt blocks until it is answered; this one does not. The workspace ignores the return value.
        return null;
    };
})();
