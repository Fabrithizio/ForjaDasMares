from pathlib import Path
import wave,math,random,struct
root=Path(__file__).resolve().parents[3]/'Assets/ForjaDasMares/Art/Characters/Prisma/Audio'
root.mkdir(parents=True,exist_ok=True)
for name,duration,start,end in [('Cast',.23,420,970),('Impact',.13,310,150),('Heavy',.58,240,1120)]:
 rng=random.Random(17);rate=22050;samples=[];phase=0
 for i in range(int(duration*rate)):
  t=i/rate;u=t/duration;phase+=math.tau*(start+(end-start)*u)/rate
  envelope=min(1,t/.012)*(1-u)**2
  tone=math.sin(phase)+.25*math.sin(phase*1.5)+.12*(rng.random()*2-1)
  samples.append(struct.pack('<h',int(max(-1,min(1,tone*envelope*.32))*32767)))
 with wave.open(str(root/('Prisma_'+name+'.wav')),'wb') as f:
  f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(b''.join(samples))
