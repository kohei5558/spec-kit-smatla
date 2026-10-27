import { defineConfig } from '@playwright/test';

// 画面の流れの自動テスト（specs/018-e2e-tests）。本物の API と画面を起動して動かす
// 順番: setup（保護者ログイン・端末登録）→ parent（保護者の画面）と child（子供の画面）→ approve（子供の交換申請を保護者が承認）
export default defineConfig({
  testDir: './tests',
  // DB を共有し、ログインの回数制限もあるので1つずつ動かす
  workers: 1,
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,
  timeout: 90_000,
  expect: { timeout: 15_000 },
  reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : [['list']],
  use: {
    baseURL: 'http://localhost:5071',
    locale: 'ja-JP',
    timezoneId: 'Asia/Tokyo',
    viewport: { width: 1100, height: 900 },
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'setup', testMatch: /setup\.ts$/ },
    { name: 'parent', testMatch: /parent\.spec\.ts$/, dependencies: ['setup'], use: { storageState: '.data/parent.json' } },
    { name: 'child', testMatch: /child\.spec\.ts$/, dependencies: ['setup'], use: { storageState: '.data/device.json' } },
    { name: 'approve', testMatch: /approve\.spec\.ts$/, dependencies: ['child'], use: { storageState: '.data/parent.json' } },
  ],
  webServer: [
    {
      command: 'node scripts/start-api.mjs',
      url: 'http://localhost:5242/healthz',
      timeout: 300_000,
      // いつもまっさらな DB で動かすため、起動中のサーバーは使わない（開発中の API・画面は止めてから動かす）
      reuseExistingServer: false,
    },
    {
      command: 'dotnet run --project ../frontend/GamifiedMathDrill.Client --urls http://localhost:5071',
      url: 'http://localhost:5071',
      timeout: 300_000,
      reuseExistingServer: false,
    },
  ],
});
