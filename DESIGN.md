# Design system

## Direction

The selected direction is a category-standard admin interface executed cleanly and without decorative metaphors. It uses a white navigation shell, neutral gray application ground, compact cards, familiar controls, and a blue primary action. The purpose is immediate recognition and low training cost.

## Tokens

| Token | Value | Use |
| --- | --- | --- |
| `--ink` | `#111827` | primary text |
| `--ink-soft` | `#667085` | secondary text |
| `--ground` | `#f5f7fa` | application ground |
| `--paper` | `#ffffff` | message and editor surfaces |
| `--line` | `#e4e7ec` | quiet separators and card borders |
| `--accent` | `#2563eb` | primary actions and selected navigation |
| `--ok` | `#147a50` | accepted state |
| `--error` | `#b42318` | failed state |
| `--focus` | `#2563eb` | keyboard focus |

## Typography

- Interface and headings: the platform UI stack, led by Segoe UI on Windows and San Francisco on Apple platforms.
- Message content uses the same family so the admin surface remains deliberately conventional and compact.
- Provider IDs: the platform monospace because these values are machine identifiers.

## Layout

- Desktop uses a persistent 224px white rail and a two-column card-based composer.
- Mobile converts the rail to a compact top bar and stacks message details before preview.
- History keeps the three essential fields on narrow screens: sent time, customer, and status.
- Spacing uses tight groups inside a task and generous separation between tasks.

## Components and states

- Standard labels, selects, inputs, buttons, tables, and textareas; no custom control metaphors.
- Blue primary buttons are reserved for sending or saving.
- Message details and preview are peer cards; the resolved SMS sits on a quiet gray inset surface.
- Loading text describes the active operation. Errors name both the failure and the recovery where possible.
- Disabled send state is visibly neutral and retains readable text contrast.
- Focus rings are 3px signal blue with a 3px offset.

## Responsive and accessibility

- Keyboard navigation and a skip link are required.
- Text and controls meet WCAG AA contrast.
- Navigation icon labels remain available to assistive technology when mobile hides their visible text.
- The send action requires confirmation because it creates an external message and cost.

## Voice

Direct, calm, and operational. Controls use verbs such as “Send message” and “Save template.” Avoid marketing language, invented claims, and provider jargon unless it helps diagnose a send.
