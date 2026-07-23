# Design System: Orpay Orman Ürünleri

## 1. Visual Theme & Atmosphere
Premium industrial luxury with warm-dark tension. Speaks to architects and builders through material quality and precision. Dark-ground with warm gold accent and emerald CTAs. Density is balanced (Level 4), variance is moderate with asymmetric hero layouts (Level 6), and motion is fluid with spring physics (Level 5). The atmosphere: premium, warm, intentional — industrial yet refined.

## 2. Color Palette & Roles
- **Obsidian Void** (#0d0d0d) — Primary background. Deep warm black with subtle brown undertone. Never pure black
- **Elevated Surface** (#1a1a1a) — Card/container fill on dark mode. Slightly lighter than background
- **Warm Off-White** (#e5e2e1) — Primary text on dark backgrounds
- **Gold Signature** (#e8c84a) — Primary accent. CTAs, active states, decorative dividers, header tabs
- **Gold Dim** (#c4a035) — Accent hover/active, secondary gold tones
- **Gold Bright** (#e8c84a) — Hover states on gold elements, shimmer effects
- **Emerald Action** (#27ae60) — CTA buttons and interactive states only. Never decorative
- **Emerald Hover** (#2ecc71) — Button hover state
- **Ivory Paper** (#f8f6f2) — Warm off-white background for light sections. Not clinical white
- **Charcoal Ink** (#2c2c2c) — Primary text on light backgrounds
- **Warm Steel** (#8a8a8a) — Secondary text, descriptions, metadata
- **Muted Stone** (#9a9a9a) — Tertiary text, timestamps, disabled states
- **Whisper Border** (rgba(232,200,74,0.15)) — Card borders, structural lines with gold tint
- **Diffused Shadow** (rgba(0,0,0,0.08)) — Card elevation. Wide-spreading, soft focus

### Banned Colors
- Pure Black (#000000) — use Obsidian Void instead
- Cold blue-white (#f0f5ff type) — use warm neutrals
- Purple/Violet neon — no AI-purple aesthetic
- Oversaturated accents above 80% saturation
- Mixed warm/cool gray systems

## 3. Typography Rules
- **Display/Headlines:** `Noto Serif` — Track-tight (-0.02em), weight-driven hierarchy (600–900). Fluid scale: `clamp(2rem, 4vw, 3.5rem)`. Luxury editorial feel. Never all-caps except short badges
- **Body:** `Manrope` — Relaxed leading (1.6), 65ch max-width. Warm Steel color (#6c6c6c) for secondary text
- **Accent/Vurgu:** `Cormorant Garamond` — For pull quotes, decorative numerals, hero subtitles. Italic weight 300 for elegance
- **Mono:** `JetBrains Mono` — For metadata, technical specs. Tabular numbers with `font-variant-numeric: tabular-nums`

### Banned Fonts
- `Inter` — banned in premium/creative contexts
- Generic serif (`Times New Roman`, `Georgia`, `Garamond`) — use Noto Serif or Cormorant Garamond instead
- System font stacks as primary — use the defined font family stack

## 4. Component Stylings
- **Buttons:** Emerald fill (#27ae60) for primary CTA. Gold Signature (#e8c84a) for secondary/ghost. Active: subtle scale(0.97). Hover: background shift. No outer glow. Minimum 44px touch target
- **Cards/Containers:** Generously rounded corners (1.5rem). Warm off-white fill. Whisper gold-tinted border (1px). Diffused shadow. Internal padding 1.5rem–2rem
- **Inputs/Forms:** Label above input. Border: warm steel. Focus ring in Gold Signature with 2px offset. Error: deep rose. No floating labels
- **Navigation:** Minimal frosted glass effect when sticky. Gold underline on active item. Clean horizontal with generous spacing
- **Hero Section:** Split-screen or asymmetric layout. Product imagery on one side, editorial text on other. No centered hero on high-variance layouts. One primary CTA.
- **Loaders:** Skeletal shimmer in warm tones matching layout. No circular spinners
- **Empty States:** Composed illustration with guidance text. Not just "No data found"
- **3D Viewer:** Full-bleed container with subtle border. Controls overlay bottom-left

## 5. Layout Principles
- **Grid-First:** CSS Grid for structural layouts. Asymmetric bento grids for feature sections
- **Containment:** Max-width 1400px, centered. Horizontal padding: 1rem mobile, 2rem tablet, 4rem desktop
- **Feature Sections:** No 3-equal-card rows. Use 2-column zig-zag, asymmetric bento, or horizontal scroll
- **Full-Height:** Use `min-height: 100dvh` — never `height: 100vh`
- **Dark-Light Contrast:** Alternate between Obsidian Void and Ivory Paper backgrounds to create rhythm
- **Gold Divider:** Section transitions use a thin gold line (1px, #e8c84a at 40% opacity) as visual punctuation

## 6. Responsive Rules
- **Mobile-First (< 768px):** All multi-column collapses to single column. Width 100%, padding 1rem
- **No Horizontal Scroll:** Critical failure if any element causes overflow
- **Typography:** Headlines scale via `clamp()`. Body text minimum 1rem/14px
- **Touch Targets:** All interactive minimum 44px. Buttons full-width on mobile
- **3D Viewer:** Full-width on mobile, controls simplified to icon-only
- **Navigation:** Desktop horizontal collapses to slide-in mobile menu

## 7. Motion & Interaction
- **Spring Physics:** `stiffness: 100, damping: 20` for interactive elements. No linear easing
- **Perpetual Micro-Interactions:** Gold shimmer on hero headings, subtle float on decorative gold elements
- **Staggered Orchestration:** Cascade reveals with `animation-delay: calc(var(--index) * 100ms)`
- **3D Transitions:** Smooth orbit controls, fade-in on model load
- **Hardware Rules:** Animate only `transform` and `opacity`. Never `top`, `left`, `width`, `height`
- **Performance:** 60fps minimum. Isolate heavy animations in leaf components

## 8. Anti-Patterns (Banned)
- No emojis anywhere in UI
- No `Inter` font — use defined stack
- No pure black (#000000)
- No neon glows or oversaturated gradients
- No 3-column equal card layouts for features
- No centered hero sections (at this variance level)
- No "Scroll to explore", "Swipe down" filler text
- No generic names ("John Doe", "Acme")
- No fake round numbers ("99.99%")
- No AI copywriting clichés ("Elevate", "Seamless", "Next-Gen")
- No broken Unsplash links — use project media from /medya/
- No circular spinners — skeletal shimmer only
- No `h-screen` — always `min-h-[100dvh]`
