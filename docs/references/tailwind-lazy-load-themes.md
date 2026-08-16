# Lazy Loading Themes in Tailwind CSS

To lazy load themes, map Tailwind utility classes to CSS variables and dynamically inject separate theme stylesheets at runtime.

## 1. Configuration (`tailwind.config.js`)
Map theme tokens directly to CSS variables.

```javascript
module.exports = {
  theme: {
    extend: {
      colors: {
        primary: 'var(--color-primary)',
        secondary: 'var(--color-secondary)',
        bgMain: 'var(--color-bg-main)',
      },
    },
  },
}
```

## 2. Separate Theme Stylesheets
Create lightweight CSS files holding only the variable overrides.

```css
/* theme-light.css */
:root {
  --color-primary: #3b82f6;
  --color-secondary: #1e40af;
  --color-bg-main: #ffffff;
}

/* theme-dark.css */
:root {
  --color-primary: #10b981;
  --color-secondary: #065f46;
  --color-bg-main: #111827;
}
```

## 3. Dynamic Loading Methods

### Method A: Link Injection (Vanilla JS)
```javascript
function loadTheme(themeName) {
  let themeLink = document.getElementById('lazy-theme');
  if (!themeLink) {
    themeLink = document.createElement('link');
    themeLink.id = 'lazy-theme';
    themeLink.rel = 'stylesheet';
    document.head.appendChild(themeLink);
  }
  themeLink.href = `/assets/themes/theme-${themeName}.css`;
}
```

### Method B: Dynamic Imports (React / Bundlers)
```typescript
import { useEffect } from 'react';

function useLazyTheme(theme) {
  useEffect(() => {
    if (theme === 'dark') {
      import('./styles/theme-dark.css');
    } else {
      import('./styles/theme-light.css');
    }
  }, [theme]);
}
```

## Key Best Practices
* **Baseline Theme:** Keep a default theme bundle inline to prevent layout shifts.
* **Early Detection:** Check `localStorage` in the HTML root to fetch assets early.
* **Scan Components:** Keep Tailwind scanning open so utilities compile at build time.