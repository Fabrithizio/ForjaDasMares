"""Original authored in-place quadruped clips on the supplied SmartRig skeleton.
No runtime bone oscillation; keys baked in FBX. Source GLB is never modified.
"""
import bpy,math,json
from pathlib import Path
from mathutils import Vector,Euler,Quaternion
root=Path(__file__).resolve().parents[2]
out=root/'Assets/ForjaDasMares/Art/Enemies/TideCrawler';out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(root/'ArtSource/Characters/TideCrawler/Source.glb'))
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith('Mesh'))
for o in list(bpy.context.scene.objects):
 if o not in [rig,mesh]:bpy.data.objects.remove(o,do_unlink=True)
bones={b.name:b for b in rig.pose.bones};rest={n:b.bone.matrix_local.to_quaternion() for n,b in bones.items()}
for b in bones.values():b.rotation_mode='QUATERNION'
def rot(n,x=0,y=0,z=0):bones[n].rotation_quaternion=rest[n].conjugated()@Euler((x,y,z),'XYZ').to_quaternion()@rest[n]
def move(n,v):bones[n].location=rest[n].conjugated()@Vector(v)
legs=[('Bone_008','Bone_007','Bone_006',1,0),('Bone_011','Bone_010','Bone_009',-1,.5),('Bone_014','Bone_013','Bone_012',1,.5),('Bone_017','Bone_016','Bone_015',-1,0)]
lengths={'Idle':61,'Walk':31,'Run':25,'Attack':31,'Hit':13,'Dead':31}
rig.animation_data_create();actions={}
for name,length in lengths.items():
 action=bpy.data.actions.new(name);action.use_fake_user=True;actions[name]=action
 for frame in range(1,length+1):
  rig.animation_data.action=None;t=(frame-1)/(length-1)
  for bone in bones.values():bone.location=(0,0,0);bone.rotation_quaternion=Quaternion()
  breathing=math.sin(t*math.tau)*.012;move('Bone_000',(0,0,breathing));rot('Bone_001',breathing*.4)
  if name in ['Walk','Run']:
   amp=.19 if name=='Walk' else .30
   move('Bone_000',(.012*math.sin(t*math.tau),0,abs(math.sin(t*math.tau))*.025-.018))
   rot('Bone_001',.025*math.sin(t*math.tau),0,.018*math.sin(t*math.tau))
   for upper,lower,foot,side,phase in legs:
    wave=math.sin((t+phase)*math.tau);lift=max(0,wave)
    rot(upper,amp*wave,.09*side*wave,side*.045*wave);rot(lower,-.32*lift,side*.08*lift);rot(foot,.16*lift)
  if name=='Attack':
   prep=math.sin(math.pi*min(1,t/.45)) if t<.45 else 0;hit=math.sin(math.pi*max(0,min(1,(t-.38)/.62)))
   move('Bone_000',(0,.07*prep-.18*hit,-.045*prep+.035*hit));rot('Bone_000',-.12*prep+.24*hit);rot('Bone_001',-.15*prep+.26*hit)
   for upper,lower,foot,side,phase in legs:rot(upper,-.10*prep+.16*hit,side*.05*hit);rot(lower,-.14*prep)
  if name=='Hit':
   p=math.sin(math.pi*t);move('Bone_000',(0,.07*p,-.04*p));rot('Bone_001',-.18*p,0,.08*p)
  if name=='Dead':
   p=min(1,t/.8);p=p*p*(3-2*p);move('Bone_000',(0,0,-.35*p));rot('Bone_000',0,.75*p,.12*p)
   for upper,lower,foot,side,phase in legs:rot(upper,0,side*.35*p);rot(lower,-.55*p)
  values={n:(b.rotation_quaternion.copy(),b.location.copy()) for n,b in bones.items()};rig.animation_data.action=action
  for n,b in bones.items():
   b.rotation_quaternion,b.location=values[n];b.keyframe_insert('rotation_quaternion',frame=frame,group=n);b.keyframe_insert('location',frame=frame,group=n)
# Decimation retains UVs, skin weights and a common 18-bone skeleton.
rig.animation_data.action=None
for bone in bones.values():bone.location=(0,0,0);bone.rotation_quaternion=Quaternion()
bpy.context.view_layer.update()
bpy.context.view_layer.objects.active=mesh
original=sum(len(p.vertices)-2 for p in mesh.data.polygons)
mod=mesh.modifiers.new('MobileBase','DECIMATE');mod.ratio=.5;bpy.ops.object.modifier_apply(modifier=mod.name);mesh.name='LOD0';mesh.data.materials.clear();meshes=[mesh]
for index,ratio in [(1,.5),(2,.25)]:
 dup=mesh.copy();dup.data=mesh.data.copy();bpy.context.collection.objects.link(dup);dup.name='LOD'+str(index);bpy.context.view_layer.objects.active=dup
 mod=dup.modifiers.new('LODReduce','DECIMATE');mod.ratio=ratio;bpy.ops.object.modifier_apply(modifier=mod.name);meshes.append(dup)
rig.animation_data.action=actions['Idle'];bpy.context.scene.render.fps=30;bpy.context.scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
for o in meshes:o.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(out/'CrawlerAnimated.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,armature_nodetype='NULL',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',path_mode='STRIP')
report={'sourceTriangles':original,'lodTriangles':[sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes],'bones':len(bones),'clips':lengths,'fps':30,'rootMotion':False,'source':'Supplied four-legged creature; original keyed clips, not motion capture.'}
(Path(__file__).parent/'crawler_report.json').write_text(json.dumps(report,indent=2));print('CRAWLER_READY',report)
