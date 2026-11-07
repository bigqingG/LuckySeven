"""Local WebGL smoke/capture. Requires playwright (only a development tool).
Usage: python Tools/Browser-Smoke.py [URL]
Screenshots are reviewed separately: canvas pixels are not DOM assertions.
"""
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'Library/browser-tools'))
from playwright.sync_api import sync_playwright

url = sys.argv[1] if len(sys.argv) > 1 else 'http://127.0.0.1:8077/'
output = ROOT / 'Docs/Validation/Browser'
output.mkdir(parents=True, exist_ok=True)
messages, failures = [], []
with sync_playwright() as p:
    browser = p.chromium.launch(
        executable_path=r'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe',
        headless=True, args=['--enable-unsafe-swiftshader'])
    page = browser.new_page(viewport={'width': 1280, 'height': 720}, device_scale_factor=1)
    page.on('console', lambda m: messages.append({'type': m.type, 'text': m.text}))
    page.on('pageerror', lambda e: failures.append(str(e)))
    page.goto(url, wait_until='domcontentloaded')
    page.wait_for_function("document.querySelector('#loading').style.display === 'none'", timeout=120000)
    page.wait_for_timeout(4000)
    page.screenshot(path=str(output / '01-menu-1280.png'))
    page.mouse.click(640, 610)
    page.wait_for_timeout(350)
    page.screenshot(path=str(output / '02-help.png'))
    page.mouse.click(640, 570)
    page.wait_for_timeout(250)
    page.mouse.click(640, 540)
    page.wait_for_timeout(400)
    page.screenshot(path=str(output / '03-battle.png'))
    # First fixed hand: ((2 + 3) + 2).
    for x, y in [(270, 520), (697, 520), (397, 520), (697, 520), (522, 520)]:
        page.mouse.click(x, y)
        page.wait_for_timeout(160)
    page.screenshot(path=str(output / '04-seven-preview.png'))
    page.mouse.click(900, 642)
    page.wait_for_timeout(200)
    page.screenshot(path=str(output / '05-lucky-hit.png'))
    page.wait_for_timeout(1400)
    page.screenshot(path=str(output / '06-next-turn.png'))
    page.set_viewport_size({'width': 1920, 'height': 1080})
    page.wait_for_timeout(500)
    page.screenshot(path=str(output / '07-battle-1920.png'))
    page.set_viewport_size({'width': 1024, 'height': 768})
    page.wait_for_timeout(500)
    page.screenshot(path=str(output / '08-letterbox.png'))
    (output / 'console.json').write_text(json.dumps(messages, ensure_ascii=False, indent=2), encoding='utf-8')
    (output / 'summary.json').write_text(json.dumps({'browser': browser.version, 'url': url, 'pageErrors': failures, 'consoleErrors': [m for m in messages if m['type'] == 'error'], 'screenshots': 8}, indent=2), encoding='utf-8')
    print(json.dumps({'browser': browser.version, 'pageErrors': failures, 'screenshots': str(output)}))
    browser.close()
