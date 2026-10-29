import bpy,json,sys,math
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parent
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
jobs=json.loads((Path(args[0]) if args else root/'jobs.json').read_text());report=[]
for job in jobs:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=job['source'])
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 bounds=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
 lo=Vector([min(v[i] for v in bounds) for i in range(3)]);hi=Vector([max(v[i] for v in bounds) for i in range(3)])
 center=Vector(((lo.x+hi.x)*.5,(lo.y+hi.y)*.5,lo.z));factor=8/max(hi.x-lo.x,hi.y-lo.y,.01)
 # FBX imported into Blender uses Z-up. Normalize to ground pivot and eight metre footprint.
 bpy.ops.object.select_all(action='DESELECT')
 for o in meshes:
  o.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join();base=bpy.context.object
 bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 for v in base.data.vertices:v.co=(base.matrix_world@v.co-center)*factor
 base.location=(0,0,0);base.rotation_euler=(0,0,0);base.scale=(1,1,1)
 base.data.materials.clear();base.name='LOD0'
 objs=[base];counts=[]
 for index,ratio in [(1,.5),(2,.22)]:
  dup=base.copy();dup.data=base.data.copy();bpy.context.collection.objects.link(dup);dup.name='LOD'+str(index)
  bpy.context.view_layer.objects.active=dup;mod=dup.modifiers.new('Reduced','DECIMATE');mod.ratio=ratio;bpy.ops.object.modifier_apply(modifier=mod.name);objs.append(dup)
 for o in objs:counts.append(sum(len(p.vertices)-2 for p in o.data.polygons))
 bpy.ops.object.select_all(action='DESELECT')
 for o in objs:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=job['output'],use_selection=True,object_types={'MESH'},add_leaf_bones=False,bake_anim=False,use_mesh_modifiers=True,path_mode='STRIP',embed_textures=False,axis_forward='-Z',axis_up='Y')
 report.append({'name':job['name'],'triangles':counts,'footprint':8,'sourceUntouched':True})
(root/('catacomb_lod_report.json' if args else 'lod_report.json')).write_text(json.dumps(report,indent=2))
print('REGION_ASSETS_READY',len(report))
