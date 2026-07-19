# CSS Sizing Summary: px vs. rem

## The Core Problem with px
* **Overrides user settings**: Absolute `px` units lock font sizes in place.
* **Breaks accessibility**: Visually impaired users cannot scale text using browser zoom preferences.

## Why to Use rem
* **Respects user preferences**: Scales dynamically relative to the root `<html>` font size.
* **Maintains accessibility**: Adapts fluidly when a user changes their default browser font size.

## Best Practices: When to Use What

### Use rem for:
* **Typography**: All `font-size` properties.
* **Layout spacing**: `margin` and `padding` values.
* **Component dimensions**: Layout element `width` and `height`.

### Use px for:
* **Thin borders**: Cosmetic elements like `1px` or `2px` borders.
* **Fixed exceptions**: Small decorative elements that must never change size.

### Use em for:
* **Contextual scaling**: Component padding (like buttons) that must scale proportionally with the element's own text size.
