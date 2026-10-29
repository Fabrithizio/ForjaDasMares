import bpy,json
from pathlib import Path
from mathutils import Vector
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=r'C:/Users/Fabricio_S/Desktop/imagens refenteica/assets gerado por ia/asstes/Meshy_AI_Character_output.glb')
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
print('RIG_MATRIX',rig.matrix_world)
print('BONES',json.dumps([{'name':b.name,'parent':b.parent.name if b.parent else None,'head':list(b.head_local),'tail':list(b.tail_local)} for b in rig.data.bones]))
for o in bpy.context.scene.objects:
 if o.type=='MESH': print('MESH',o.name,len(o.data.vertices),sum(len(p.vertices)-2 for p in o.data.polygons),list(o.dimensions),o.matrix_world)
