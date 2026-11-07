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

OUT = ROOT / 'Docs/Validation/TableCards/Browser'
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
    page.wait_for_timeout(4500)
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
    def click(x,y,delay=650):
        before=state();page.mouse.move(x,y,steps=8);page.wait_for_timeout(150);page.mouse.click(x,y,delay=90);page.wait_for_timeout(delay)
        actions.append({'seconds':round(time.monotonic()-started,2),'click':[x,y],'before':before,'after':state()})
    def position(index):
        s=state();selected=s['selected'];picked=selected[index]
        group=[i for i,v in enumerate(selected) if v==picked];n=len(group);j=group.index(index)
        if picked:
            spacing=min(112,900/max(1,n));start=(1280-((n-1)*spacing+104))/2;return start+j*spacing+25,395
        spacing=min(74,1000/max(1,n));start=(1280-((n-1)*spacing+120))/2;return start+j*spacing+25,590
    def card(index):click(*position(index))
    def drag(index,x,y,label):
        before=state();start=position(index);page.mouse.move(*start,steps=8);page.wait_for_timeout(200);page.mouse.down();page.wait_for_timeout(180)
        page.mouse.move(x,y,steps=28);page.wait_for_timeout(400);shot(label+'-dragging');page.mouse.up();page.wait_for_timeout(850)
        actions.append({'seconds':round(time.monotonic()-started,2),'drag':[list(start),[x,y]],'before':before,'after':state()});shot(label)
    print('TABLE RECORDING READY',flush=True)
    shot('00-menu');page.wait_for_timeout(1200);click(640,540);assert state()['state']=='ChooseLucky';shot('01-lucky-choice')
    click(515,407);click(465,508);assert state()['state']=='PlayerInput';shot('02-bottom-hand');page.wait_for_timeout(1700)
    card(0);assert state()['selected'][0];shot('03-click-to-table');page.wait_for_timeout(1200)
    card(0);assert not any(state()['selected']);shot('04-click-return');page.wait_for_timeout(1000)
    drag(0,80,300,'05-invalid-drop');assert not any(state()['selected'])
    drag(0,640,410,'06-drag-to-table');assert state()['selected'][0] and sum(state()['selected'])==1
    drag(0,640,610,'07-drag-return');assert not any(state()['selected'])
    drag(0,590,410,'08-play-two');card(1);assert sum(v for v,picked in zip(state()['values'],state()['selected']) if picked)==7
    shot('09-two-plus-five');page.wait_for_timeout(1800);click(890,508);assert state()['state']=='AttackChoice';shot('10-broken')
    card(0);shot('11-attack-staged');page.wait_for_timeout(1000);card(0);assert not any(state()['selected']);shot('12-attack-return')
    drag(0,640,410,'13-attack-drag');click(890,508,1800);assert state()['state']=='SupplyChoice';shot('14-after-attack')
    click(770,508);assert state()['state']=='ExchangeSelection';card(0);card(1);shot('15-exchange-staged');page.wait_for_timeout(1200)
    card(0);assert sum(state()['selected'])==1;card(0);click(890,508);shot('16-exchange-complete');page.wait_for_timeout(1400)
    final=state()
    with page.expect_download(timeout=30000) as download:
        page.evaluate('''() => new Promise(resolve=>{window.__recorder.onstop=()=>{const a=document.createElement('a');a.href=URL.createObjectURL(new Blob(window.__chunks,{type:'video/webm'}));a.download='TableCards.webm';a.click();resolve();};window.__recorder.stop();})''')
    download.value.save_as(str(OUT/'TableCards.webm'))
    for w,h in [(960,540),(1600,900)]:
        page.set_viewport_size({'width':w,'height':h});page.wait_for_timeout(900);shot(f'responsive-{w}x{h}')
    report={'browser':browser.version,'durationSeconds':time.monotonic()-started,'pageErrors':errors,'consoleErrors':[m for m in messages if m['type']=='error'],'final':final,'checks':['click stage','click return','invalid drop snaps back','drag stage without duplicate click','drag return','2+5 break','attack card return and drag','exchange staging and return'],'method':'Real browser mouse clicks and pointer dragging; read-only visible HUD assertions.'}
    (OUT/'summary.json').write_text(json.dumps(report,indent=2),encoding='utf-8');(OUT/'actions.json').write_text(json.dumps(actions,indent=2),encoding='utf-8');(OUT/'console.json').write_text(json.dumps(messages,indent=2),encoding='utf-8')
    assert not errors and not report['consoleErrors'];browser.close();print('FINISHED '+json.dumps(report),flush=True)
