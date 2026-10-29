from pathlib import Path
import zipfile,json,sys
from PIL import Image,ImageOps
import numpy as np
project=Path(__file__).resolve().parents[2]
source=Path(sys.argv[1]) if len(sys.argv)>1 else Path(r'C:/Users/Fabricio_S/Desktop/imagens refenteica/assets gerado por ia/asstes')
staging=Path(__file__).resolve().parent/'Sources';dest=project/'Assets/ForjaDasMares/Art/World/Coast';jobs=[]
for archive in source.glob('*.zip'):
 name=archive.stem.split('_2026')[0].removeprefix('Meshy_AI_');folder=staging/name;folder.mkdir(parents=True,exist_ok=True);target=dest/name;target.mkdir(parents=True,exist_ok=True)
 with zipfile.ZipFile(archive) as z:
  for entry in z.namelist():
   if entry.endswith('.fbx'):(folder/'source.fbx').write_bytes(z.read(entry))
   elif entry.endswith('.png'):
    stem=Path(entry).stem;kind='Normal' if stem.endswith('_normal') else 'Metallic' if stem.endswith('_metallic') else 'Roughness' if stem.endswith('_roughness') else 'BaseColor'
    (folder/(kind+'.png')).write_bytes(z.read(entry))
 for kind in ['BaseColor','Normal']:
  with Image.open(folder/(kind+'.png')) as im:im.resize((1024,1024),Image.Resampling.LANCZOS).save(target/(kind+'.png'))
 with Image.open(folder/'Metallic.png') as im:metal=im.convert('L').resize((1024,1024))
 with Image.open(folder/'Roughness.png') as im:rough=im.convert('L').resize((1024,1024))
 Image.merge('RGBA',(metal,Image.new('L',(1024,1024)),Image.new('L',(1024,1024)),ImageOps.invert(rough))).save(target/'MetalSmooth.png')
 jobs.append({'name':name,'source':str((folder/'source.fbx').resolve()),'output':str((target/'Model.fbx').resolve())})
(Path(__file__).parent/'jobs.json').write_text(json.dumps(jobs,indent=2))
import runpy
runpy.run_path(str(Path(__file__).parent/'create_ground.py'))
