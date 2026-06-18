export interface SeoIssueItem { type: 'product' | 'category'; id: number; name: string; }

export interface SeoIssueGroup {
  key: string;
  title: string;
  severity: 'warn' | 'danger' | 'info';
  count: number;
  items: SeoIssueItem[];
}

export interface SeoOverview {
  sitemapUrlCount: number;
  productModelCount: number;
  categoryCount: number;
  totalIssues: number;
  groups: SeoIssueGroup[];
}
