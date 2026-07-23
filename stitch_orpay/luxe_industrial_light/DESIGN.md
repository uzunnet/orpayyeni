---
name: Luxe Industrial Light
colors:
  surface: '#f4fbf1'
  surface-dim: '#d5dcd2'
  surface-bright: '#f4fbf1'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff6ec'
  surface-container: '#e9f0e6'
  surface-container-high: '#e3eae0'
  surface-container-highest: '#dde5db'
  on-surface: '#171d17'
  on-surface-variant: '#3d4a3f'
  inverse-surface: '#2b322c'
  inverse-on-surface: '#ecf3e9'
  outline: '#6d7a6e'
  outline-variant: '#bccabc'
  surface-tint: '#006d37'
  primary: '#006d37'
  on-primary: '#ffffff'
  primary-container: '#27ae60'
  on-primary-container: '#00391a'
  inverse-primary: '#61de8a'
  secondary: '#4e6073'
  on-secondary: '#ffffff'
  secondary-container: '#cfe2f9'
  on-secondary-container: '#526478'
  tertiary: '#595f63'
  on-tertiary: '#ffffff'
  tertiary-container: '#93999d'
  on-tertiary-container: '#2b3135'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#7efba4'
  primary-fixed-dim: '#61de8a'
  on-primary-fixed: '#00210c'
  on-primary-fixed-variant: '#005228'
  secondary-fixed: '#d1e4fb'
  secondary-fixed-dim: '#b5c8df'
  on-secondary-fixed: '#091d2e'
  on-secondary-fixed-variant: '#36485b'
  tertiary-fixed: '#dde3e7'
  tertiary-fixed-dim: '#c1c7cb'
  on-tertiary-fixed: '#161c20'
  on-tertiary-fixed-variant: '#41484b'
  background: '#f4fbf1'
  on-background: '#171d17'
  surface-variant: '#dde5db'
typography:
  display-lg:
    fontFamily: EB Garamond
    fontSize: 48px
    fontWeight: '500'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: EB Garamond
    fontSize: 32px
    fontWeight: '500'
    lineHeight: '1.2'
  headline-md:
    fontFamily: EB Garamond
    fontSize: 32px
    fontWeight: '500'
    lineHeight: '1.2'
  headline-sm:
    fontFamily: EB Garamond
    fontSize: 24px
    fontWeight: '500'
    lineHeight: '1.3'
  body-lg:
    fontFamily: Work Sans
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Work Sans
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  label-caps:
    fontFamily: JetBrains Mono
    fontSize: 12px
    fontWeight: '500'
    lineHeight: '1.0'
    letterSpacing: 0.1em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 8px
  gutter: 24px
  margin-desktop: 64px
  margin-mobile: 20px
  container-max: 1280px
---

## Brand & Style
The design system embodies a "Luxe Industrial" aesthetic, blending the precision of architectural engineering with the airy sophistication of high-end galleries. It targets a professional audience that values structural integrity, clarity, and premium craftsmanship. 

The style is a hybrid of **Minimalism** and **Glassmorphism**, grounded by **Corporate Modern** principles. The interface should feel bright and expansive, utilizing high-quality whitespace and "engineered" details like hairline borders and technical alignments to evoke a sense of professional reliability and expensive simplicity.

## Colors
The palette is centered on a warm, architectural off-white surface that prevents the "clinical" feel of pure white.
- **Surface (#F7F7F7):** The foundation for all layouts, providing a warm, sophisticated backdrop.
- **Primary (#27AE60):** A corporate green used purposefully for calls to action and key indicators, suggesting growth and stability.
- **Deep Charcoal (#2C3E50):** Used for primary typography and structural lines to ensure high legibility and a grounded feel.
- **Metallic Silver (#BDC3C7):** A technical accent used for borders, inactive states, and subtle decorative elements.
- **Glass Effects:** Surfaces use a semi-transparent white (rgba(255, 255, 255, 0.7)) with a 20px backdrop blur to create depth without visual clutter.

## Typography
The typographic hierarchy relies on the tension between the classic elegance of **EB Garamond** and the functional precision of **Work Sans**. 
- **Headlines:** Use EB Garamond for a premium, editorial feel. Keep tracking tight on larger sizes.
- **Body:** Work Sans provides a grounded, neutral experience for long-form reading and data.
- **Technical Accents:** JetBrains Mono is utilized for labels and metadata to reinforce the "engineered" industrial theme. All labels should be uppercase with increased letter spacing.

## Layout & Spacing
The layout follows a **Fixed Grid** system on desktop to maintain architectural symmetry, transitioning to a **Fluid Grid** on mobile.
- **Grid:** A 12-column system with 24px gutters. Elements should align strictly to the grid to maintain the "structural" feel.
- **Rhythm:** Use an 8px base unit for all padding and margins. 
- **Negative Space:** Embrace generous margins (64px+) between major sections to emphasize the "Luxe" aspect of the design.
- **Mobile:** Scale margins down to 20px and collapse columns to a single-stack, maintaining the same 8px rhythm.

## Elevation & Depth
Depth is communicated through **Glassmorphism** and **Tonal Layers** rather than heavy shadows.
- **Planes:** Primary content sits on "Frosted Glass" panes. Use a 1px solid border in `#BDC3C7` (Silver) at 40% opacity to define the edges of these planes.
- **Shadows:** Use extremely subtle, large-radius ambient shadows (e.g., `0 20px 40px rgba(0,0,0,0.04)`) only for high-level floating elements like modals.
- **Interactive Depth:** When hovered, elements should not "lift" with shadows, but rather shift in background opacity or gain a slightly more defined border.

## Shapes
The shape language is "Soft" yet disciplined. While industrial design often leans toward sharp corners, this system uses subtle rounding to ensure the UI feels approachable and high-end.
- **Base Radius:** 4px (0.25rem) for inputs and small components.
- **Container Radius:** 8px (0.5rem) for cards and glass panes.
- **Buttons:** Maintain the 4px radius to keep them looking sharp and intentional.

## Components
- **Buttons:** Primary buttons use a solid `#27AE60` fill with white text. Secondary buttons use a "Ghost" style with a 1px `#2C3E50` border. Use the `label-caps` typography for button text.
- **Cards:** Use the "Glass" effect (frosted white background + subtle silver border). Avoid shadows; rely on the border for definition.
- **Inputs:** Square-ish with 4px radius. Use a light silver border that darkens to `#27AE60` on focus. Background should be slightly lighter than the main surface.
- **Chips/Badges:** Use JetBrains Mono text. Backgrounds should be a very pale tint of the primary color or a simple silver stroke.
- **Navigation:** Top-tier navigation should use EB Garamond for a sophisticated first impression, while sub-navigation uses Work Sans.
- **Dividers:** Use 1px hairline strokes in `#BDC3C7` at 30% opacity to separate content sections without breaking the visual flow.