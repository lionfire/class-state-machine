# LionFire Documentation Platform Architecture

## Executive Summary

This document outlines the architecture for a unified documentation platform serving multiple LionFire.* open source projects, with future expansion to include Axi and other projects. The platform uses **Astro Starlight** as the primary documentation framework with **DocFX** providing automated .NET API reference generation.

**Target URL:** `https://lionfire.software` (or `https://docs.lionfire.software`)

---

## Goals & Requirements

| Requirement | Solution |
|-------------|----------|
| Multi-project umbrella | Starlight multi-sidebar with folder-based organization |
| Markdown-based content | Native Starlight support (.md, .mdx) |
| Git integration | GitHub-native workflow, PR-based updates |
| Version pivots | Branch-based versioning with path rewrites |
| .NET API docs | DocFX metadata → Starlight pipeline |
| Low maintenance | AI-assisted content curation, automated builds |
| Custom domain | GitHub Pages CNAME or VPS nginx config |
| Search | Built-in Pagefind (upgradeable to Algolia) |

---

## Technology Stack

```
┌─────────────────────────────────────────────────────────────────┐
│                        lionfire.software                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐        │
│   │  LionFire   │    │  LionFire   │    │    Axi      │        │
│   │    Core     │    │   Vos       │    │             │  ...   │
│   └──────┬──────┘    └──────┬──────┘    └──────┬──────┘        │
│          │                  │                  │                │
│          ▼                  ▼                  ▼                │
│   ┌─────────────────────────────────────────────────────┐      │
│   │              Astro Starlight                         │      │
│   │  • Multi-sidebar navigation                          │      │
│   │  • Pagefind search                                   │      │
│   │  • i18n-ready                                        │      │
│   │  • Dark/light themes                                 │      │
│   └─────────────────────────────────────────────────────┘      │
│                              │                                  │
│          ┌───────────────────┼───────────────────┐             │
│          ▼                   ▼                   ▼             │
│   ┌────────────┐      ┌────────────┐      ┌────────────┐       │
│   │  Markdown  │      │   DocFX    │      │   Assets   │       │
│   │  Content   │      │  API Docs  │      │  & Media   │       │
│   └────────────┘      └────────────┘      └────────────┘       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                 GitHub Actions CI/CD Pipeline
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
      GitHub Pages                    VPS (Optional)
      (Primary)                       (Self-hosted)
```

### Core Technologies

| Component | Technology | Purpose |
|-----------|------------|---------|
| Framework | Astro 5.x | Static site generation, islands architecture |
| Doc Theme | Starlight | Documentation-specific features, components |
| API Docs | DocFX 2.x | .NET metadata extraction, YAML generation |
| Search | Pagefind | Client-side search, zero runtime cost |
| Build | GitHub Actions | Automated builds, deployments |
| Hosting | GitHub Pages | Free, CDN-backed, custom domain support |

---

## Repository Structure

### Option A: Monorepo (Recommended)

All documentation lives in a single repository for unified search, consistent theming, and simpler CI/CD.

```
lionfire-docs/
├── .github/
│   └── workflows/
│       ├── build-deploy.yml        # Main build pipeline
│       └── docfx-metadata.yml      # Scheduled API doc updates
│
├── astro.config.mjs                # Starlight configuration
├── package.json
├── tsconfig.json
│
├── src/
│   ├── content/
│   │   ├── docs/
│   │   │   ├── index.mdx                    # Main landing page
│   │   │   │
│   │   │   ├── lionfire-core/               # Project 1
│   │   │   │   ├── index.md                 # Project overview
│   │   │   │   ├── getting-started.md
│   │   │   │   ├── guides/
│   │   │   │   │   ├── installation.md
│   │   │   │   │   └── configuration.md
│   │   │   │   └── api/                     # Generated from DocFX
│   │   │   │       ├── index.md
│   │   │   │       ├── LionFire.Core.md
│   │   │   │       └── LionFire.Core.Extensions.md
│   │   │   │
│   │   │   ├── lionfire-vos/                # Project 2
│   │   │   │   ├── index.md
│   │   │   │   ├── concepts/
│   │   │   │   │   ├── virtual-filesystem.md
│   │   │   │   │   └── mounting.md
│   │   │   │   └── api/
│   │   │   │
│   │   │   ├── lionfire-trading/            # Project 3
│   │   │   │   └── ...
│   │   │   │
│   │   │   ├── axi/                         # Future: Axi orchestration
│   │   │   │   └── ...
│   │   │   │
│   │   │   └── meta/                        # Cross-cutting docs
│   │   │       ├── contributing.md
│   │   │       ├── architecture.md
│   │   │       └── changelog.md
│   │   │
│   │   └── i18n/                            # Future: translations
│   │
│   ├── components/                          # Custom Astro components
│   │   ├── ProjectCard.astro
│   │   └── VersionBadge.astro
│   │
│   ├── styles/
│   │   └── custom.css                       # LionFire branding
│   │
│   └── assets/
│       ├── lionfire-logo.svg
│       └── project-icons/
│
├── scripts/
│   ├── docfx-to-starlight.ts               # Transform DocFX YAML → MDX
│   ├── sync-api-docs.sh                    # Pull API docs from source repos
│   └── validate-links.ts                   # Pre-deploy link checker
│
├── docfx/                                   # DocFX configuration
│   ├── docfx.json                          # Multi-project config
│   └── templates/                          # Custom API doc templates
│
└── public/
    ├── favicon.svg
    └── CNAME                               # lionfire.software
```

### Option B: Federated (Source Repos Own Their Docs)

Each LionFire.* repo has a `/docs` folder; the main docs site pulls them in at build time.

```
# In each source repo (e.g., LionFire.Core)
LionFire.Core/
├── src/
├── tests/
└── docs/                    # Docs live with code
    ├── index.md
    ├── getting-started.md
    └── guides/

# In the docs aggregator repo
lionfire-docs/
├── scripts/
│   └── aggregate-docs.sh    # Clones/pulls docs from source repos
└── ...
```

**Recommendation:** Start with Option A (monorepo). It's simpler to maintain, has unified search, and you can always split later. Federated adds complexity without much benefit until you have external contributors.

---

## Starlight Configuration

### `astro.config.mjs`

```javascript
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

export default defineConfig({
  site: 'https://lionfire.software',
  integrations: [
    starlight({
      title: 'LionFire',
      logo: {
        src: './src/assets/lionfire-logo.svg',
        replacesTitle: true,
      },
      
      // Social links
      social: {
        github: 'https://github.com/lionfire',
      },
      
      // Custom CSS for LionFire branding
      customCss: ['./src/styles/custom.css'],
      
      // Enable search
      pagefind: true,
      
      // Multi-project sidebar configuration
      sidebar: [
        {
          label: 'Home',
          link: '/',
        },
        {
          label: 'LionFire Core',
          collapsed: true,
          autogenerate: { directory: 'lionfire-core' },
          badge: { text: 'Stable', variant: 'success' },
        },
        {
          label: 'LionFire Vos',
          collapsed: true,
          autogenerate: { directory: 'lionfire-vos' },
          badge: { text: 'Beta', variant: 'caution' },
        },
        {
          label: 'LionFire Trading',
          collapsed: true,
          autogenerate: { directory: 'lionfire-trading' },
          badge: { text: 'Alpha', variant: 'danger' },
        },
        {
          label: 'Axi',
          collapsed: true,
          autogenerate: { directory: 'axi' },
          badge: { text: 'Coming Soon', variant: 'note' },
        },
        {
          label: 'Contributing',
          autogenerate: { directory: 'meta' },
        },
      ],
      
      // Default frontmatter for all pages
      defaults: {
        tableOfContents: { minHeadingLevel: 2, maxHeadingLevel: 3 },
      },
      
      // Code block themes
      expressiveCode: {
        themes: ['github-dark', 'github-light'],
      },
      
      // Components that can be used in MDX
      components: {
        // Override default components if needed
      },
    }),
  ],
});
```

### Multi-Sidebar Plugin (Optional Enhancement)

For truly independent navigation per project, install `starlight-utils`:

```bash
npm install starlight-utils
```

```javascript
// astro.config.mjs
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import { multiSidebar } from 'starlight-utils';

export default defineConfig({
  integrations: [
    starlight({
      plugins: [
        multiSidebar([
          {
            path: 'lionfire-core',
            sidebar: [
              { label: 'Overview', link: '/lionfire-core/' },
              { label: 'Guides', autogenerate: { directory: 'lionfire-core/guides' } },
              { label: 'API Reference', autogenerate: { directory: 'lionfire-core/api' } },
            ],
          },
          {
            path: 'lionfire-vos',
            sidebar: [
              { label: 'Overview', link: '/lionfire-vos/' },
              { label: 'Concepts', autogenerate: { directory: 'lionfire-vos/concepts' } },
              { label: 'API Reference', autogenerate: { directory: 'lionfire-vos/api' } },
            ],
          },
          // ... more projects
        ]),
      ],
    }),
  ],
});
```

---

## DocFX → Starlight Pipeline

### Overview

DocFX generates YAML metadata from your .NET assemblies. We transform this into Starlight-compatible MDX files.

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  .NET Projects   │────▶│     DocFX        │────▶│   YAML Files     │
│  (XML Comments)  │     │  docfx metadata  │     │   (api/*.yml)    │
└──────────────────┘     └──────────────────┘     └────────┬─────────┘
                                                           │
                                                           ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  Starlight Site  │◀────│  Transform       │◀────│   Parse YAML     │
│  (MDX files)     │     │  (Node script)   │     │   Extract data   │
└──────────────────┘     └──────────────────┘     └──────────────────┘
```

### DocFX Configuration

Create `docfx/docfx.json`:

```json
{
  "metadata": [
    {
      "src": [
        {
          "files": ["**/LionFire.Core.csproj"],
          "src": "../src"
        }
      ],
      "dest": "api/lionfire-core",
      "disableGitFeatures": false,
      "disableDefaultFilter": false
    },
    {
      "src": [
        {
          "files": ["**/LionFire.Vos.csproj"],
          "src": "../src"
        }
      ],
      "dest": "api/lionfire-vos"
    }
  ],
  "build": {
    "content": [
      {
        "files": ["api/**/*.yml", "api/**/*.md"]
      }
    ],
    "dest": "_site",
    "globalMetadataFiles": [],
    "fileMetadataFiles": [],
    "template": ["default", "modern"],
    "postProcessors": [],
    "markdownEngineProperties": {
      "enableSourceInfo": true
    }
  }
}
```

### Transform Script

Create `scripts/docfx-to-starlight.ts`:

```typescript
#!/usr/bin/env npx ts-node

/**
 * Transforms DocFX YAML output into Starlight-compatible MDX files.
 * 
 * Usage: npx ts-node scripts/docfx-to-starlight.ts
 */

import * as fs from 'fs';
import * as path from 'path';
import * as yaml from 'js-yaml';

interface DocFXItem {
  uid: string;
  id: string;
  name: string;
  fullName: string;
  type: 'Namespace' | 'Class' | 'Interface' | 'Enum' | 'Method' | 'Property';
  summary?: string;
  remarks?: string;
  syntax?: {
    content: string;
    parameters?: Array<{ id: string; type: string; description: string }>;
    return?: { type: string; description: string };
  };
  children?: string[];
  parent?: string;
}

interface DocFXYaml {
  items: DocFXItem[];
  references?: Array<{ uid: string; name: string; fullName: string }>;
}

const API_SOURCE_DIR = './docfx/api';
const API_OUTPUT_DIR = './src/content/docs';

function transformToMdx(item: DocFXItem, allItems: Map<string, DocFXItem>): string {
  const lines: string[] = [];
  
  // Frontmatter
  lines.push('---');
  lines.push(`title: ${item.name}`);
  lines.push(`description: ${item.summary?.replace(/\n/g, ' ') || `API reference for ${item.fullName}`}`);
  lines.push('---');
  lines.push('');
  
  // Type badge
  const typeBadge = {
    Class: '🔷 Class',
    Interface: '🔶 Interface',
    Enum: '📋 Enum',
    Namespace: '📁 Namespace',
    Method: '⚙️ Method',
    Property: '📌 Property',
  }[item.type] || item.type;
  
  lines.push(`> ${typeBadge}`);
  lines.push('');
  
  // Full name
  lines.push(`\`${item.fullName}\``);
  lines.push('');
  
  // Summary
  if (item.summary) {
    lines.push('## Summary');
    lines.push('');
    lines.push(item.summary);
    lines.push('');
  }
  
  // Syntax
  if (item.syntax?.content) {
    lines.push('## Declaration');
    lines.push('');
    lines.push('```csharp');
    lines.push(item.syntax.content);
    lines.push('```');
    lines.push('');
  }
  
  // Parameters
  if (item.syntax?.parameters?.length) {
    lines.push('## Parameters');
    lines.push('');
    lines.push('| Name | Type | Description |');
    lines.push('|------|------|-------------|');
    for (const param of item.syntax.parameters) {
      lines.push(`| \`${param.id}\` | \`${param.type}\` | ${param.description || '-'} |`);
    }
    lines.push('');
  }
  
  // Return value
  if (item.syntax?.return) {
    lines.push('## Returns');
    lines.push('');
    lines.push(`\`${item.syntax.return.type}\``);
    if (item.syntax.return.description) {
      lines.push('');
      lines.push(item.syntax.return.description);
    }
    lines.push('');
  }
  
  // Remarks
  if (item.remarks) {
    lines.push('## Remarks');
    lines.push('');
    lines.push(item.remarks);
    lines.push('');
  }
  
  // Children (for namespaces/classes)
  if (item.children?.length) {
    const childItems = item.children
      .map(uid => allItems.get(uid))
      .filter((i): i is DocFXItem => i !== undefined);
    
    const classes = childItems.filter(i => i.type === 'Class');
    const interfaces = childItems.filter(i => i.type === 'Interface');
    const enums = childItems.filter(i => i.type === 'Enum');
    
    if (classes.length) {
      lines.push('## Classes');
      lines.push('');
      for (const cls of classes) {
        const link = cls.uid.replace(/\./g, '/').toLowerCase();
        lines.push(`- [\`${cls.name}\`](./${link}) - ${cls.summary?.split('\n')[0] || ''}`);
      }
      lines.push('');
    }
    
    if (interfaces.length) {
      lines.push('## Interfaces');
      lines.push('');
      for (const iface of interfaces) {
        const link = iface.uid.replace(/\./g, '/').toLowerCase();
        lines.push(`- [\`${iface.name}\`](./${link}) - ${iface.summary?.split('\n')[0] || ''}`);
      }
      lines.push('');
    }
    
    if (enums.length) {
      lines.push('## Enums');
      lines.push('');
      for (const enm of enums) {
        const link = enm.uid.replace(/\./g, '/').toLowerCase();
        lines.push(`- [\`${enm.name}\`](./${link}) - ${enm.summary?.split('\n')[0] || ''}`);
      }
      lines.push('');
    }
  }
  
  return lines.join('\n');
}

async function processProject(projectName: string): Promise<void> {
  const sourceDir = path.join(API_SOURCE_DIR, projectName);
  const outputDir = path.join(API_OUTPUT_DIR, projectName, 'api');
  
  if (!fs.existsSync(sourceDir)) {
    console.log(`Skipping ${projectName}: source directory not found`);
    return;
  }
  
  // Ensure output directory exists
  fs.mkdirSync(outputDir, { recursive: true });
  
  // Build item map from all YAML files
  const allItems = new Map<string, DocFXItem>();
  const yamlFiles = fs.readdirSync(sourceDir).filter(f => f.endsWith('.yml'));
  
  for (const file of yamlFiles) {
    const content = fs.readFileSync(path.join(sourceDir, file), 'utf-8');
    const parsed = yaml.load(content) as DocFXYaml;
    
    if (parsed?.items) {
      for (const item of parsed.items) {
        allItems.set(item.uid, item);
      }
    }
  }
  
  // Generate MDX for each item
  for (const [uid, item] of allItems) {
    // Skip methods/properties (inline them in parent)
    if (item.type === 'Method' || item.type === 'Property') continue;
    
    const mdxContent = transformToMdx(item, allItems);
    const outputPath = path.join(outputDir, `${uid.replace(/\./g, '-').toLowerCase()}.mdx`);
    
    fs.writeFileSync(outputPath, mdxContent);
    console.log(`Generated: ${outputPath}`);
  }
  
  // Generate index
  const namespaces = Array.from(allItems.values()).filter(i => i.type === 'Namespace');
  const indexContent = generateApiIndex(projectName, namespaces);
  fs.writeFileSync(path.join(outputDir, 'index.md'), indexContent);
  console.log(`Generated: ${outputDir}/index.md`);
}

function generateApiIndex(projectName: string, namespaces: DocFXItem[]): string {
  const lines: string[] = [
    '---',
    `title: ${projectName} API Reference`,
    `description: Complete API documentation for ${projectName}`,
    '---',
    '',
    `# ${projectName} API Reference`,
    '',
    'This section contains auto-generated API documentation from the source code.',
    '',
    '## Namespaces',
    '',
  ];
  
  for (const ns of namespaces.sort((a, b) => a.fullName.localeCompare(b.fullName))) {
    const link = ns.uid.replace(/\./g, '-').toLowerCase();
    lines.push(`- [\`${ns.fullName}\`](./${link}) - ${ns.summary?.split('\n')[0] || ''}`);
  }
  
  return lines.join('\n');
}

// Main execution
async function main() {
  const projects = ['lionfire-core', 'lionfire-vos', 'lionfire-trading'];
  
  for (const project of projects) {
    console.log(`\nProcessing ${project}...`);
    await processProject(project);
  }
  
  console.log('\n✅ API documentation generation complete!');
}

main().catch(console.error);
```

### Install Dependencies

```bash
npm install js-yaml @types/js-yaml ts-node typescript --save-dev
```

---

## GitHub Actions Workflow

### Primary Build & Deploy

Create `.github/workflows/build-deploy.yml`:

```yaml
name: Build and Deploy Documentation

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: true

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'

      - name: Install dependencies
        run: npm ci

      - name: Build Starlight site
        run: npm run build

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: ./dist

  deploy:
    if: github.ref == 'refs/heads/main'
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### Scheduled API Doc Updates

Create `.github/workflows/update-api-docs.yml`:

```yaml
name: Update API Documentation

on:
  schedule:
    # Run weekly on Sunday at midnight
    - cron: '0 0 * * 0'
  workflow_dispatch:
    inputs:
      project:
        description: 'Project to update (or "all")'
        required: false
        default: 'all'

jobs:
  update-api-docs:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout docs repo
        uses: actions/checkout@v4

      - name: Checkout LionFire.Core
        uses: actions/checkout@v4
        with:
          repository: lionfire/LionFire.Core
          path: source-repos/LionFire.Core

      - name: Checkout LionFire.Vos
        uses: actions/checkout@v4
        with:
          repository: lionfire/LionFire.Vos
          path: source-repos/LionFire.Vos

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Install DocFX
        run: dotnet tool install -g docfx

      - name: Generate API metadata
        run: |
          cd docfx
          docfx metadata

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Transform to Starlight MDX
        run: |
          npm ci
          npx ts-node scripts/docfx-to-starlight.ts

      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v6
        with:
          commit-message: 'docs: update API documentation'
          title: '📚 Update API Documentation'
          body: |
            Automated update of API documentation from source repositories.
            
            Projects updated:
            - LionFire.Core
            - LionFire.Vos
            
            Please review the changes before merging.
          branch: automated/api-docs-update
          delete-branch: true
```

---

## Versioning Strategy

### Option 1: Branch-Based Versioning (Recommended)

Each major version lives in its own branch. The hosting platform handles routing.

```
main           → lionfire.software/           (latest/v3)
v2             → lionfire.software/v2/
v1             → lionfire.software/v1/
```

**Implementation:**

1. Create version branches: `git checkout -b v2`

2. In version branch, update `astro.config.mjs`:
   ```javascript
   export default defineConfig({
     base: '/v2/',
     outDir: './dist/v2',
     // ...
   });
   ```

3. Configure hosting (GitHub Pages or Vercel/Netlify) to build from multiple branches.

### Option 2: Folder-Based Versioning

All versions in a single branch, organized by folder.

```
src/content/docs/
├── lionfire-core/
│   ├── v3/           # Current (also symlinked to root)
│   ├── v2/
│   └── v1/
```

**Pros:** Single branch to maintain
**Cons:** Larger repo, more complex sidebar config

### Recommendation

Start with **no versioning**—you can add it later when you actually have breaking changes worth preserving. When the time comes, use branch-based versioning with the hosting platform's branch deploy feature.

---

## Hosting Options

### GitHub Pages (Recommended to Start)

**Pros:**
- Free, fast CDN
- Native GitHub integration
- Custom domain support
- Zero maintenance

**Setup:**
1. Add `public/CNAME` with `lionfire.software`
2. Configure DNS: CNAME record pointing to `lionfire.github.io`
3. Enable GitHub Pages in repo settings

### Self-Hosted (Your VPS)

**Pros:**
- Full control
- Can add server-side features later
- No vendor lock-in

**Nginx Configuration:**

```nginx
server {
    listen 80;
    listen 443 ssl http2;
    server_name lionfire.software;

    ssl_certificate /etc/letsencrypt/live/lionfire.software/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/lionfire.software/privkey.pem;

    root /var/www/lionfire-docs;
    index index.html;

    # Handle clean URLs
    location / {
        try_files $uri $uri/ $uri.html =404;
    }

    # Cache static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
}
```

**Deployment Script:**

```bash
#!/bin/bash
# deploy.sh - Run from GitHub Actions or manually

set -e

REMOTE_USER="deploy"
REMOTE_HOST="your-vps.example.com"
REMOTE_PATH="/var/www/lionfire-docs"

# Build
npm run build

# Deploy
rsync -avz --delete ./dist/ $REMOTE_USER@$REMOTE_HOST:$REMOTE_PATH/

echo "✅ Deployed to $REMOTE_HOST"
```

---

## Custom Theming

### `src/styles/custom.css`

```css
/* LionFire brand colors */
:root {
  --sl-color-accent-low: #1a1a2e;
  --sl-color-accent: #e94560;
  --sl-color-accent-high: #ff6b6b;
  
  --sl-color-white: #ffffff;
  --sl-color-gray-1: #f5f5f5;
  --sl-color-gray-2: #e0e0e0;
  --sl-color-gray-3: #bdbdbd;
  --sl-color-gray-4: #757575;
  --sl-color-gray-5: #424242;
  --sl-color-gray-6: #212121;
  --sl-color-black: #0f0f0f;
  
  /* Custom font (optional) */
  --sl-font: 'Inter', system-ui, sans-serif;
  --sl-font-mono: 'JetBrains Mono', 'Fira Code', monospace;
}

/* Dark mode adjustments */
:root[data-theme='dark'] {
  --sl-color-accent-low: #2a1a3e;
  --sl-color-accent: #ff6b6b;
  --sl-color-accent-high: #ffa8a8;
}

/* Project badges in sidebar */
.sl-sidebar-group[data-project="core"] .sl-badge {
  background-color: var(--sl-color-green);
}

.sl-sidebar-group[data-project="beta"] .sl-badge {
  background-color: var(--sl-color-orange);
}

/* Code block enhancements */
.expressive-code {
  --ec-brdRad: 8px;
}

/* API reference styling */
.api-reference h2 {
  border-bottom: 2px solid var(--sl-color-accent);
  padding-bottom: 0.5rem;
}

.api-parameter-table {
  font-size: 0.9rem;
}

.api-parameter-table code {
  background: var(--sl-color-gray-6);
  padding: 0.1rem 0.3rem;
  border-radius: 4px;
}
```

---

## Search Configuration

### Default: Pagefind (Included)

Starlight includes Pagefind by default. No configuration needed—it just works.

### Upgrade Path: Algolia DocSearch

When you outgrow Pagefind (unlikely for most projects), you can apply for free DocSearch:

1. Apply at https://docsearch.algolia.com/
2. Once approved, update config:

```javascript
// astro.config.mjs
starlight({
  pagefind: false, // Disable built-in
  components: {
    Search: './src/components/AlgoliaSearch.astro',
  },
});
```

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1)

- [ ] Create `lionfire-docs` repository
- [ ] Initialize Astro + Starlight project
- [ ] Set up basic folder structure
- [ ] Configure multi-project sidebar
- [ ] Add LionFire branding (logo, colors)
- [ ] Deploy to GitHub Pages
- [ ] Configure `lionfire.software` domain

### Phase 2: Content Migration (Week 2-3)

- [ ] Migrate existing README content to Starlight
- [ ] Create getting-started guides for each project
- [ ] Add architecture/concepts documentation
- [ ] Set up contributing guidelines

### Phase 3: API Documentation (Week 4)

- [ ] Configure DocFX for .NET projects
- [ ] Create transform script (YAML → MDX)
- [ ] Set up GitHub Action for scheduled updates
- [ ] Test with LionFire.Core as pilot

### Phase 4: Polish (Week 5+)

- [ ] Add interactive examples (if desired)
- [ ] Set up versioning (when needed)
- [ ] Configure Algolia (if Pagefind insufficient)
- [ ] Add Axi documentation
- [ ] Create custom components (ProjectCard, etc.)

---

## Quick Start Commands

```bash
# Create new Starlight project
npm create astro@latest -- --template starlight lionfire-docs
cd lionfire-docs

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Install DocFX (globally, requires .NET SDK)
dotnet tool install -g docfx

# Generate API metadata
cd docfx && docfx metadata

# Transform to Starlight
npx ts-node scripts/docfx-to-starlight.ts
```

---

## References

- [Starlight Documentation](https://starlight.astro.build/)
- [Astro Documentation](https://docs.astro.build/)
- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [Starlight Multi-Sidebar Plugin](https://starlight-utils.pages.dev/utilities/multi-sidebar/)
- [GitHub Pages Custom Domains](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site)

---

*Document Version: 1.0*
*Last Updated: December 2024*
*Author: Generated for LionFire project*
