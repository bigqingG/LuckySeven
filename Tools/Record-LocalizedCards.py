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

OUT = ROOT / 'Docs/Validation/LocalizedCards/Browser'
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
    page.goto('http://127.0.0.1:8078/', wait_until='domcontentloaded')
    page.wait_for_function("document.querySelector('#loading').style.display === 'none'", timeout=120000)
    page.wait_for_function("window.luckySevenSnapshot?.state === 'MainMenu'", timeout=120000)
    page.wait_for_timeout(1500)
    tracks = page.evaluate('''async () => {
        const stream = document.querySelector('canvas').captureStream(30);
        await window.__captureContext.resume();
        // Keep the audio track clock running from frame zero, including silent menus.
        window.__silent = window.__captureContext.createConstantSource();
        window.__silent.offset.value = 0;
        window.__silent.connect(window.__captureDestination);
        window.__silent.start();
        for (const track of window.__captureDestination.stream.getAudioTracks()) stream.addTrack(track);
        window.__chunks = [];
        window.__recorder = new MediaRecorder(stream, {mimeType:'video/webm;codecs=vp9,opus', videoBitsPerSecond:2800000});
        window.__recorder.ondataavailable = event => { if(event.data.size) window.__chunks.push(event.data); };
        window.__recorder.start(1000);
        return stream.getTracks().map(t => t.kind);
    }''')
    started=time.monotonic()
    def state():return page.evaluate('window.luckySevenSnapshot')
    def shot(name):page.screenshot(path=str(OUT/(name+'.png')))
    def click(x,y,delay=700):
        before=state();page.mouse.move(x,y,steps=8);page.wait_for_timeout(170);page.mouse.click(x,y,delay=90);page.wait_for_timeout(delay)
        actions.append({'seconds':round(time.monotonic()-started,2),'click':[x,y],'before':before,'after':state()})
    def position(index):
        s=state();picked=s['selected'][index];group=[i for i,v in enumerate(s['selected']) if v==picked];n=len(group);j=group.index(index)
        spacing=min(112,900/max(1,n)) if picked else min(58,1000/max(1,n));width=104 if picked else 100
        start=(1280-((n-1)*spacing+width))/2
        return start+j*spacing+22,395 if picked else 590
    def card(i):click(*position(i))
    def drag(i,x,y):
        before=state();start=position(i);page.mouse.move(*start,steps=8);page.wait_for_timeout(400);page.mouse.down();page.mouse.move(x,y,steps=26);page.wait_for_timeout(250);page.mouse.up();page.wait_for_timeout(700)
        actions.append({'drag':[list(start),[x,y]],'before':before,'after':state()})
    def language(expected):
        click(970,38,1000);page.wait_for_function('expected=>window.luckySevenSnapshot.locale===expected',arg=expected)
    assert state()['locale']=='zh-Hans'
    print('LOCALIZED RECORDING READY',flush=True);shot('00-menu-chinese');page.wait_for_timeout(1500)
    click(640,610);shot('01-rules-chinese');page.wait_for_timeout(2500)
    language('en');shot('02-rules-english');page.wait_for_timeout(1700)
    language('zh-Hans');click(640,585);click(640,540);shot('03-lucky-chinese');click(515,407);click(465,508)
    shot('04-poker-hand');page.wait_for_timeout(1200)
    for i in [0,1,2,3]:
        page.mouse.move(*position(i),steps=16);page.wait_for_timeout(850);shot('05-hover-'+str(i))
    page.mouse.move(*position(0),steps=12);page.wait_for_timeout(600)
    x,_=position(0);click(x,535);assert state()['selected'][0];shot('05-click-lifted-face');card(0);assert not any(state()['selected'])
    # With the first face raised, its neighbour must still be clickable.
    page.mouse.move(*position(0),steps=10);page.wait_for_timeout(500);card(1);assert state()['selected'][1] and not state()['selected'][0]
    card(1);assert not any(state()['selected']);drag(0,590,410);card(1);assert sum(v for v,b in zip(state()['values'],state()['selected']) if b)==7
    shot('06-lucky-seven-chinese');page.wait_for_timeout(1500)
    before=state();language('en');assert state()['values']==before['values'] and state()['selected']==before['selected'];shot('07-battle-english')
    language('zh-Hans');click(890,508);assert state()['state']=='AttackChoice';shot('08-break-chinese')
    card(0);shot('09-attack-card');click(890,508,1800);shot('10-counter-chinese')
    click(770,508);card(0);card(1);shot('11-exchange-chinese');click(890,508);page.mouse.move(1200,300);page.wait_for_timeout(1200);shot('12-final-hand')
    with page.expect_download(timeout=30000) as download:
        page.evaluate('''()=>new Promise(resolve=>{window.__recorder.onstop=()=>{const a=document.createElement('a');a.href=URL.createObjectURL(new Blob(window.__chunks,{type:'video/webm'}));a.download='LocalizedCards.webm';a.click();resolve();};window.__recorder.stop();})''')
    download.value.save_as(str(OUT/'LocalizedCards.webm'))
    for w,h in [(960,540),(1600,900)]:page.set_viewport_size({'width':w,'height':h});page.wait_for_timeout(900);shot(f'responsive-{w}x{h}')
    page.set_viewport_size({'width':1280,'height':720});page.wait_for_timeout(600)
    language('en');page.wait_for_timeout(1800);page.reload(wait_until='domcontentloaded');page.wait_for_function("window.luckySevenSnapshot?.state==='MainMenu'",timeout=120000);assert state()['locale']=='en';shot('13-reload-english')
    language('zh-Hans');page.wait_for_timeout(1800);page.reload(wait_until='domcontentloaded');page.wait_for_function("window.luckySevenSnapshot?.state==='MainMenu'",timeout=120000);assert state()['locale']=='zh-Hans';shot('14-reload-chinese')
    report={'browser':browser.version,'pageErrors':errors,'consoleErrors':[m for m in messages if m['type']=='error'],'warnings':[m for m in messages if m['type']=='warning'],'checks':['Chinese default','Chinese help','English help without closing modal','battle language switch preserves hand and selection','hover faces raised with reachable neighbours','drag and click 2+5 break','Chinese attack and counter','Chinese exchange','English preference survives reload','Chinese preference survives reload'],'mouseActions':len(actions)}
    (OUT/'summary.json').write_text(json.dumps(report,indent=2),encoding='utf-8');(OUT/'actions.json').write_text(json.dumps(actions,indent=2),encoding='utf-8');(OUT/'console.json').write_text(json.dumps(messages,indent=2),encoding='utf-8')
    assert not errors and not report['consoleErrors'];assert not any('Unicode' in m['text'] or 'No translation' in m['text'] for m in messages)
    browser.close();print('FINISHED '+json.dumps(report),flush=True)
