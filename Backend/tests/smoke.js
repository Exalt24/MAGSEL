// Smoke test for the login backend. It boots the real app (index.js) against a throwaway MongoDB given by MONGO_URI and checks the
// five behaviours the app exists for: the health route, register, a duplicate register, login and a wrong-password login.
// CI gives it a mongo service; locally run: MONGO_URI=mongodb://localhost:27017/magsel_smoke npm test
const { spawn } = require('node:child_process');
const path = require('node:path');

const PORT = process.env.SMOKE_PORT || '3999';
const BASE = `http://127.0.0.1:${PORT}`;
const MONGO_URI = process.env.MONGO_URI;
if (!MONGO_URI) {
    console.error('MONGO_URI is not set: point it at a throwaway MongoDB');
    process.exit(2);
}

const stamp = Date.now();
const user = { email: `smoke${stamp}@example.com`, name: `smoke${stamp}`, password: 'secret-123' };
let failures = 0;
const check = (name, ok, detail) => {
    console.log(`${ok ? 'PASS' : 'FAIL'} ${name}${ok ? '' : ` (${detail})`}`);
    if (!ok) failures += 1;
};
const post = (route, body) => fetch(`${BASE}${route}`, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(body),
});
const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

async function waitUntilUp(child, ceilingMs) {
    const deadline = Date.now() + ceilingMs;
    while (Date.now() < deadline) {
        if (child.exitCode !== null) throw new Error(`the server exited early with code ${child.exitCode}`);
        try {
            const res = await fetch(`${BASE}/`);
            if (res.status === 200) return;
        } catch (err) {
            // not listening yet: poll again
        }
        await sleep(100);
    }
    throw new Error(`the server did not answer within ${ceilingMs} ms`);
}

async function main() {
    const child = spawn(process.execPath, [path.join(__dirname, '..', 'index.js')], {
        env: { ...process.env, PORT, MONGO_URI },
        stdio: ['ignore', 'pipe', 'pipe'],
    });
    let log = '';
    child.stdout.on('data', (d) => { log += d; });
    child.stderr.on('data', (d) => { log += d; });
    try {
        await waitUntilUp(child, 30000);
        check('GET / answers 200', true);

        const created = await post('/user/register', user);
        check('register creates the user (201)', created.status === 201, `status ${created.status}`);

        const again = await post('/user/register', user);
        check('registering the same email again is refused (400)', again.status === 400, `status ${again.status}`);

        const ok = await post('/user/login', { email: user.email, password: user.password });
        const okBody = await ok.json().catch(() => ({}));
        check('login with the right password works (200) and returns the name', ok.status === 200 && okBody.name === user.name, `status ${ok.status}`);

        const bad = await post('/user/login', { email: user.email, password: 'not-the-password' });
        check('login with a wrong password is refused (400)', bad.status === 400, `status ${bad.status}`);
    } catch (err) {
        check('the app boots and keeps answering', false, err.message);
        console.log('--- server log ---');
        console.log(log.slice(-2000));
    } finally {
        child.kill();
    }
    console.log(failures === 0 ? '\nALL PASS' : `\n${failures} FAILED`);
    process.exit(failures === 0 ? 0 : 1);
}

main();
