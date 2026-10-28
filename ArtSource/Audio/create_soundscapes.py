"""Original deterministic instrumental scores and environmental loops. NumPy only."""
import json, math, wave
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ForjaDasMares/Audio/Soundscapes';OUT.mkdir(parents=True,exist_ok=True)
SR=32000;RNG=np.random.default_rng(20261027);report={}
def hz(note):return 440*2**((note-69)/12)
def colored(seconds,low=20,high=9000,pink=.7):
    n=int(seconds*SR);s=np.fft.rfft(RNG.normal(size=n));f=np.fft.rfftfreq(n,1/SR)
    s*=((f/(low+.001))**2/(1+(f/(low+.001))**2))*np.exp(-(f/high)**4)/np.maximum(f,20)**pink
    y=np.fft.irfft(s,n);return y/(np.std(y)+1e-8)
def note(midi,seconds,kind):
    t=np.arange(int(seconds*SR))/SR;f=hz(midi);phase=2*np.pi*f*t
    if kind=='lute':
        y=sum(np.sin(phase*k+k*.19)*np.exp(-t*(1.6+k*.9))/(k**1.15) for k in range(1,10))
        y+=RNG.normal(size=len(t))*.055*np.exp(-t*85);env=np.minimum(1,t/.004)
    elif kind=='flute':
        phase+=.045*np.sin(2*np.pi*5.1*t)*np.minimum(1,t/.25)
        y=np.sin(phase)+.18*np.sin(phase*2)+.045*np.sin(phase*3)+RNG.normal(size=len(t))*.022
        env=np.minimum(1,t/.12)*np.minimum(1,(seconds-t)/.18)
    else:
        y=sum(np.sin(phase*k+.009*np.sin(2*np.pi*(3+k*.12)*t))/(k*k) for k in [1,2,3,4,5])
        env=np.minimum(1,t/.35)*np.minimum(1,(seconds-t)/.55)
    return (y*env).astype(np.float32)
def add(mix,sound,at,gain,pan=0):
    offset=int(at*SR)%len(mix);p=np.array([math.sqrt((1-pan)/2),math.sqrt((1+pan)/2)],np.float32)
    data=sound[:,None]*p[None,:]*gain
    count=min(len(data),len(mix)-offset);mix[offset:offset+count]+=data[:count]
    if count<len(data):mix[:len(data)-count]+=data[count:]
def room(mix):
    dry=mix.copy()
    for delay,gain in [(.173,.10),(.291,.075),(.431,.055),(.673,.038),(.991,.022)]:mix+=np.roll(dry,int(delay*SR),axis=0)[:,::-1]*gain
def save(name,mix,loop=True,target=.10):
    mix-=mix.mean(axis=0);mix*=target/(np.sqrt(np.mean(mix*mix))+1e-9)
    mix=np.tanh(mix*.9)/.9
    if loop:
        # Continuity at the loop boundary, including environmental/reverb tails.
        width=int(SR*.035);a=np.linspace(0,1,width)[:,None]
        mix[-width:]=mix[-width:]*(1-a)+mix[:1]*a
    else:
        count=min(int(.15*SR),len(mix)//4);mix[-count:]*=np.linspace(1,0,count)[:,None]
    peak=float(np.max(np.abs(mix)))
    if peak>.88:mix*=.88/peak
    pcm=(np.clip(mix,-.99,.99)*32767).astype('<i2')
    with wave.open(str(OUT/(name+'.wav')),'wb') as w:w.setparams((2,2,SR,len(mix),'NONE','not compressed'));w.writeframes(pcm.tobytes())
    report[name]={'seconds':round(len(mix)/SR,3),'peak':float(np.max(np.abs(mix))),'rms':float(np.sqrt(np.mean(mix*mix))),'seam':float(np.max(np.abs(mix[-1]-mix[0]))),'stereo':True,'loop':loop}
    print('AUDIO_READY',name,report[name],flush=True)
def score(name,bpm,root,style):
    beat=60/bpm;length=64*beat;mix=np.zeros((round(length*SR),2),np.float32)
    chords=[(0,3,7),(-2,2,5),(3,7,10),(-5,-1,2)]
    motif=[(0,0),(1.5,3),(2.5,7),(4,10),(5.5,7),(7,5),(8,3),(10,2),(12,0),(13.5,7),(15,5)]
    for bar in range(16):
        chord=chords[(bar//2)%4];base=root+chord[0]
        for index,pitch in enumerate(chord):add(mix,note(root+pitch+12,4.3*beat,'pad'),bar*4*beat,.055,-.5+index*.5)
        add(mix,note(base-12,3.8*beat,'pad'),bar*4*beat,.065,0)
        steps=8 if style in ['forest','boss'] else 4
        for step in range(steps):
            pitch=root+12+chord[(step+bar)%3]+(12 if step%4==3 else 0)
            add(mix,note(pitch,1.7,'lute'),(bar*4+step*4/steps)*beat,.10 if style=='menu' else .07,(-.55 if step%2 else .55))
        if style!='catacomb' and bar%4 in [0,2,3] or style=='catacomb' and bar in [3,7,11,15]:
            phrase=bar//4
            for at,interval in motif[:4] if bar%4!=3 else motif[7:]:
                at=at%4;pitch=root+24+interval+(12 if style=='forest' and phrase%2 else 0)
                add(mix,note(pitch,beat*(.95 if style=='boss' else 1.3),'flute'),(bar*4+at)*beat,.065 if style!='boss' else .055,-.16)
        if style in ['volcano','boss','forest']:
            for step in ([0,1.5,2,3] if style=='boss' else [0,2]):
                t=np.arange(int(.45*SR))/SR
                drum=np.sin(2*np.pi*(62*t-34*t*t))*np.exp(-t*12)+RNG.normal(size=len(t))*.15*np.exp(-t*45)
                add(mix,drum,(bar*4+step)*beat,.11 if style=='boss' else .065,-.15 if step==0 else .2)
            if style=='forest':
                for step in [1,3]:
                    t=np.arange(int(.09*SR))/SR;wood=(np.sin(t*2*np.pi*820)+.3*np.sin(t*2*np.pi*1330))*np.exp(-t*65)
                    add(mix,wood,(bar*4+step)*beat,.025,.6)
    room(mix);save(name,mix,target=.105)
for args in [('Menu_Mares',80,50,'menu'),('Catacumbas_Ecos',84,45,'catacomb'),('Floresta_Raizes',92,50,'forest'),('Fortaleza_Brasas',100,43,'volcano'),('Chefe_Ferro',112,45,'boss')]:score(*args)
def ambience(name,kind):
    length=40;n=length*SR;t=np.arange(n)/SR;mix=np.zeros((n,2),np.float32)
    low=colored(length,18,240,.85);air=colored(length,200,3800,.4)
    for side in range(2):mix[:,side]=np.roll(low,side*8000)*(.10 if kind=='volcano' else .045)+np.roll(air,side*700)*(.013+.007*np.sin(t*.17+side)**2)
    for when in np.arange(1,length,2.3 if kind=='catacomb' else .47 if kind in ['home','volcano'] else 1.3):
        when+=float(RNG.uniform(-.15,.15));seconds=.28 if kind=='catacomb' else .16 if kind=='home' else .45
        tt=np.arange(int(seconds*SR))/SR
        if kind=='catacomb':
            f=RNG.uniform(430,820);y=np.sin(2*np.pi*(f*tt+95*tt*tt))*np.exp(-tt*24)*np.minimum(1,tt/.002)
            for delay,gain in [(0,.06),(.17,.025),(.35,.015)]:add(mix,y,when+delay,gain,float(RNG.uniform(-.7,.7)))
        elif kind=='forest':
            y=RNG.normal(size=len(tt))*np.sin(np.pi*np.arange(len(tt))/len(tt))**2
            add(mix,y,when,.018,float(RNG.uniform(-.8,.8)))
        elif kind=='volcano':
            y=(np.sin(2*np.pi*(95*tt-65*tt*tt))+RNG.normal(size=len(tt))*.10)*np.exp(-tt*11)*np.minimum(1,tt/.01)
            add(mix,y,when,.04,float(RNG.uniform(-.8,.8)))
        else:
            y=RNG.normal(size=len(tt))*np.exp(-tt*70)*np.minimum(1,tt/.001)
            add(mix,y,when,.028,float(RNG.uniform(-.6,.6)))
    if kind=='forest':
        water=colored(length,500,6500,.25);mix[:,0]+=water*.013;mix[:,1]+=np.roll(water,1200)*.013
    room(mix);save(name,mix,target=.065)
for args in [('Amb_Porto','home'),('Amb_Catacumbas','catacomb'),('Amb_Floresta','forest'),('Amb_Fortaleza','volcano')]:ambience(*args)
for name,pitches in [('Vitoria',[62,65,69,74]),('Derrota',[62,60,57,50]),('Partida',[50,57,62])]:
    mix=np.zeros((int(3*SR),2),np.float32)
    for i,pitch in enumerate(pitches):add(mix,note(pitch,1.7,'lute'),i*.25,.35,(i-1.5)*.2)
    room(mix);save(name,mix,False,.11)
(Path(__file__).parent/'soundscape_report.json').write_text(json.dumps({'sampleRate':SR,'seed':20261027,'credits':'Original procedural composition and synthesis for Forja das Mares; no external samples.','assets':report},indent=2))
