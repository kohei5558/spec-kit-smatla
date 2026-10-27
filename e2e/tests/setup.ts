import { writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { test, expect, DATA_DIR, PARENT } from './helpers';

// 保護者でログインし、この端末を子供用に登録する。
// parent.json: 保護者のログイン状態（＋端末）、device.json: 端末の登録だけ（子供ログイン用）
test('保護者ログインと端末登録', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('メールアドレス').fill(PARENT.email);
  await page.getByLabel('パスワード').fill(PARENT.password);
  await page.getByLabel('ログイン状態を保持する').check();
  await page.locator('button[type=submit]').click();
  await page.waitForURL(u => u.pathname === '/parent-dashboard');

  await page.goto('/parent/children');
  await page.getByRole('button', { name: 'この端末を子供用に登録する' }).click();
  await page.locator('#deviceName').fill('E2E の端末');
  await page.getByRole('button', { name: '登録する', exact: true }).click();
  await expect(page.getByText('この端末は子供用に登録済みです')).toBeVisible();

  const state = await page.context().storageState({ path: join(DATA_DIR, 'parent.json') });
  const device = {
    cookies: [],
    origins: state.origins.map(o => ({ ...o, localStorage: o.localStorage.filter(item => item.name === 'deviceToken') })),
  };
  expect(device.origins.some(o => o.localStorage.length === 1)).toBe(true);
  writeFileSync(join(DATA_DIR, 'device.json'), JSON.stringify(device));
});
