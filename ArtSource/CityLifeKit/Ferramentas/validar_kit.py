import bpy,json,math
from pathlib import Path
root=Path(__file__).resolve().parents[1]
report=[]
for path in sorted((root/'Modelos').glob('*.fbx')):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=str(path))
 objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
 errors=[]
 if len(objects)!=1:errors.append('Esperado exatamente um mesh')
 for o in objects:
  if not o.data.polygons:errors.append('Mesh vazio')
  if len(o.data.materials)!=1:errors.append('Material nao compartilhado')
  if not o.data.uv_layers:errors.append('UV ausente')
  if any(not math.isfinite(c) for v in o.data.vertices for c in v.co):errors.append('Coordenada invalida')
  if max(o.dimensions)>15:errors.append('Dimensao inesperada')
  material=o.data.materials[0]
  images=[n.image for n in material.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
  if not images:errors.append('Textura nao encontrada na reimportacao')
  for image in images:
   if not image.has_data:errors.append('Imagem sem pixels: '+image.name)
 report.append({'arquivo':path.name,'ok':not errors,'erros':errors,'meshes':len(objects)})
(root/'Documentacao'/'validacao_fbx.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
print('VALIDACAO_FBX',len(report),'PASS',sum(r['ok'] for r in report),'FAIL',sum(not r['ok'] for r in report),flush=True)
if len(report)!=30 or any(not r['ok'] for r in report):raise RuntimeError('Falha na validacao do kit')
