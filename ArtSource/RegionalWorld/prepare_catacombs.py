"""Prepare only the two new supplied static props and the supplied quadruped rig.
Run with bundled Python. Originals remain outside the Unity working art directory.
"""
from pathlib import Path
import zipfile,json,struct
from io import BytesIO
from PIL import Image,ImageOps
root=Path(__file__).resolve().parents[2]
src=Path(r'C:/Users/Fabricio_S/Desktop/imagens refenteica/assets gerado por ia/asstes')
out=root/'Assets/ForjaDasMares/Art/World/Catacombs';out.mkdir(parents=True,exist_ok=True)
staging=Path(__file__).parent/'Sources';jobs=[]
for name in ['Ancient_Tidegate','Ancient_Wave_Urn']:
 archive=next(src.glob('Meshy_AI_'+name+'_*.zip'));folder=staging/name;folder.mkdir(parents=True,exist_ok=True);dest=out/name;dest.mkdir(parents=True,exist_ok=True)
 with zipfile.ZipFile(archive) as z:
  for entry in z.namelist():
   if entry.endswith('.fbx'):(folder/'source.fbx').write_bytes(z.read(entry))
   elif entry.endswith('.png'):
    stem=Path(entry).stem;kind='Normal' if stem.endswith('_normal') else 'Metallic' if stem.endswith('_metallic') else 'Roughness' if stem.endswith('_roughness') else 'BaseColor'
    (folder/(kind+'.png')).write_bytes(z.read(entry))
 for kind in ['BaseColor','Normal']:
  with Image.open(folder/(kind+'.png')) as im:im.resize((1024,1024),Image.Resampling.LANCZOS).save(dest/(kind+'.png'))
 with Image.open(folder/'Metallic.png') as im:metal=im.convert('L').resize((1024,1024))
 with Image.open(folder/'Roughness.png') as im:rough=im.convert('L').resize((1024,1024))
 Image.merge('RGBA',(metal,Image.new('L',(1024,1024)),Image.new('L',(1024,1024)),ImageOps.invert(rough))).save(dest/'MetalSmooth.png')
 jobs.append({'name':name,'source':str(folder/'source.fbx'),'output':str(dest/'Model.fbx')})
(staging/'catacomb_jobs.json').write_text(json.dumps(jobs,indent=2))
creature=root/'Assets/ForjaDasMares/Art/Enemies/TideCrawler';creature.mkdir(parents=True,exist_ok=True)
b=(src/'Meshy_AI_Character_output.glb').read_bytes();n=struct.unpack_from('<I',b,12)[0];j=json.loads(b[20:20+n]);start=28+n
mat=j['materials'][0];pbr=mat['pbrMetallicRoughness']
def texture(index):
 image=j['images'][j['textures'][index]['source']];view=j['bufferViews'][image['bufferView']];offset=start+view.get('byteOffset',0)
 return Image.open(BytesIO(b[offset:offset+view['byteLength']])).resize((1024,1024),Image.Resampling.LANCZOS)
texture(pbr['baseColorTexture']['index']).convert('RGB').save(creature/'BaseColor.png')
texture(mat['normalTexture']['index']).convert('RGB').save(creature/'Normal.png')
packed=texture(pbr['metallicRoughnessTexture']['index']).convert('RGB');r,g,metal=packed.split()
Image.merge('RGBA',(metal,Image.new('L',metal.size),Image.new('L',metal.size),ImageOps.invert(g))).save(creature/'MetalSmooth.png')
floor=Path(r'C:/Users/Fabricio_S/Desktop/Jogos/Referencias Meshy/Catacumbas Kit 01/06_piso_TEXTURA.png')
with Image.open(floor) as im:im.resize((1024,1024),Image.Resampling.LANCZOS).save(out/'FloorColor.png')
print('CATACOMB_TEXTURES_READY')
