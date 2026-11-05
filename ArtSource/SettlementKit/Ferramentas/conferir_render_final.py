import bpy,json,os
from mathutils import Vector
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(root/'Kit_original_cidade_e_vegetacao.blend'))
scene=bpy.context.scene
targets=[r['asset'] for r in json.loads((root/'Documentacao'/'metricas.json').read_text(encoding='utf8'))]
if os.environ.get('ASSET_FILTER'):targets=[n for n in targets if n in os.environ['ASSET_FILTER'].split(',')]
scene.cycles.samples=8
for o in scene.objects:
 if o.type=='MESH':o.hide_render=o.name!='Piso apenas da previa'
for name in targets:
 o=bpy.data.objects[name+'_LOD0'];loc=o.location.copy();o.location=(0,0,0);o.hide_render=False
 dim=o.dimensions;sz=max(dim);target=Vector((0,0,dim.z*.43));cam=scene.camera
 cam.location=target+Vector((1.15,-1.7,1.05))*sz;cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=sz*1.65
 scene.render.filepath=str(root/'Previews'/f'{name}.png');bpy.ops.render.render(write_still=True)
 o.hide_render=True;o.location=loc
print('PREVIAS_FINAIS',len(targets),flush=True)
