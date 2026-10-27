import { DatabaseSync } from 'node:sqlite';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { test as base, expect, type Page } from '@playwright/test';

export { expect };

export const API = 'http://localhost:5242';
export const DATA_DIR = join(dirname(fileURLToPath(import.meta.url)), '..', '.data');

// シードで作られるデモの家族（Development のみ）
export const PARENT = { email: 'parent@example.com', password: 'Parent123!' };
export const HANAKO = { name: '花子', pin: '1234' }; // 3年生

/**
 * どのテストでも、画面の例外と API の 500 番台があれば失敗にする
 */
export const test = base.extend<{ watchErrors: void }>({
  watchErrors: [
    async ({ page }, use) => {
      const problems: string[] = [];
      page.on('pageerror', e => problems.push(`画面の例外: ${e.message}`));
      page.on('response', r => {
        if (r.url().startsWith(API) && r.status() >= 500) {
          problems.push(`API ${r.status()} ${r.request().method()} ${new URL(r.url()).pathname}`);
        }
      });
      await use();
      expect(problems).toEqual([]);
    },
    { auto: true },
  ],
});

/**
 * 子供ログイン（端末登録ずみの storageState で使う）
 */
export async function childLogin(page: Page, child = HANAKO) {
  await page.goto('/child-login');
  await page.locator('.child-card', { hasText: child.name }).click();
  const boxes = page.locator('.pin-box');
  for (let i = 0; i < child.pin.length; i++) {
    await boxes.nth(i).click();
    await page.keyboard.type(child.pin[i]);
  }
  await page.getByRole('button', { name: 'ログイン', exact: true }).click();
  await page.waitForURL(u => u.pathname === '/');
}

export type ShownProblem = { id: number; question: string; answerFormat: string; answerUnits: string[]; clockTimes: string[] };

/**
 * 画面が問題を読み込むのを待つ。action（クリックなど）で次の問題が読み込まれる
 */
export async function waitProblem(page: Page, action: () => Promise<unknown>): Promise<ShownProblem> {
  const [response] = await Promise.all([
    page.waitForResponse(r => r.url().startsWith(`${API}/api/problems/next`) && r.status() === 200),
    action(),
  ]);
  const problem = (await response.json()).data as ShownProblem;
  await expect(page.locator('.problem-display')).toBeVisible();
  return problem;
}

/**
 * 問題の正解を E2E 用の DB から読む（API は正解を返さないため）
 */
export function correctAnswer(problemId: number): number[] {
  const db = new DatabaseSync(join(DATA_DIR, 'e2e.db'), { readOnly: true });
  try {
    const row = db.prepare('SELECT CorrectAnswer, CorrectRemainder FROM Problems WHERE Id = ?').get(problemId) as
      { CorrectAnswer: number; CorrectRemainder: number | null } | undefined;
    if (!row) {
      throw new Error(`E2E の DB に問題 ${problemId} がない`);
    }
    return row.CorrectRemainder === null ? [row.CorrectAnswer] : [row.CorrectAnswer, row.CorrectRemainder];
  } finally {
    db.close();
  }
}

/**
 * 表示中の問題に答えて、正解・不正解の表示を待つ
 */
export async function answer(page: Page, values: number[]) {
  const inputs = page.locator('.answer-input input');
  await expect(inputs).toHaveCount(values.length);
  for (let i = 0; i < values.length; i++) {
    await inputs.nth(i).fill(String(values[i]));
  }
  await page.getByRole('button', { name: '答える' }).click();
  await expect(page.getByRole('button', { name: '次の問題へ' })).toBeVisible();
}
