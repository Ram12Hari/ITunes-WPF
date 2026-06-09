# Theme Switching Status (Temporary Freeze)

## Summary
As of 2026-04-07, the app is intentionally stabilized to **Light theme only**.

Theme switching (Light/Dark at runtime) was implemented experimentally but remains unreliable in this codebase and can trigger runtime brush/resource edge cases in WPF.

## Current Behavior
- Default theme: `Light`
- Settings page shows theme section, but switching controls are disabled
- Application remains stable and usable with the Light theme

## Decision
We are deferring runtime theme switching until a dedicated revisit.

## Why Deferred
- Runtime resource replacement/mutation in WPF can cause frozen/read-only brush exceptions depending on how resources are materialized.
- The app now prioritizes functional stability over unfinished theme toggling behavior.

## Revisit Plan
When revisiting, evaluate one stable approach end-to-end:
1. Fully `DynamicResource`-driven styling for all theme-dependent values
2. Centralized theme dictionary swap with strict resource key parity checks
3. Add startup + runtime smoke tests for navigation/views after theme swap
4. Validate dialogs/context menus/templates specifically (common WPF edge zones)

## Notes for Next Session
- `IThemeService` currently exposes only `Light` in `AvailableThemes`
- Settings controls are disabled by design for now
- Do not re-enable switching until the above revisit plan is completed
