"""Real WebGL mouse acceptance for the pixel skin. Reads public HUD only; no state mutation."""
import itertools
import json
import pathlib
import sys
import time

ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Library/browser-tools"))
from playwright.sync_api import sync_playwright

OUT = ROOT / "Docs/Validation/PixelAdventure/Browser"
OUT.mkdir(parents=True, exist_ok=True)
messages, errors, actions = [], [], []
with sync_playwright() as p:
    browser = p.chromium.launch(executable_path=r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        headless=True, args=["--enable-unsafe-swiftshader", "--autoplay-policy=no-user-gesture-required"])
    page = browser.new_page(viewport={"width": 1280, "height": 720}, device_scale_factor=1)
    page.on("console", lambda m: messages.append({"type": m.type, "text": m.text}))
    page.on("pageerror", lambda e: errors.append(str(e)))
    page.goto("http://127.0.0.1:8078/", wait_until="domcontentloaded")
    page.wait_for_function("document.querySelector('#loading').style.display==='none'", timeout=120000)
    page.wait_for_function("window.luckySevenSnapshot?.state==='MainMenu'", timeout=120000)
    page.wait_for_timeout(1000)
    def state(): return page.evaluate("window.luckySevenSnapshot")
    def shot(name): page.screenshot(path=str(OUT / (name + ".png")))
    def click(x, y, delay=450):
        before = state()
        page.mouse.move(x,y,steps=5)
        page.wait_for_timeout(120)
        page.mouse.click(x,y,delay=70)
        page.wait_for_timeout(delay)
        actions.append({"click": [x,y], "before": before, "after": state()})
    def pos(index):
        s=state();picked=s["selected"][index]
        group=[i for i,v in enumerate(s["selected"]) if v==picked]
        n=len(group);j=group.index(index)
        spacing=min(112,900/max(1,n)) if picked else min(82,676/max(1,n-1))
        width=104 if picked else 100
        return (1280-((n-1)*spacing+width))/2+j*spacing+22, 395 if picked else 608
    def card(i):click(*pos(i))
    def drag(i,x,y):
        before=state();page.mouse.move(*pos(i),steps=8);page.wait_for_timeout(350)
        page.mouse.down();page.mouse.move(x,y,steps=24);page.wait_for_timeout(120);page.mouse.up();page.wait_for_timeout(400)
        actions.append({"drag": [x,y], "before":before, "after":state()})
    def language(code):
        click(970,38,700)
        page.wait_for_function("code=>window.luckySevenSnapshot.locale===code",arg=code)
    def combo(s):
        n=len(s["values"])
        for size in range(1,n):
            for subset in itertools.combinations(range(n),size):
                if sum(s["values"][i] for i in subset) in (7,s["lucky"]):return subset
        return ()
    try:
        assert state()["locale"]=="zh-Hans"
        shot("00-menu")
        click(640,612);shot("01-help-chinese")
        language("en");shot("02-help-english")
        language("zh-Hans");click(640,585);click(640,540)
        shot("03-choose-lucky")
        click(515,407);shot("04-supply")
        click(118,630);shot("05-hand")
        before=state();click(118,630);assert state()["values"]==before["values"],"Disabled Draw changed the hand"
        page.mouse.move(*pos(2),steps=8);page.wait_for_timeout(500);shot("06-hover")
        x,_=pos(2);click(x,548);assert state()["selected"][2],"Raised face click failed"
        card(2);assert not any(state()["selected"])
        page.mouse.move(*pos(0),steps=8);page.wait_for_timeout(350);card(1)
        assert state()["selected"][1] and not state()["selected"][0],"Neighbour strip blocked"
        card(1)
        drag(0,25,300);assert not any(state()["selected"])
        drag(0,620,410);assert state()["selected"][0]
        shot("07-staged")
        before=state();language("en")
        assert state()["values"]==before["values"] and state()["selected"]==before["selected"]
        shot("08-battle-english");language("zh-Hans")
        drag(0,620,620);assert not any(state()["selected"])
        page.set_viewport_size({"width":960,"height":540});page.wait_for_timeout(500);shot("09-small-window")
        page.set_viewport_size({"width":1280,"height":720});page.wait_for_timeout(500)
        shown_break=False;shown_hit=False;shown_exchange=False;seen_stages=set()
        for step in range(90):
            s=state();seen_stages.add(s["stage"])
            if s["state"]=="ChooseLucky":click(515,407)
            elif s["state"]=="SupplyChoice":
                if not shown_exchange and len(s["values"])>=2:
                    click(118,670);card(0);card(1);shot("12-exchange")
                    click(890,508);shown_exchange=True
                else:click(118,630)
            elif s["state"]=="TrimHand":
                for i in range(len(s["values"])-9):card(i)
                shot("13-trim");click(890,508)
            elif s["state"]=="PlayerInput":
                chosen=combo(s)
                if chosen:
                    for i in chosen:card(i)
                    if not shown_break:shot("10-lucky-sum")
                    old=state();click(890,508,90)
                    assert state()["state"]=="AttackChoice" and state()["enemyHp"]==old["enemyHp"]
                    if not shown_break:shot("11-break");shown_break=True
                else:
                    i=max(range(len(s["values"])),key=lambda i:s["values"][i]+(2 if s["effects"][i]==0 else 0))
                    card(i);click(545,508,1600)
            elif s["state"]=="AttackChoice":
                i=max(range(len(s["values"])),key=lambda i:s["values"][i]+(2 if s["effects"][i]==0 else 0))
                expected=s["values"][i]+(2 if s["effects"][i]==0 else 0)
                card(i);old=state();click(890,508,110)
                assert state()["enemyHp"]==max(0,old["enemyHp"]-expected)
                if not shown_hit:shot("14-hit");shown_hit=True
                page.wait_for_timeout(1500)
            elif s["state"]=="BetweenBattles":
                shot("15-between-"+str(s["stage"]));click(640,520)
            elif s["state"] in ("Victory","Defeat"):
                shot("16-result");outcome=s["state"];click(640,520)
                assert state()["state"]=="ChooseLucky";shot("17-replay");break
            else:page.wait_for_timeout(1700)
        else:raise AssertionError("Browser flow did not finish")
        assert shown_break and shown_hit and shown_exchange
        assert not errors,errors
        result={"passed":True,"outcome":outcome,"stages":sorted(seen_stages),"break":shown_break,"hit":shown_hit,
            "exchange":shown_exchange,"locale_switch":True,"hover_neighbour_drag":True,"viewport":[1280,720,960,540],
            "page_errors":errors,"actions":len(actions)}
        (OUT/"summary.json").write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding="utf8")
        print(json.dumps(result,ensure_ascii=False),flush=True)
    except Exception as exc:
        shot("failure");(OUT/"summary.json").write_text(json.dumps({"passed":False,"error":str(exc),"state":state()},ensure_ascii=False,indent=2),encoding="utf8");raise
    finally:
        (OUT/"console.json").write_text(json.dumps({"messages":messages,"page_errors":errors},ensure_ascii=False,indent=2),encoding="utf8")
        (OUT/"actions.json").write_text(json.dumps(actions,ensure_ascii=False,indent=2),encoding="utf8")
        browser.close()

