---
name: Luxe Industrial Dark
colors:
  surface: '#131313'
  surface-dim: '#131313'
  surface-bright: '#3a3939'
  surface-container-lowest: '#0e0e0e'
  surface-container-low: '#1c1b1b'
  surface-container: '#201f1f'
  surface-container-high: '#2a2a2a'
  surface-container-highest: '#353534'
  on-surface: '#e5e2e1'
  on-surface-variant: '#bccabc'
  inverse-surface: '#e5e2e1'
  inverse-on-surface: '#313030'
  outline: '#879487'
  outline-variant: '#3d4a3f'
  surface-tint: '#61de8a'
  primary: '#61de8a'
  on-primary: '#00391a'
  primary-container: '#27ae60'
  on-primary-container: '#00391a'
  inverse-primary: '#006d37'
  secondary: '#e9c349'
  on-secondary: '#3c2f00'
  secondary-container: '#af8d11'
  on-secondary-container: '#342800'
  tertiary: '#c8c6c5'
  on-tertiary: '#313030'
  tertiary-container: '#9a9898'
  on-tertiary-container: '#313131'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#7efba4'
  primary-fixed-dim: '#61de8a'
  on-primary-fixed: '#00210c'
  on-primary-fixed-variant: '#005228'
  secondary-fixed: '#ffe088'
  secondary-fixed-dim: '#e9c349'
  on-secondary-fixed: '#241a00'
  on-secondary-fixed-variant: '#574500'
  tertiary-fixed: '#e5e2e1'
  tertiary-fixed-dim: '#c8c6c5'
  on-tertiary-fixed: '#1c1b1b'
  on-tertiary-fixed-variant: '#474746'
  background: '#131313'
  on-background: '#e5e2e1'
  surface-variant: '#353534'
  deep-anthracite: '#121212'
  glass-stroke: rgba(255, 255, 255, 0.1)
  forest-glow: rgba(39, 174, 96, 0.15)
typography:
  display-lg:
    fontFamily: EB Garamond
    fontSize: 72px
    fontWeight: '500'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: EB Garamond
    fontSize: 48px
    fontWeight: '500'
    lineHeight: '1.2'
  headline-lg-mobile:
    fontFamily: EB Garamond
    fontSize: 32px
    fontWeight: '500'
    lineHeight: '1.2'
  headline-md:
    fontFamily: EB Garamond
    fontSize: 32px
    fontWeight: '500'
    lineHeight: '1.3'
  title-lg:
    fontFamily: Hanken Grotesk
    fontSize: 20px
    fontWeight: '600'
    lineHeight: '1.5'
    letterSpacing: 0.1em
  body-lg:
    fontFamily: Hanken Grotesk
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.7'
  body-md:
    fontFamily: Hanken Grotesk
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  label-sm:
    fontFamily: Hanken Grotesk
    fontSize: 12px
    fontWeight: '700'
    lineHeight: '1.4'
    letterSpacing: 0.2em
spacing:
  container-max: 1440px
  gutter: 32px
  margin-x: 64px
  section-gap: 128px
  stack-sm: 8px
  stack-md: 24px
---

## Brand & Style

This design system establishes a premium, "High-End Industrial" identity for Orpay Orman Ürünleri. It bridges the gap between raw forest resources and architectural luxury. The aesthetic is defined by a "Midnight Architectural" approach—utilizing deep, layered dark tones to create a sense of vast space and prestige.

The target audience consists of architects, interior designers, and high-end contractors who value precision and craftsmanship. The visual narrative combines **Minimalism** with **Glassmorphism**, using translucent surfaces and frosted blurs to suggest sophistication and depth. The mood is authoritative, technological, and exclusively premium, elevating industrial wood products to the status of architectural art.

## Colors

The palette is anchored in **Dark Mode** to evoke the "Gold Banyo" luxury aesthetic. 

- **Primary:** A refined Corporate Green derived from the Orpay leaf, used for core actions and brand recognition.
- **Secondary:** An Architectural Gold used sparingly for high-value accents, highlights, and "Premium Series" labels.
- **Neutral/Background:** The system utilizes a tiered black system. `#0D0D0D` serves as the base void, while `#121212` and `#1A1A1A` are used for container elevation.
- **Accent Logic:** Use the "Forest Glow" (low-opacity primary) for background blurs behind glass cards to provide a subtle technological feel without breaking the dark aesthetic.

## Typography

The typography strategy pairs the classical elegance of **EB Garamond** with the technical precision of **Hanken Grotesk**. 

- **Serif (Headlines):** Used for emotional storytelling and section headers. High-contrast and elegant, it should be set with slightly tight tracking for a modern editorial feel.
- **Sans-Serif (Functional):** Used for navigation, body text, and technical specifications. 
- **Tracking:** All Hanken Grotesk labels and titles must use expanded letter-spacing (0.1em to 0.2em) to reinforce the luxury/technological feel.
- **Hierarchy:** Large display sizes should be reserved for hero sections, transitioning to clean, legible sans-serif for product catalogs.

## Layout & Spacing

The layout utilizes a **Fixed Grid** philosophy for desktop to maintain a controlled, gallery-like presentation.

- **Desktop:** 12-column grid with a wide 32px gutter. Margins are generous (64px+) to allow the dark background to provide visual "breathing room."
- **Sectioning:** Large vertical gaps (128px) separate major content blocks (e.g., from Brand Story to Product Groups) to emphasize the premium nature of the content.
- **Mobile:** Transition to a single-column fluid layout with 20px margins. Product cards should stack vertically or use a horizontal scroll snap for "Product Groups."
- **Alignment:** Content should predominantly use left-alignment for technical sections, while hero and "Brand Reveal" sections may use centered compositions.

## Elevation & Depth

Hierarchy is established through **Glassmorphism** and **Tonal Layering** rather than traditional heavy shadows.

- **Base Layer:** Background at `#0D0D0D`.
- **Surface Layer (Cards/Panels):** Use a semi-transparent `#1A1A1A` with a 12px Backdrop Blur (Saturate 150%). 
- **Outlines:** Every elevated element must have a 1px solid border using `glass-stroke` (`rgba(255,255,255,0.1)`). This defines "Sharp Lines" requested in the brief.
- **Shadows:** Use "Ambient Glows" instead of drop shadows. A very soft, 40px blur shadow with 5% opacity of the Primary Green can be placed behind featured product cards to make them "hum" with energy.

## Shapes

To reflect the precision of industrial wood processing and door manufacturing, the design system adopts a **Sharp (0px)** roundedness strategy. 

All primary containers, buttons, and product imagery must have 90-degree corners. This creates a structural, architectural aesthetic that feels more "engineered" and "luxurious" than rounded consumer-grade interfaces. Subtle 1px dividers should be used to separate list items in technical specifications.

## Components

- **Buttons:** Primary buttons are sharp-edged, solid Green (`#27AE60`) with white text. Secondary buttons use a "Ghost" style: 1px Gold (`#D4AF37`) border with wide-tracked Hanken Grotesk text.
- **Product Cards:** Featured items (e.g., Orlam Melamin) use large-scale imagery with a glass-morphic overlay at the bottom for the title. Hovering should trigger a subtle zoom-in of the image and an increase in the backdrop blur.
- **Input Fields:** Dark background (`#0D0D0D`) with a bottom-only border. On focus, the border transitions to a Primary Green gradient.
- **Navigation:** A sticky top bar with a backdrop blur effect. Links use `label-sm` styling, changing to Gold on hover.
- **Reveal Animations:** Use "Intersection Observer" to trigger 0.8s ease-out slide-ups for text blocks and scale-ins for images as the user scrolls, creating a premium "reveal" effect.
- **Chips/Labels:** Used for "New" or "Premium" tags. These should be Gold text on a 10% Gold background, sharp-edged.