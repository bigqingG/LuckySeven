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

OUT = ROOT / 'Docs/Validation/V2/Browser'
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
    started = time.monotonic()
    def state():
        return page.evaluate('window.luckySevenSnapshot')
    def shot(name):
        page.screenshot(path=str(OUT / (name + '.png')))
    def click(x,y,delay=500):
        before=state()
        page.mouse.move(x,y,steps=8)
        page.wait_for_timeout(100)
        page.mouse.click(x,y,delay=90)
        page.wait_for_timeout(delay)
        actions.append({'seconds':round(time.monotonic()-started,2),'click':[x,y],'before':before,'after':state()})
    def card(i):
        n=len(state()['values']);spacing=min(120,1160/n);start=(1280-spacing*n)/2
        click(start+i*spacing+(spacing-10)/2,500,400)
    def combos(s):
        from itertools import combinations
        found=[]
        for k in range(1,len(s['values'])):
            for ids in combinations(range(len(s['values'])),k):
                total=sum(s['values'][i] for i in ids)
                if total not in [7,s['lucky']]:continue
                remain=[i for i in range(len(s['values'])) if i not in ids]
                best=max(s['values'][i]+(2 if s['effects'][i]==0 else 0)+(min(3,s['counter']) if s['effects'][i]==1 else .5) for i in remain)
                # Prefer showing the second lucky number once; then value both damage and retained cards.
                score=best-k*.5+(100 if total==s['lucky'] and s['secondary']==0 else 0)
                found.append((score,ids))
        return max(found,default=(0,()))[1]
    print('RECORDING READY',flush=True)
    shot('00-menu');page.wait_for_timeout(1800)
    click(640,610);shot('01-rules');page.wait_for_timeout(4000)
    click(640,585);click(640,540)
    assert state()['state']=='ChooseLucky'
    seen=set();exchange_done=False;normal_done=False;overflow_done=False
    for step in range(160):
        s=state();phase=s['state'];seen.add(phase)
        print(f"STEP {step} {phase} E{s['stage']+1} T{s['turn']} HP{s['hp']} enemy{s['enemyHp']}",flush=True)
        (OUT/'live.json').write_text(json.dumps(s),encoding='utf-8')
        if phase=='ChooseLucky':
            shot(f"encounter-{s['stage']+1}-choose")
            # Select a candidate supported by the visible opening hand.
            options=[]
            for idx,target in enumerate(s['candidates']):
                test={**s,'lucky':target,'secondary':0};ids=combos(test)
                options.append((sum(s['values'][i] for i in ids)==target,-len(ids),idx))
            idx=max(options)[2];click(515+idx*125,347)
        elif phase=='SupplyChoice':
            if not exchange_done and s['turn']>=2:
                click(770,652);assert state()['state']=='ExchangeSelection'
                choices=sorted(range(len(s['values'])),key=lambda i:s['values'][i],reverse=True)[:2]
                for i in choices:card(i)
                shot('exchange-selected');click(890,652);exchange_done=True
            else:click(465,652)
            shot(f"hand-{s['stage']+1}-{s['turn']}")
        elif phase=='TrimHand':
            count=len(s['values'])-9
            for i in sorted(range(len(s['values'])),key=lambda i:s['values'][i],reverse=True)[:count]:card(i)
            shot('overflow-discard');click(890,652);overflow_done=True
        elif phase=='PlayerInput':
            # Demonstrate ordinary attack first and retain enough cards to exercise the hand limit.
            if not normal_done or (not overflow_done and s['stage']==0 and s['turn']<=3):
                idx=max(range(len(s['values'])),key=lambda i:s['values'][i]+(2 if s['effects'][i]==0 else 0))
                card(idx);shot('normal-attack-selected');click(545,652,1800);normal_done=True
            else:
                ids=combos(s)
                if ids:
                    for i in ids:card(i)
                    shot(f"sum-{s['stage']+1}-{s['turn']}");click(890,652);assert state()['state']=='AttackChoice';shot('armor-broken');page.wait_for_timeout(700)
                else:
                    idx=max(range(len(s['values'])),key=lambda i:s['values'][i]+(2 if s['effects'][i]==0 else 0))
                    card(idx);click(545,652,1800)
        elif phase=='AttackChoice':
            idx=max(range(len(s['values'])),key=lambda i:s['values'][i]+(2 if s['effects'][i]==0 else 0)+(min(3,s['counter']) if s['effects'][i]==1 else .5))
            card(idx);shot('attack-card-selected');click(890,652,1800)
        elif phase=='BetweenBattles':
            shot(f"encounter-{s['stage']+1}-won");page.wait_for_timeout(1600);click(640,520)
        elif phase in ['Victory','Defeat']:
            shot('final-'+phase.lower());page.wait_for_timeout(3500);break
        else:raise AssertionError('Unexpected phase '+phase)
    else:raise AssertionError('Exceeded action limit')
    final=state()
    assert final['state']=='Victory',final
    assert final['breaks']>0 and final['secondary']>0,final
    assert exchange_done and normal_done and overflow_done
    # Verify replay reset in the real browser.
    click(640,520);assert state()['state']=='ChooseLucky' and len(state()['values'])==5 and state()['hp']==30
    shot('replay-reset');page.wait_for_timeout(1500)
    with page.expect_download(timeout=30000) as download:
        page.evaluate('''() => new Promise(resolve => {
            window.__recorder.onstop = () => {
                const blob = new Blob(window.__chunks, {type:'video/webm'});
                const a=document.createElement('a');a.href=URL.createObjectURL(blob);a.download='LuckySeven-V2.webm';a.click();resolve();
            };window.__recorder.stop();
        })''')
    download.value.save_as(str(OUT/'LuckySeven-V2.webm'))
    # Responsive inspection after recording, with the same build.
    for w,h in [(960,540),(1600,900)]:
        page.set_viewport_size({'width':w,'height':h});page.wait_for_timeout(1000);shot(f'responsive-{w}x{h}')
    report={'browser':browser.version,'durationSeconds':time.monotonic()-started,'tracks':tracks,'pageErrors':errors,'consoleErrors':[m for m in messages if m['type']=='error'],'final':final,'phases':sorted(seen),'exchange':exchange_done,'normal':normal_done,'overflow':overflow_done,'replay':True,'method':'Real browser mouse clicks; read-only visible HUD snapshot, no game-state mutation.'}
    (OUT/'actions.json').write_text(json.dumps(actions,indent=2),encoding='utf-8')
    (OUT/'console.json').write_text(json.dumps(messages,indent=2),encoding='utf-8')
    (OUT/'summary.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    assert not errors and not report['consoleErrors'],report
    browser.close();print('FINISHED '+json.dumps(report),flush=True)
