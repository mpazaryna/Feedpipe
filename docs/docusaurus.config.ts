import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Feedpipe',
  tagline: 'A production-ready data pipeline for multi-source content aggregation',
  favicon: 'img/favicon.ico',

  future: {
    v4: true,
  },

  url: 'https://mpazaryna.github.io',
  baseUrl: '/Feedpipe/',

  organizationName: 'mpazaryna',
  projectName: 'Feedpipe',

  onBrokenLinks: 'throw',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/mpazaryna/Feedpipe/tree/main/docs/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'Feedpipe',
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'projectSidebar',
          position: 'left',
          label: 'Docs',
        },
        {
          href: 'https://github.com/mpazaryna/Feedpipe',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Documentation',
          items: [
            { label: 'Roadmap', to: '/docs/roadmap' },
            { label: 'Foundation', to: '/docs/milestones/foundation' },
            { label: 'Decisions', to: '/docs/decisions/adr-000-the-score' },
          ],
        },
        {
          title: 'Links',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/mpazaryna/Feedpipe',
            },
          ],
        },
      ],
      copyright: `Copyright ${new Date().getFullYear()} Feedpipe. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'json', 'bash'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
