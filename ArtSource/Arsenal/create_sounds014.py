from pathlib import Path
import wave,math,random,struct
r=Path(__file__).resolve().parents[2];a=r/'Assets/ForjaDasMares/Audio/Combat014';a.mkdir(parents=True,exist_ok=True)
rng=random.Random(140)
for name,seconds in [('Impact',.16),('Swing',.22),('Heavy',.3),('Step',.095),('Reward',.55),('Perfect',.32)]:
 samples=[];n=int(22050*seconds);last=0
 for i in range(n):
  t=i/22050;x=i/n;noise=rng.uniform(-1,1);last=.6*last+.4*noise;env=(1-x)**3*min(1,x*80)
  if name=='Impact':v=(math.sin(2*math.pi*(155*t-140*t*t))*.65+last*.7)*env
  elif name=='Heavy':v=(math.sin(2*math.pi*(95*t-55*t*t))*.8+last*.55)*env
  elif name=='Swing':v=last*math.sin(math.pi*x)**2*.7
  elif name=='Step':v=(last+math.sin(t*650)*.3)*env*.35
  else:
   f=[523.25,659.25,783.99][min(2,int(x*3))] if name=='Reward' else 880+660*x
   v=(math.sin(2*math.pi*f*t)+.25*math.sin(4*math.pi*f*t))*env*.45
  samples.append(struct.pack('<h',int(max(-1,min(1,v))*.7*32767)))
 with wave.open(str(a/(name+'.wav')),'wb') as w:w.setparams((1,2,22050,n,'NONE','not compressed'));w.writeframes(b''.join(samples))
