### PulsNet – Style Editing Guide

#### Colors and Theme
- Base theme lives in `config/secure_config.json > Theme`.
- Admin UI allows adjusting theme at runtime; values persist in DB `settings` table.
- Frontend uses CSS variables in `wwwroot/styles.css`.

To change defaults:
1. Edit `secure_config.json` Theme colors.
2. Restart app or trigger theme save in Admin UI.

#### Animations and Micro-interactions
- Button hover/active transitions: `.btn:hover` and `.btn:active` in `styles.css`.
- Card hover lift: `.card.hover:hover`.
- Link usage bar transitions: `.bar .bar-fill` width transition.

#### Layout
- Responsive grid via `.grid`, `.grid.two` in `styles.css`.
- Adjust min card width in `.grid` definition.

#### Add new components
- Create a new HTML template or card in `wwwroot/*.html`.
- Style with existing tokens and utility classes (`row`, `space`, `muted`).

### PulsNet – Style Editing Guide

#### Colors and Theme
- Base theme lives in `config/secure_config.json > Theme`.
- Admin UI allows adjusting theme at runtime; values persist in DB `settings` table.
- Frontend uses CSS variables in `wwwroot/styles.css`.

To change defaults:
1. Edit `secure_config.json` Theme colors.
2. Restart app or trigger theme save in Admin UI.

#### Animations and Micro-interactions
- Button hover/active transitions: `.btn:hover` and `.btn:active` in `styles.css`.
- Card hover lift: `.card.hover:hover`.
- Link usage bar transitions: `.bar .bar-fill` width transition.

#### Layout
- Responsive grid via `.grid`, `.grid.two` in `styles.css`.
- Adjust min card width in `.grid` definition.

#### Add new components
- Create a new HTML template or card in `wwwroot/*.html`.
- Style with existing tokens and utility classes (`row`, `space`, `muted`).

