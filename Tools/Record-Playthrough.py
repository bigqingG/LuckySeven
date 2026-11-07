"""Records actual WebGL canvas/audio while accepting explicit mouse actions.
No gameplay state injection. Commands and screenshots are kept as test evidence.
"""
import json
import pathlib
import sys
import time

ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'Library/browser-tools'))
from playwright.sync_api import sync_playwright

OUT = ROOT / 'Docs/Validation/Playthrough20261106'
OUT.mkdir(parents=True, exist_ok=True)
request = OUT / 'request.json'
messages, errors, actions = [], [], []
with sync_playwright() as p:
    browser = p.chromium.launch(
        executable_path=r'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe',
        headless=True, args=['--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required'])
    page = browser.new_page(viewport={'width': 1280, 'height': 720}, device_scale_factor=1, accept_downloads=True)
    page.add_init_script('''
        window.__recordTaps = new Map();
        window.__captureContext = new AudioContext();
        window.__captureDestination = window.__captureContext.createMediaStreamDestination();
        window.__captureSources = [];
        const originalConnect = AudioNode.prototype.connect;
        AudioNode.prototype.connect = function(...args) {
            const result = originalConnect.apply(this, args);
            if (args[0] === this.context.destination) {
                let tap = window.__recordTaps.get(this.context);
                if (!tap) {
                    tap = this.context.createMediaStreamDestination();
                    window.__recordTaps.set(this.context, tap);
                    const source = window.__captureContext.createMediaStreamSource(tap.stream);
                    originalConnect.call(source, window.__captureDestination);
                    window.__captureSources.push(source);
                }
                originalConnect.call(this, tap);
            }
            return result;
        };
    ''')
    page.on('console', lambda m: messages.append({'type': m.type, 'text': m.text}))
    page.on('pageerror', lambda e: errors.append(str(e)))
    page.goto('http://127.0.0.1:8077/', wait_until='domcontentloaded')
    page.wait_for_function("document.querySelector('#loading').style.display === 'none'", timeout=120000)
    page.wait_for_timeout(4500)
    tracks = page.evaluate('''async () => {
        const stream = document.querySelector('canvas').captureStream(30);
        await window.__captureContext.resume();
        for (const track of window.__captureDestination.stream.getAudioTracks()) stream.addTrack(track);
        window.__chunks = [];
        window.__recorder = new MediaRecorder(stream, {mimeType:'video/webm;codecs=vp9,opus', videoBitsPerSecond:2800000});
        window.__recorder.ondataavailable = event => { if(event.data.size) window.__chunks.push(event.data); };
        window.__recorder.start(1000);
        return stream.getTracks().map(t => t.kind);
    }''')
    started = time.monotonic()
    page.screenshot(path=str(OUT / '00-menu.png'))
    (OUT / 'status.json').write_text(json.dumps({'state': 'ready', 'tracks': tracks}), encoding='utf-8')
    print('READY ' + json.dumps(tracks), flush=True)
    while True:
        if not request.exists():
            page.wait_for_timeout(100)
            continue
        command = json.loads(request.read_text(encoding='utf-8-sig'))
        request.unlink()
        actions.append({'seconds': round(time.monotonic() - started, 2), **command})
        if command.get('finish'):
            page.wait_for_timeout(2200)
            await_js = '''() => new Promise(resolve => {
                window.__recorder.onstop = () => {
                    const blob = new Blob(window.__chunks, {type:'video/webm'});
                    const a = document.createElement('a'); a.href = URL.createObjectURL(blob);
                    a.download = 'LuckySeven-Playthrough.webm'; a.click(); resolve();
                }; window.__recorder.stop();
            })'''
            with page.expect_download(timeout=30000) as download:
                page.evaluate(await_js)
            download.value.save_as(str(OUT / 'LuckySeven-Playthrough.webm'))
            break
        for action in command.get('clicks', []):
            x, y = action[:2]
            page.mouse.move(x, y, steps=8)
            page.wait_for_timeout(180)
            page.mouse.click(x, y, delay=90)
            page.wait_for_timeout(action[2] if len(action) > 2 else 550)
        page.wait_for_timeout(command.get('wait', 500))
        name = command['name']
        page.screenshot(path=str(OUT / (name + '.png')))
        (OUT / 'status.json').write_text(json.dumps({'state': 'ready', 'step': name}), encoding='utf-8')
        (OUT / 'actions.json').write_text(json.dumps(actions, indent=2), encoding='utf-8')
        print('STEP ' + name, flush=True)
    (OUT / 'actions.json').write_text(json.dumps(actions, indent=2), encoding='utf-8')
    (OUT / 'console.json').write_text(json.dumps(messages, indent=2), encoding='utf-8')
    (OUT / 'summary.json').write_text(json.dumps({'browser':browser.version,'durationSeconds':time.monotonic()-started,'tracks':tracks,'pageErrors':errors,'consoleErrors':[m for m in messages if m['type']=='error']}, indent=2), encoding='utf-8')
    (OUT / 'status.json').write_text('{"state":"finished"}', encoding='utf-8')
    browser.close()
    print('FINISHED', flush=True)
