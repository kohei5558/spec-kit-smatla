// E2E 用に API を起動する。毎回まっさらな SQLite の DB（.data/e2e.db）から始める（デモの保護者・子供はシードで作られる）
import { spawn } from 'node:child_process';
import { mkdirSync, rmSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const e2eDir = join(dirname(fileURLToPath(import.meta.url)), '..');
const dbPath = join(e2eDir, '.data', 'e2e.db');
mkdirSync(dirname(dbPath), { recursive: true });
for (const suffix of ['', '-wal', '-shm']) {
  rmSync(dbPath + suffix, { force: true });
}

const api = spawn('dotnet', ['run', '--project', join(e2eDir, '..', 'backend', 'src', 'GamifiedMathDrill.Api'), '--urls', 'http://localhost:5242'], {
  stdio: 'inherit',
  env: {
    ...process.env,
    ASPNETCORE_ENVIRONMENT: 'Development',
    // E2E だけで使う鍵（本物の鍵ではない）。User Secrets が無い CI でも起動できるように渡す
    Jwt__SecretKey: 'e2e-only-key-e2e-only-key-e2e-only-key',
    ConnectionStrings__DefaultConnection: `Data Source=${dbPath}`,
  },
});
for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => api.kill(signal));
}
api.on('exit', code => process.exit(code ?? 0));
