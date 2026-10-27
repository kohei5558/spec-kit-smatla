import { test, expect } from './helpers';

test('子供の交換申請を承認する（詳細に初期景品の画像が出る）', async ({ page }) => {
  await page.goto('/exchange-requests');
  const row = page.locator('tr', { hasText: 'うまい棒 チーズ味' }).filter({ hasText: '申請中' }).first();
  await expect(row).toBeVisible();

  // 詳細で初期景品の画像が出る（以前は出なかった）
  await row.locator('[title="詳細"]').click();
  await page.waitForURL(/\/exchange-request\/\d+$/);
  const image = page.locator('img[src*="umaibo"]').first();
  await expect.poll(() => image.evaluate(i => (i as HTMLImageElement).complete && (i as HTMLImageElement).naturalWidth > 0)).toBe(true);

  await page.goto('/exchange-requests');
  await page.locator('tr', { hasText: 'うまい棒 チーズ味' }).filter({ hasText: '申請中' }).first().locator('[title="承認"]').click();
  await page.getByRole('button', { name: '承認する' }).click();
  await expect(page.locator('tr', { hasText: 'うまい棒 チーズ味' }).filter({ hasText: '承認済み' }).first()).toBeVisible();
});
