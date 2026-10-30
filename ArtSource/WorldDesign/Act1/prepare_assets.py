"""Prepare supplied art for Act 1; never edit the external material library."""
from pathlib import Path
import hashlib, json, shutil
from PIL import Image, ImageOps

project = Path(__file__).resolve().parents[3]
library = project.parent / 'Materiais do jogo/01 Modelos 3D/Cenarios'
destination = project / 'Assets/ForjaDasMares/Art/World/Act1'
sources = Path(__file__).parent / 'Sources'
assets = {
    'RefugeTent': 'Tenda de abrigo azul',
    'CoastalRock': 'Rocha costeira com musgo',
    'LowWall': 'Muro de pedra com musgo',
    'GuardianStatue': 'Estatua do guardiao das mares',
    'RootStump': 'Toco de arvore com raizes',
    'CoalBrazier': 'Braseiro de pedra sem fogo',
    'Gravewood': 'Arvore Velha Com caveiras',
}
jobs = []
for name, folder in assets.items():
    # Meshy paths may exceed the legacy Windows MAX_PATH limit.
    external = Path('\\\\?\\' + str((library / folder).resolve()))
    files = list(external.rglob('*'))
    fbx = next(p for p in files if p.suffix.lower() == '.fbx' and 'Animacao' not in p.name)
    source = sources / name
    target = destination / name
    source.mkdir(parents=True, exist_ok=True)
    target.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(fbx, source / 'Source.fbx')
    textures = {}
    for p in files:
        if p.suffix.lower() != '.png' or p.name.startswith('00'): continue
        kind = next((k for k in ('normal', 'metallic', 'roughness') if k in p.stem.lower()), 'color')
        textures[kind] = p
    for kind, output in [('color', 'BaseColor'), ('normal', 'Normal')]:
        with Image.open(textures[kind]) as image:
            image.convert('RGB').resize((1024, 1024), Image.Resampling.LANCZOS).save(target / (output + '.png'))
    with Image.open(textures['metallic']) as image: metal = image.convert('L').resize((1024, 1024))
    with Image.open(textures['roughness']) as image: rough = image.convert('L').resize((1024, 1024))
    zero = Image.new('L', (1024, 1024))
    Image.merge('RGBA', (metal, zero, zero, ImageOps.invert(rough))).save(target / 'MetalSmooth.png')
    jobs.append({'name': name, 'source': str(source / 'Source.fbx'), 'output': str(target / 'Model.fbx'),
                 'original': str(fbx), 'sha256': hashlib.sha256(fbx.read_bytes()).hexdigest()})
(Path(__file__).parent / 'jobs.json').write_text(json.dumps(jobs, indent=2), encoding='utf8')
print('ACT1_SOURCES_READY', len(jobs))
