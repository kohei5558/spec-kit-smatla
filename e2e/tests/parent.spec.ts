import { join } from 'node:path';
import { test, expect } from './helpers';
import type { Locator, Page } from '@playwright/test';

const fixture = (name: string) => join(import.meta.dirname, '..', 'fixtures', name);

async function expectImageLoaded(img: Locator) {
  await expect.poll(() => img.evaluate(i => (i as HTMLImageElement).complete && (i as HTMLImageElement).naturalWidth > 0)).toBe(true);
}

async function nav(page: Page, text: string) {
  await page.locator('.mud-nav-link', { hasText: text }).first().click();
}

test('ダッシュボードとメニューの各リンク', async ({ page }) => {
  await page.goto('/');
  await page.waitForURL(u => u.pathname === '/parent-dashboard');
  // 子供のアバターの画像が出る
  const avatars = page.locator('img[src^="/avatars/"]');
  await expect(avatars.first()).toBeVisible();
  await expectImageLoaded(avatars.first());

  await nav(page, '景品管理');
  await expect(page).toHaveURL(/\/parent\/rewards$/);
  await expect(page.getByText('景品一覧')).toBeVisible();

  await nav(page, '交換申請');
  await expect(page).toHaveURL(/\/exchange-requests$/);
  await expect(page.getByText('ページが見つかりません')).toHaveCount(0);

  await nav(page, 'ホーム');
  await expect(page).toHaveURL(/\/parent-dashboard$/);
});

test('景品を画像つきで追加し、画像を変えて編集する', async ({ page }) => {
  const name = `E2E の景品 ${Date.now()}`;
  await page.goto('/parent/rewards/create');
  await page.locator('input[type=file]').setInputFiles(fixture('reward-orange.png'));
  await page.getByLabel('景品名').fill(name);
  await page.getByLabel('説明').fill('自動テストで追加');
  await page.getByLabel('必要ポイント').fill('5');
  await page.getByRole('button', { name: '保存' }).click();
  await expect(page.getByText('景品を追加しました')).toBeVisible();
  await page.waitForURL(u => u.pathname === '/parent/rewards');

  const row = page.locator('tr', { hasText: name });
  const image = row.locator(`img[alt="${name}"]`);
  await expectImageLoaded(image);
  const before = await image.getAttribute('src');

  // 既存の景品の画像を変えても保存できる（以前はエラーになった）
  await row.locator('[title="編集"]').click();
  await page.waitForURL(/\/parent\/rewards\/edit\/\d+$/);
  await page.locator('input[type=file]').setInputFiles(fixture('reward-blue.png'));
  await page.getByLabel('景品名').fill(`${name} 改`);
  await page.getByRole('button', { name: '保存' }).click();
  await expect(page.getByText('景品を更新しました')).toBeVisible();
  await page.waitForURL(u => u.pathname === '/parent/rewards');

  const edited = page.locator('tr', { hasText: `${name} 改` }).locator('img');
  await expectImageLoaded(edited);
  expect(await edited.getAttribute('src')).not.toBe(before);
});

test('子供アカウントを作る（ほかの子と同じ PIN なら注意が出る）', async ({ page }) => {
  const name = `E2E${Date.now() % 100000}`;
  await page.goto('/parent/children/create');
  await page.locator('#name').fill(name);
  await page.locator('#grade').selectOption('3');
  const pins = page.locator('.pin-input-container input');
  for (let i = 0; i < 4; i++) {
    await pins.nth(i).fill('1234'[i]); // 花子と同じ
  }
  await page.locator('button[type=submit]').click();
  // 本物のサーバーでだけ起きていた不具合（日本語のヘッダーで 400）の確認も兼ねる
  await expect(page.getByText('ほかの子供と同じ PIN です')).toBeVisible();
  await page.waitForURL(u => u.pathname === '/parent/children');
  await expect(page.getByRole('heading', { name, exact: true })).toBeVisible();
});
