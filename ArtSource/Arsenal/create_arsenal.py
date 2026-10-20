"""Original arsenal + additional clips. Keeps existing Brasa mesh and locomotion actions."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector,Euler,Quaternion
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ForjaDasMares/Art/Arsenal';OUT.mkdir(parents=True,exist_ok=True)
SOURCE=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'ArtSource/Characters/Brasa/Brasa.blend'))
arm=next(o for o in bpy.data.objects if o.type=='ARMATURE')
def rot(name,angles):
 b=arm.pose.bones[name];q=b.bone.matrix_local.to_quaternion();b.rotation_quaternion=q.conjugated()@Euler(angles,'XYZ').to_quaternion()@q
moves=['SwordSlash','SwordReturn','SwordThrust','AxeCleave','AxeBackhand','AxeSpin','HammerSweep','HammerBack','HammerSlam','HeavySword','HeavyAxe','HeavyHammer']
for name in moves:
 if bpy.data.actions.get(name):bpy.data.actions.remove(bpy.data.actions[name])
 action=bpy.data.actions.new(name);action.use_fake_user=True;arm.animation_data.action=action
 length=41 if name.startswith('Heavy') else 33
 for frame in range(1,length+1):
  t=(frame-1)/(length-1);hit=.5 if name.startswith('Heavy') else .3
  def smooth(x):x=max(0,min(1,x));return x*x*(3-2*x)
  anticipation=smooth(t/(hit*.65)) if t<hit else 1-smooth((t-hit)/(1-hit))
  swing=smooth((t-hit*.65)/(hit*.35)) if t<hit else 1-smooth((t-hit)/(1-hit))
  recoil=math.sin(math.pi*max(0,min(1,(t-hit)/(.22))))*.11 if hit<t<hit+.22 else 0
  for b in arm.pose.bones:b.rotation_mode='QUATERNION';b.location=(0,0,0);b.rotation_quaternion=Quaternion();b.scale=(1,1,1)
  rot('UpperArm.R',(-.24,0,-.10));rot('UpperArm.L',(-.12,0,.08));rot('Forearm.R',(-.28,0,0));rot('Forearm.L',(-.16,0,0))
  power=1.15 if 'Axe' in name else 1.3 if 'Hammer' in name else 1
  reverse=-1 if name in ['SwordReturn','AxeBackhand','HammerBack'] else 1
  overhead=name in ['HammerSlam','HeavyHammer','HeavyAxe','HeavySword']
  if overhead:
   rot('UpperArm.R',(-.24-1.95*anticipation+2.25*swing, .1, -.1+.2*swing))
   rot('Forearm.R',(-.28-.8*anticipation+.7*swing,0,0))
   rot('UpperArm.L',(-.12-1.4*anticipation+1.8*swing,0,.08-.25*anticipation))
   rot('Forearm.L',(-.16-.85*anticipation+.7*swing,0,0))
   rot('Spine',(-.24*anticipation+.58*swing+recoil,0,-.12*anticipation+.16*swing))
  elif name=='SwordThrust':
   rot('UpperArm.R',(-.24-.65*anticipation-.5*swing,.10,-.1+.20*anticipation-.25*swing))
   rot('Forearm.R',(-.28-1.1*anticipation+1.35*swing,0,0))
   rot('Spine',(.12*swing,0,-.30*anticipation+.50*swing))
   rot('UpperArm.L',(-.12-.25*swing,0,.08+.35*swing))
  else:
   rot('UpperArm.R',(-.24-.6*anticipation+.25*swing,.12*anticipation,-.10+reverse*power*(-.85*anticipation+1.65*swing)))
   rot('Forearm.R',(-.28-.55*anticipation+.35*swing,0,0))
   rot('UpperArm.L',(-.12-.35*anticipation+.1*swing,0,.08+reverse*(.3*anticipation-.6*swing)))
   rot('Forearm.L',(-.16-.5*anticipation+.2*swing,0,0))
   rot('Spine',(.12*swing+recoil,0,reverse*power*(-.38*anticipation+.78*swing)))
  if name=='AxeSpin':
   angle=math.tau*smooth((t-.10)/.65);rot('Spine',(.08*math.sin(t*math.pi),0,angle));rot('UpperArm.R',(-.5,0,-1.1*math.sin(math.pi*t)));rot('UpperArm.L',(-.25,0,.4*math.sin(math.pi*t)))
  rot('Head',(-.06*anticipation+.12*swing,0,.1*anticipation-.1*swing))
  rot('Thigh.L',(-.18*anticipation-.12*swing,0,-.035*anticipation));rot('Shin.L',(.25*anticipation+.15*swing,0,0));rot('Foot.L',(-.07*anticipation-.03*swing,0,0))
  rot('Thigh.R',(.06*anticipation-.09*swing,0,.03*anticipation));rot('Shin.R',(.12*anticipation+.17*swing,0,0));rot('Foot.R',(-.18*anticipation-.08*swing,0,0))
  arm.pose.bones['Hips'].location.y=-.06*anticipation-.04*swing;arm.pose.bones['Hips'].location.x=.035*reverse*anticipation-.035*reverse*swing
  for bone in arm.pose.bones:
   bone.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=bone.name);bone.keyframe_insert(data_path='location',frame=frame,group=bone.name)
arm.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT')
for name in [arm.name,'BrasaBody','GreatSword']:bpy.data.objects[name].select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/ForjaDasMares/Art/Characters/Brasa/Brasa.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,armature_nodetype='NULL',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y')
# Original static meshes, four shared materials.
palette={'Iron':((.26,.34,.38,1),.38,.7),'Bronze':((.55,.29,.09,1),.4,.65),'Leather':((.13,.055,.025,1),.86,0),'Rune':((.08,.65,.8,1),.25,.25),'Pages':((.68,.59,.37,1),.9,0)}
mats={}
for name,(color,rough,metal) in palette.items():
 m=bpy.data.materials.new('Arsenal_'+name);m.diffuse_color=color;m.use_nodes=True;bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=color;bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal;mats[name]=m
parts=[]
def finish(obj,mat):
 obj.data.materials.append(mats[mat]);bpy.context.view_layer.objects.active=obj;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);parts.append(obj);obj.select_set(False);return obj

def profile(name,outline,thickness,mat,y=0):
 v=[(x,y-thickness/2,z) for x,z in outline]+[(x,y+thickness/2,z) for x,z in outline];n=len(outline);f=[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],f);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
 bevel=o.modifiers.new('Forged bevel','BEVEL');bevel.width=.012;bevel.segments=2;bpy.ops.object.modifier_apply(modifier=bevel.name);return finish(o,mat)

def cylinder(a,b,r,mat):
 a,b=Vector(a),Vector(b);d=b-a;bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=r,radius2=r*.95,depth=d.length,location=(a+b)*.5);o=bpy.context.object;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();bpy.ops.object.transform_apply(location=False,rotation=True,scale=False);return finish(o,mat)

def box(pos,size,mat):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);m=o.modifiers.new('Soft bevel','BEVEL');m.width=.025;m.segments=2;bpy.ops.object.modifier_apply(modifier=m.name);return finish(o,mat)

def crystal(pos,size,mat='Rune'):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=pos);o=bpy.context.object;o.scale=size;return finish(o,mat)

def curve(points,r,mat):
 c=bpy.data.curves.new('Carved bow','CURVE');c.dimensions='3D';c.bevel_depth=r;c.bevel_resolution=2;s=c.splines.new('POLY');s.points.add(len(points)-1)
 for p,co in zip(s.points,points):p.co=(*co,1)
 o=bpy.data.objects.new('Bent limb',c);bpy.context.collection.objects.link(o);bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH');return finish(bpy.context.object,mat)

def grip(top=.2):
 cylinder((0,0,-.26),(0,0,top),.055,'Leather')
 for z in [-.24,-.14,-.04,.06,top]:cylinder((0,0,z-.012),(0,0,z+.012),.065,'Bronze')
 crystal((0,0,-.31),(.09,.075,.10),'Bronze')
report={};weapons=[]
for kind in ['Sword','Axe','Hammer','Bow','Daggers','Staff','Grimoire','Shield','MagicWeapon']:
 parts=[]
 if kind in ['Sword','Daggers']:
  grip(.18);length=1.35 if kind=='Sword' else .72
  profile('Tempered blade',[(-.10,.20),(-.13,.36),(-.085,length-.16),(0,length),(.085,length-.16),(.13,.36),(.10,.20)],.075,'Iron')
  profile('Central ridge',[(-.022,.32),(0,length-.13),(.022,.32)],.015,'Bronze',-.048)
  profile('Swept guard',[(-.28,.12),(-.32,.20),(-.08,.26),(.08,.26),(.32,.20),(.28,.12),(.08,.18),(-.08,.18)],.13,'Bronze')
  crystal((0,-.1,.22),(.065,.035,.08))
 elif kind=='Axe':
  grip(.35);cylinder((0,0,.22),(0,0,1.12),.052,'Leather')
  profile('Bearded axe',[(0,.78),(.28,.69),(.50,.78),(.57,1.08),(.40,1.17),(.05,1.04)],.13,'Iron')
  profile('Bronze binding',[(-.09,.77),(.08,.77),(.08,1.13),(-.09,1.13)],.17,'Bronze')
  profile('Rear pick',[(-.07,.98),(-.4,1.12),(-.27,.93),(-.07,.88)],.09,'Iron');crystal((.1,-.09,.94),(.07,.025,.12))
 elif kind=='Hammer':
  grip(.4);cylinder((0,0,.3),(0,0,1.18),.06,'Leather');box((0,0,1.10),(.70,.34,.38),'Iron')
  for x in [-.36,.36]:box((x,0,1.10),(.09,.39,.42),'Bronze')
  profile('Hammer sigil',[(-.10,1.1),(0,1.27),(.10,1.1),(0,.94)],.018,'Rune',-.19)
 elif kind=='Bow':
  pts=[(.35*math.cos(-math.pi/2+i*math.pi/24)-.35,0,.78*math.sin(-math.pi/2+i*math.pi/24)) for i in range(25)];curve(pts,.045,'Leather');curve([pts[0],pts[-1]],.008,'Pages');cylinder((0,0,-.14),(0,0,.14),.055,'Bronze')
  for z in [-.55,.55]:crystal((-.11,0,z),(.05,.065,.12),'Bronze')
 elif kind in ['Staff','MagicWeapon']:
  grip(.28);cylinder((0,0,.15),(0,0,1.15 if kind=='Staff' else .6),.04,'Leather')
  if kind=='MagicWeapon':
   bpy.ops.mesh.primitive_torus_add(major_segments=24,minor_segments=6,major_radius=.23,minor_radius=.022,location=(0,0,.82),rotation=(math.pi/2,0,0));finish(bpy.context.object,'Bronze')
  top=1.24 if kind=='Staff' else .72;crystal((0,0,top+.12),(.12,.1,.30))
  for sign in [-1,1]:cylinder((0,0,top-.2),(sign*.18,0,top+.15),.035,'Bronze');crystal((sign*.18,0,top+.20),(.045,.045,.1))
 elif kind=='Grimoire':
  box((0,0,.13),(.43,.14,.57),'Pages')
  for y in [-.095,.095]:box((0,y,.13),(.48,.055,.64),'Leather')
  cylinder((-.23,0,-.18),(-.23,0,.44),.07,'Bronze')
  for z in [-.12,.37]:box((0,-.13,z),(.48,.025,.045),'Bronze')
  profile('Book seal',[(-.12,.14),(0,.32),(.12,.14),(0,-.04)],.025,'Bronze',-.14);crystal((0,-.17,.14),(.055,.025,.085))
 elif kind=='Shield':
  shape=[(-.34,.4),(.34,.4),(.38,.13),(.24,-.26),(0,-.47),(-.24,-.26),(-.38,.13)]
  profile('Kite rim',shape,.15,'Bronze');profile('Kite face',[(x*.88,z*.88) for x,z in shape],.04,'Iron',-.095)
  profile('Shield crest',[(-.10,.05),(0,.27),(.10,.05),(0,-.25)],.03,'Rune',-.13);box((0,.13,0),(.24,.10,.09),'Leather')
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='Arsenal_'+kind;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 bpy.ops.export_scene.fbx(filepath=str(OUT/(kind+'.fbx')),use_selection=True,object_types={'MESH'},bake_anim=False,add_leaf_bones=False,axis_forward='-Z',axis_up='Y')
 report[kind]={'triangles':sum(len(poly.vertices)-2 for poly in o.data.polygons),'vertices':len(o.data.vertices)};weapons.append(o)
(OUT/'palette.json').write_text(json.dumps({'materials':[{'name':'Arsenal_'+k,'color':list(v[0]),'roughness':v[1],'metallic':v[2]} for k,v in palette.items()]},indent=2))
# Save a separate editable source; original Brasa.blend stays untouched.
for o in list(bpy.context.scene.objects):o.hide_render=o not in weapons
for i,o in enumerate(weapons):o.location=((i%3-1)*1.7,0,(2-i//3)*2)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=1000;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
bpy.ops.object.camera_add(location=(3,-11,5));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,2.5))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=6.6;scene.camera=camera
for pos,power,size in [((-3,-5,7),1200,5),((4,-2,5),900,4)]:
 bpy.ops.object.light_add(type='AREA',location=pos);lamp=bpy.context.object;lamp.data.energy=power;lamp.data.shape='DISK';lamp.data.size=size;lamp.rotation_euler=(Vector((0,0,2.5))-lamp.location).to_track_quat('-Z','Y').to_euler()
scene.world.color=(.12,.12,.12);scene.render.filepath=str(SOURCE/'Arsenal_preview.png')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Brasa_Arsenal.blend'));bpy.ops.render.render(write_still=True)
report['newClips']=moves;report['originalActionsPreserved']=13;(SOURCE/'arsenal_report.json').write_text(json.dumps(report,indent=2));print('ARSENAL_READY '+json.dumps(report))

# Real renders from these original meshes, used as inventory icons.
icons=OUT/'Icons';icons.mkdir(exist_ok=True);scene.render.resolution_x=scene.render.resolution_y=256;scene.render.film_transparent=True;scene.cycles.samples=8
camera.location=(2,-4,2);camera.rotation_euler=(Vector((0,0,.55))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=1.95
for obj in weapons:obj.hide_render=True
for obj in weapons:
 obj.hide_render=False;obj.location=(0,0,0);scene.render.filepath=str(icons/(obj.name.replace('Arsenal_','')+'.png'));bpy.ops.render.render(write_still=True);obj.hide_render=True
print('ARSENAL_ICONS_READY 9')
