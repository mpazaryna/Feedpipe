import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  projectSidebar: [
    'intro',
    'roadmap',
    {
      type: 'category',
      label: 'Milestones',
      items: [
        'milestones/foundation',
        'milestones/multi-source-ingestion',
        'milestones/data-transformation',
        'milestones/production-hardening',
      ],
    },
    {
      type: 'category',
      label: 'Decisions',
      items: [
        'decisions/adr-000-the-score',
      ],
    },
    {
      type: 'category',
      label: 'Devlog',
      items: [
        'devlog/2026-03-28-project-kickoff',
      ],
    },
  ],
};

export default sidebars;
