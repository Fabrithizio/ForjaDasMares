"""Small original synthesized placeholders. No samples, music or external recordings."""
import math,random,wave,struct
from pathlib import Path
out=Path(__file__).resolve().parents[2]/'Assets/ForjaDasMares/Art/ExpeditionSet'
rate=22050
for kind,duration in [('Hit',.12),('Crate',.25),('Heal',.36),('Loot',.28)]:
 rng=random.Random(810);samples=[]
 for i in range(int(duration*rate)):
  t=i/rate;x=t/duration;envelope=math.sin(math.pi*min(1,x*15))*math.exp(-x*7)
  if kind in ('Hit','Crate'):value=(rng.uniform(-1,1)*.45+math.sin(2*math.pi*(100 if kind=='Hit' else 65)*t)*.55)*envelope
  else:
   f=(660 if kind=='Heal' else 880)*(1 if x<.4 else 1.25 if x<.7 else 1.5)
   value=(math.sin(2*math.pi*f*t)+.2*math.sin(4*math.pi*f*t))*envelope*.4
  samples.append(struct.pack('<h',int(max(-1,min(1,value))*.5*32767)))
 with wave.open(str(out/('Sfx_'+kind+'.wav')),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(b''.join(samples))
print('Created 4 original synthesized feedback sounds.')
