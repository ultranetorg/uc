# Assets naming

`<name>[-<size>].<ext>` — kebab-case. The default size `md` (24px) has **no postfix**; every other size does.

| postfix   | px     | postfix | px    |
|-----------|--------|---------|-------|
| `xxs`     | 14     | `xl`    | 32    |
| `xs`      | 16     | `2xl`   | 40–44 |
| `sm`      | 20     | `3xl`   | 56–64 |
| *(none)*  | 24     | `4xl`   | 72–80 |
| `lg`      | 28     | `5xl`   | 90    |
|           |        | `6xl`   | 120   |
|           |        | `7xl`   | 512   |

- `chevron-down.svg` is the 24px icon, `chevron-down-sm.svg` is the 20px one. Do not add `-md`.
- Size is the nominal grid of the asset (height for icons, width for wide logos), not the size it is rendered at.
- Base names follow Bootstrap Icons (`chevron-down`, `box-arrow-up-right`, `file-earmark-code`); no digits glued to a word (`person-2` → `person`, `chevron-down2` → `chevron-down-alt`).
- Order: `<subject>[-<part/direction>][-<shape>][-<fill>][-colored][-<size>]`, e.g. `check-circle-fill-colored-4xl`.
- Raster fallbacks live in `fallback/` as `<entity>-avatar-<size>.png`.
- SVG exports are `Svg` + PascalCase of the file name: `chevron-down.svg` → `SvgChevronDown`, `chevron-down-sm.svg` → `SvgChevronDownSm`. Each folder's `index.ts` re-exports its SVGs.
