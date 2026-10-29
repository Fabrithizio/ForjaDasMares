from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import numpy as np
out=Path(__file__).resolve().parents[2]/'Assets/ForjaDasMares/Art/World/Coast';rng=np.random.default_rng(4711)
noise=Image.fromarray(rng.integers(0,255,(16,16),dtype=np.uint8)).resize((512,512),Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(6))
a=np.array(noise).astype(float)/255;rgb=np.empty((512,512,3),dtype=np.uint8)
for c,base in enumerate((57,79,53)):rgb[:,:,c]=np.clip(base+(a-.5)*26,0,255)
Image.fromarray(rgb).save(out/'Grass.png')
im=Image.new('RGB',(512,512),(49,52,42));draw=ImageDraw.Draw(im)
for row,y in enumerate(range(-64,576,54)):
 x=-65 if row%2 else -20
 while x<512:
  w=int(rng.integers(46,78));shade=int(rng.integers(-8,9));color=(102+shade,99+shade,80+shade)
  draw.rounded_rectangle((x+2,y+2,x+w-2,y+51),radius=6,fill=color,outline=(122+shade,119+shade,94+shade),width=2)
  if rng.random()<.2:draw.line((x+5,y+38,x+w-8,y+38),fill=(63,80,49),width=3)
  x+=w
im.filter(ImageFilter.GaussianBlur(.5)).save(out/'Soil.png')
rock=np.empty((256,256,3),dtype=np.uint8)
for c,b in enumerate((87,95,92)):rock[:,:,c]=np.clip(b+rng.normal(0,3,(256,256)),0,255)
Image.fromarray(rock).save(out/'Rock.png')
