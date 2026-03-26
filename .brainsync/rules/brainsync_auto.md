

# Project Memory — Quanlydoankham
> 273 notes | Score threshold: >40

## Safety — Never Run Destructive Commands

> Dangerous commands are actively monitored.
> Critical/high risk commands trigger error notifications in real-time.

- **NEVER** run `rm -rf`, `del /s`, `rmdir`, `format`, or any command that deletes files/directories without EXPLICIT user approval.
- **NEVER** run `DROP TABLE`, `DELETE FROM`, `TRUNCATE`, or any destructive database operation.
- **NEVER** run `git push --force`, `git reset --hard`, or any command that rewrites history.
- **NEVER** run `npm publish`, `docker rm`, `terraform destroy`, or any irreversible deployment/infrastructure command.
- **NEVER** pipe remote scripts to shell (`curl | bash`, `wget | sh`).
- **ALWAYS** ask the user before running commands that modify system state, install packages, or make network requests.
- When in doubt, **show the command first** and wait for approval.

**Stack:** JavaScript · Tailwind + Vue

## 📝 NOTE: 1 uncommitted file(s) in working tree.\n\n## Important Warnings

- **gotcha in QuanLyDoanKham.API.pdb** — File updated (external): QuanLyDoanKham.API/bin/Debug/net10.0/QuanLyDo
- **gotcha in QuanLyDoanKham.API.pdb** — File updated (external): QuanLyDoanKham.API/bin/Debug/net10.0/QuanLyDo
- **gotcha in rjsmrazor.dswa.cache.json** — File updated (external): QuanLyDoanKham.API/obj/Debug/net10.0/rjsmrazo
- **gotcha in rjsmcshtml.dswa.cache.json** — File updated (external): QuanLyDoanKham.API/obj/Debug/net10.0/rjsmcsht
- **gotcha in QuanLyDoanKham.API.pdb** — File updated (external): QuanLyDoanKham.API/obj/Debug/net10.0/QuanLyDo
- **gotcha in QuanLyDoanKham.API.runtimeconfig.json** — File updated (external): QuanLyDoanKham.API/bin/Debug/net10.0/QuanLyDo

## Active: `.`

- **convention in .gitignore**
- **gotcha in QuanLyDoanKham.API.pdb**
- **gotcha in QuanLyDoanKham.API.pdb**
- **gotcha in rjsmrazor.dswa.cache.json**
- **gotcha in rjsmcshtml.dswa.cache.json**

## Project Standards

- convention in .gitignore
- [.cursorrules] `find(query)` — Full-text lookup
- [.cursorrules] `query(q)` — Deep search when stuck
- [.cursorrules] When in doubt, **show the command first** and wait for approval.
- [.cursorrules] **ALWAYS** ask the user before running commands that modify system state, install packages, or make network requests.
- [.cursorrules] **NEVER** pipe remote scripts to shell (`curl | bash`, `wget | sh`).
- [.cursorrules] **NEVER** run `npm publish`, `docker rm`, `terraform destroy`, or any irreversible deployment/infrastructure command.
- [.cursorrules] **NEVER** run `git push --force`, `git reset --hard`, or any command that rewrites history.

## Recent Decisions

- decision in Users.vue
- decision in Contracts.vue
- decision in Groups.vue
- decision in Program.cs

## Learned Patterns

- Avoid: gotcha in QuanLyDoanKham.API.pdb (seen 2x)
- Avoid: gotcha in rpswa.dswa.cache.json (seen 2x)
- Avoid: gotcha in QuanLyDoanKham.API.GeneratedMSBuildEditorConfig.editorconfig (seen 2x)
- Agent generates new migration for every change (squash related changes)
- Agent installs packages without checking if already installed

### 📚 Core Framework Rules: [expo/expo-tailwind-setup]
# Tailwind CSS Setup for Expo with react-native-css

This guide covers setting up Tailwind CSS v4 in Expo using react-native-css and NativeWind v5 for universal styling across iOS, Android, and Web.

## Overview

This setup uses:

- **Tailwind CSS v4** - Modern CSS-first configuration
- **react-native-css** - CSS runtime for React Native
- **NativeWind v5** - Metro transformer for Tailwind in React Native
- **@tailwindcss/postcss** - PostCSS plugin for Tailwind v4

## Installation



Add resolutions for lightningcss compatibility:



- autoprefixer is not needed in Expo because of lightningcss
- postcss is included in expo by default

## Configuration Files

### Metro Config

Create or update `metro.config.js`:



### PostCSS Config

Create `postcss.config.mjs`:



### Global CSS

Create `src/global.css`:



## IMPORTANT: No Babel Config Needed

With Tailwind v4 and NativeWind v5, you do NOT need a babel.config.js for Tailwind. Remove any NativeWind babel presets if present:



## CSS Component Wrappers

Since react-native-css requires explicit CSS element wrapping, create reusable components:

### Main Components (`src/tw/index.tsx`)



### Image Component (`src/tw/image.tsx`)



### Animated Components (`src/tw/animated.tsx`)



## Usage

Import CSS-wrapped components from your tw directory:



## Custom Theme Variables

Add custom theme variables in your global.css using `@theme`:



## Platform-Specific Styles

Use platform media queries for platform-specific styling:



## Apple System Colors with CSS Variables

Create a CSS file for Apple semantic colors:



Then use in components:



## Using CSS Variables in JavaScript

Use the `useCSSVariable` hook:



## Key Differences from NativeWind v4 / Tailwind v3

1. **No babel.config.js** - Configuration is now CSS-first
2. **PostCSS plugin** - Uses `@tailwindcss/postcss` instead of `tailwindcss`
3. **CSS imports** - Use `@import "tailwindcss/..."` instead of `@tailwind` directives
4. **Theme config** - Use `@theme` in CSS instead of `tailwind.config.js`
5. **Component wrappers** - Must wrap components with `useCssElement` for className support
6. **Metro config** - Use `withNativewind` with different options (`inlineVariables: false`)

## Troubleshooting

### Styles not applying

1. Ensure you have the CSS file imported in your app entry
2. Check that components are wrapped with `useCssElement`
3. Verify Metro config has `withNativewind` applied

### Platform colors not working

1. Use `platformColor()` i...
(truncated)


### 📚 Core Framework Rules: [czlonkowski/n8n-code-javascript]
# JavaScript Code Node

Expert guidance for writing JavaScript code in n8n Code nodes.

---

## Quick Start



### Essential Rules

1. **Choose "Run Once for All Items" mode** (recommended for most use cases)
2. **Access data**: `$input.all()`, `$input.first()`, or `$input.item`
3. **CRITICAL**: Must return `[{json: {...}}]` format
4. **CRITICAL**: Webhook data is under `$json.body` (not `$json` directly)
5. **Built-ins available**: $helpers.httpRequest(), DateTime (Luxon), $jmespath()

---

## Mode Selection Guide

The Code node offers two execution modes. Choose based on your use case:

### Run Once for All Items (Recommended - Default)

**Use this mode for:** 95% of use cases

- **How it works**: Code executes **once** regardless of input count
- **Data access**: `$input.all()` or `items` array
- **Best for**: Aggregation, filtering, batch processing, transformations, API calls with all data
- **Performance**: Faster for multiple items (single execution)



**When to use:**
- ✅ Comparing items across the dataset
- ✅ Calculating totals, averages, or statistics
- ✅ Sorting or ranking items
- ✅ Deduplication
- ✅ Building aggregated reports
- ✅ Combining data from multiple items

### Run Once for Each Item

**Use this mode for:** Specialized cases only

- **How it works**: Code executes **separately** for each input item
- **Data access**: `$input.item` or `$item`
- **Best for**: Item-specific logic, independent operations, per-item validation
- **Performance**: Slower for large datasets (multiple executions)



**When to use:**
- ✅ Each item needs independent API call
- ✅ Per-item validation with different error handling
- ✅ Item-specific transformations based on item properties
- ✅ When items must be processed separately for business logic

**Decision Shortcut:**
- **Need to look at multiple items?** → Use "All Items" mode
- **Each item completely independent?** → Use "Each Item" mode
- **Not sure?** → Use "All Items" mode (you can always loop inside)

---

## Data Access Patterns

### Pattern 1: $input.all() - Most Common

**Use when**: Processing arrays, batch operations, aggregations



### Pattern 2: $input.first() - Very Common

**Use when**: Working with single objects, API responses, first-in-first-out



### Pattern 3: $input.item - Each Item Mode Only

**Use when**: In "Run Once for Each Item" mode



### Pattern 4: $node - Reference Other Nodes

**Use when**: Need data from specific nodes in workflow



**See**: [DATA_ACCESS.md](DATA_ACCESS.md) ...
(truncated)

- [JavaScript/TypeScript] Use === not == (strict equality prevents type coercion bugs)
- [JavaScript/TypeScript] Use const by default, let when reassignment needed, never var

## Available Tools (ON-DEMAND only)
- `query(q)` — Deep search when stuck
- `find(query)` — Full-text lookup
> Context above IS your context. Do NOT call load() at startup.
