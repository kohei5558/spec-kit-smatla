import { test, expect, childLogin, waitProblem, correctAnswer, answer, API } from './helpers';
import type { Page } from '@playwright/test';

test.beforeEach(async ({ page }) => {
  await childLogin(page);
});

async function nav(page: Page, text: string) {
  await page.locator('.mud-nav-link', { hasText: text }).first().click();
}

/** ホームで計算の種類を選んで、問題画面を開く */
async function startCategory(page: Page, category: string) {
  await page.getByText(category, { exact: true }).first().click();
  return waitProblem(page, () => page.getByRole('button', { name: '問題を解く' }).click());
}

async function solveCorrectly(page: Page, problemId: number) {
  await answer(page, correctAnswer(problemId));
  await expect(page.getByText('正解！ 🎉')).toBeVisible();
}

test('メニューの各リンク', async ({ page }) => {
  await nav(page, '問題を解く');
  await expect(page).toHaveURL(/\/problem$/);
  await expect(page.locator('.problem-display')).toBeVisible();

  await nav(page, '学習結果');
  await expect(page).toHaveURL(/\/results\/\d+$/);
  await expect(page.getByText('統計サマリー')).toBeVisible();

  await nav(page, '景品一覧');
  await expect(page).toHaveURL(/\/rewards$/);
  await expect(page.getByText('あなたの持っているポイント')).toBeVisible();

  await nav(page, '交換申請');
  await expect(page).toHaveURL(/\/my-exchange-requests$/);

  for (const text of ['ページが見つかりません', '読み込みに失敗']) {
    await expect(page.getByText(text)).toHaveCount(0);
  }
});

test('足し算: 正解・不正解の判定と、けたの多い問題のひっ算', async ({ page }) => {
  let problem = await startCategory(page, '足し算');
  for (let i = 0; i < 3; i++) {
    const [, a, b] = problem.question.match(/^(\d+) \+ (\d+) = \?$/)!;
    // どちらかが2けた以上なら、ひっ算の形（016）
    await expect(page.locator('.vertical-calc')).toHaveCount(Number(a) >= 10 || Number(b) >= 10 ? 1 : 0);
    if (i === 0) {
      await answer(page, [correctAnswer(problem.id)[0] + 1]);
      await expect(page.getByText('不正解')).toBeVisible();
    } else {
      await solveCorrectly(page, problem.id);
    }
    problem = await waitProblem(page, () => page.getByRole('button', { name: '次の問題へ' }).click());
  }
});

// 新しい単元の画面（欄の数と単位、時計の絵）。1問ずつ正しく答えられる
const units = [
  { category: 'あまりのあるわり算', check: async (page: Page) => expect(page.locator('.answer-input input')).toHaveCount(2) },
  { category: '時こくと時間', check: async () => {} },
  { category: '長さ・重さ・かさ', check: async () => {} },
  { category: '文章問題', check: async () => {} },
];
for (const { category, check } of units) {
  test(`${category}を1問解く`, async ({ page }) => {
    const problem = await startCategory(page, category);
    const fields = problem.answerFormat === 'Number' ? [] : problem.answerUnits;
    for (const unit of fields) {
      await expect(page.locator('.answer-input').getByText(unit, { exact: true }).last()).toBeVisible();
    }
    if (problem.clockTimes.length > 0) {
      await expect(page.locator('.problem-display svg, svg').first()).toBeVisible();
    }
    await check(page);
    await solveCorrectly(page, problem.id);
  });
}

test('学習結果に種類ごとのタブと集計が出る', async ({ page }) => {
  await nav(page, '学習結果');
  await expect(page.getByText('統計サマリー')).toBeVisible();
  for (const tab of ['すべて', 'たし算', 'あまりあり', '時こく', '単位', '文章']) {
    // タブは横にスクロールするので、表示されているかではなく、あるかを見る
    await expect(page.locator('.mud-tab', { hasText: new RegExp(`^${tab}$`) })).toHaveCount(1);
  }
  await expect(page.getByText('学習記録一覧')).toBeVisible();
});

test('デイリーチャレンジに正解すると、今日はおわりになる', async ({ page }) => {
  const [today] = await Promise.all([
    page.waitForResponse(r => r.url().toLowerCase().startsWith(`${API}/api/dailychallenges/today`) && r.status() === 200),
    page.getByRole('button', { name: '挑戦する！' }).click(),
  ]);
  await page.waitForURL(u => u.pathname === '/daily-challenge');
  await expect(page.locator('.problem-display')).toBeVisible();
  const question: string = (await today.json()).question;
  const [, a, op, b] = question.match(/^(\d+) (.) (\d+)/)!;
  const x = Number(a), y = Number(b);
  const value = op === '+' ? x + y : op === '-' ? x - y : op === '×' ? x * y : Math.floor(x / y);
  await page.locator('.answer-input input').first().fill(String(value));
  await page.getByRole('button', { name: '答える' }).click();
  await expect(page.getByText('せいかい！ 🎉')).toBeVisible();

  await page.getByRole('link', { name: 'ホーム画面に戻る' }).click();
  await expect(page.getByText('今日のチャレンジはおわり')).toBeVisible();
  await expect(page.getByRole('button', { name: '挑戦する！' })).toHaveCount(0);
});

test('景品の交換を申請する', async ({ page }) => {
  await nav(page, '景品一覧');
  const card = page.locator('.reward-card', { hasText: 'うまい棒 チーズ味' });
  // 初期景品の画像が出る
  await expect.poll(() => card.locator('.mud-card-media').evaluate(e => getComputedStyle(e).backgroundImage)).toContain('umaibo');
  await card.getByRole('button', { name: '交換する' }).click();
  const [response] = await Promise.all([
    page.waitForResponse(r => r.url().startsWith(`${API}/api/`) && r.request().method() === 'POST'),
    page.getByRole('button', { name: '交換する' }).last().click(),
  ]);
  expect(response.status()).toBeLessThan(300);

  await nav(page, '交換申請');
  await expect(page.locator('tr', { hasText: 'うまい棒 チーズ味' }).getByText('申請中')).toBeVisible();
});
