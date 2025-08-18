# PulsNet Style Editing Guide

## Where to Edit
- Global styles: `src/PulsNet.Web/wwwroot/css/site.css`
- Dashboard interactions: `src/PulsNet.Web/wwwroot/js/dashboard.js`
- Layout and navigation: `src/PulsNet.Web/Views/Shared/_Layout.cshtml`

## Colors and Theme
- Background, text, and component colors are defined near the top-level selectors in `site.css`.
- Adjust gradients and status dot colors under `.link-usage .fill` and `.status .dot`.

## Micro-animations
- Card hover: `.device-card:hover` transitions.
- Link usage bar: transition on width and background.
- IP reveal blur: `.ip` vs `.ip.revealed`.

## Responsiveness
- Grid layout is responsive via `grid-template-columns: repeat(auto-fill, minmax(300px, 1fr))`.
- Tweak the `minmax` value to fit more/fewer cards per row.

## Admin Pages
- Minimal styles; extend tables and forms as needed by adding classes and corresponding CSS.

## Optional PWA
- Add a `manifest.json` under `wwwroot/` and a service worker. Link the manifest in `_Layout.cshtml`.