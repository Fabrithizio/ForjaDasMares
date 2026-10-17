"""Original low-poly game assets and inventory renders. Blender 4.5, no external inputs."""
import bpy, math, sys, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ForjaDasMares/Art/ExpeditionSet'
OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
palette={'wood':(.25,.10,.035),'edge':(.55,.29,.09),'steel':(.25,.32,.34),'gold':(.85,.49,.09),'stone':(.20,.25,.23),'leaf':(.08,.28,.10),'skin':(.32,.51,.20),'cloth':(.27,.06,.09),'dark':(.045,.055,.07),'cyan':(.08,.7,.85),'healing':(.15,.85,.32),'fire':(.95,.25,.035),'bone':(.68,.60,.40)}
M={}
for name,col in palette.items():
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*col,1);bs.inputs['Roughness'].default_value=.65
 M[name]=m
def cube(name,p,s,mat,parent=None):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.scale=s;o.data.materials.append(M[mat]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if parent:o.parent=parent
 return o
def ball(name,p,s,mat,parent=None):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=p);o=bpy.context.object;o.name=name;o.scale=s;o.data.materials.append(M[mat])
 if parent:o.parent=parent
 return o
def cyl(name,p,r,d,mat,n=10):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=d,location=p);o=bpy.context.object;o.name=name;o.data.materials.append(M[mat]);return o
def pivot(name,p):
 o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.location=p;return o
def limb(name,p,length,width,mat):
 root=pivot(name,p);cube(name+'Mesh',(0,0,-length*.5),(width,width,length),mat,root);return root
def export(name,before):
 objects=[o for o in bpy.context.scene.objects if o not in before]
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
 for o in objects:o.hide_render=True;o.hide_set(True)
 return objects
def prop(kind):
 before=set(bpy.context.scene.objects)
 if kind in ('supply','crate'):
  for i in range(5):cube('OakPlank'+str(i),((i-2)*.19,0,.5),(.18,.95,.95),'wood')
  for z in (.13,.87):cube('IronBand',(0,0,z),(1.04,1.02,.10),'steel')
  for y in (-.51,.51):
   o=cube('Brace',(0,y,.5),(.13,.06,1.05),'edge');o.rotation_euler[1]=math.radians(40)
  if kind=='supply':cube('HealingMark',(0,-.554,.51),(.42,.025,.12),'healing');cube('HealingMark2',(0,-.555,.51),(.12,.025,.42),'healing')
 elif kind=='barrel':
  cyl('OakBarrel',(0,0,.53),.45,1.06,'wood',12)
  for z in (.15,.53,.91):cyl('Hoop',(0,0,z),.47,.085,'steel',12)
 elif kind=='seal':
  cyl('Plinth',(0,0,.15),.58,.3,'stone');o=ball('SealCrystal',(0,0,.9),(.32,.32,.7),'cyan');o.rotation_euler[1]=.15
  for i in range(4):cube('Rune',(math.cos(i*math.pi/2)*.5,math.sin(i*math.pi/2)*.5,.4),(.13,.13,.4),'gold')
 elif kind=='well':
  cyl('Base',(0,0,.13),.72,.26,'stone');cyl('Water',(0,0,.32),.58,.10,'healing')
  for i in range(8):cube('StoneRim',(math.cos(i*math.pi/4)*.65,math.sin(i*math.pi/4)*.65,.38),(.3,.3,.3),'stone')
 elif kind=='rock':
  ball('Rock',(0,0,.55),(1,.75,.8),'stone');ball('Shard',(.65,.1,.25),(.4,.4,.4),'steel')
 elif kind=='bush':
  for i in range(4):ball('Leaves',(math.cos(i*1.8)*.35,math.sin(i*1.8)*.35,.45),(.65,.5,.55),'leaf')
 elif kind=='mushrooms':
  for x,y,h in [(-.2,0,.65),(.25,.1,.4)]:cyl('Stem',(x,y,h*.45),.09,h,'bone');ball('Cap',(x,y,h),(.38,.38,.18),'cyan')
 elif kind=='banner':
  cyl('Pole',(0,0,1.2),.05,2.4,'wood');cube('Flag',(.4,0,1.9),(.8,.04,.7),'cloth');cube('Sigil',(.4,-.03,1.9),(.14,.02,.4),'gold')
 return export(kind,before)
def enemy(kind):
 before=set(bpy.context.scene.objects)
 elite=kind.startswith('elite');guard=kind in ('guardian','elite_warden');mage=kind in ('summoner','elite_shaman');archer=kind=='archer'
 skin='stone' if guard else 'bone' if mage else 'skin';cloth='cyan' if mage else 'cloth'
 ball('Torso',(0,0,1.02),(.34,.23,.44),skin);cube('Belt',(0,0,.82),(.65,.48,.12),'wood')
 ball('Head',(0,-.015,1.57),(.28,.26,.30),skin)
 for x in (-.115,.115):cube('Eye',(x,-.255,1.61),(.065,.04,.065),'fire' if elite else 'gold')
 for x in (-.30,.30):o=ball('Ear',(x,0,1.61),(.18,.10,.09),skin);o.rotation_euler[1]=x
 for x in (-.16,.16):
  root=limb('LegL' if x<0 else 'LegR',(x,0,.78),.59,.21,'dark');cube('Boot',(0,-.08,-.61),(.25,.37,.18),'wood',root)
 for x in (-.39,.39):limb('ArmL' if x<0 else 'ArmR',(x,0,1.30),.58,.20,skin)
 if guard:
  cube('Breastplate',(0,-.05,1.10),(.67,.48,.55),'steel');cube('Shield',(-.43,-.30,1.02),(.55,.16,.85),'steel')
  cube('ShieldRune',(-.43,-.40,1.02),(.12,.03,.48),'gold')
 if mage:
  bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=.4,radius2=.19,depth=.8,location=(0,0,.77));bpy.context.object.data.materials.append(M[cloth])
  cyl('Staff',(.52,0,1),.055,1.8,'wood');ball('StaffGem',(.52,0,1.94),(.17,.17,.25),'cyan')
 elif archer:
  for z in (.65,1.5):o=cube('Bow',(.5,-.12,z),(.10,.10,.5),'edge');o.rotation_euler[1]=.55 if z<1 else -.55
  cube('String',(.66,-.12,1.08),(.018,.018,1.1),'bone')
 elif kind=='unstable':
  cyl('PowderKeg',(0,-.22,1.02),.33,.55,'wood');cyl('Fuse',(0,-.25,1.45),.04,.25,'fire')
 else:
  o=cube('Blade',(.47,-.12,.75),(.17,.12,.78),'steel');o.rotation_euler[0]=.2
 if elite:
  for x in (-.36,.36):ball('Shoulder',(x,0,1.35),(.28,.3,.22),'gold' if guard else 'steel')
  for x in (-.2,.2):o=cube('Horn',(x,0,1.97),(.09,.10,.4),'bone');o.rotation_euler[1]=x*2
 return export(kind,before)
def weapon(kind):
 if kind in ('Sword','Daggers','Axe','Hammer','MagicWeapon'):
  cube('Grip',(0,0,-.55),(.15,.18,.45),'wood');cube('Guard',(0,0,-.25),(.65,.18,.12),'gold')
  if kind=='Axe':cube('Head',(.12,0,.48),(.85,.22,.60),'steel');cube('Haft',(0,0,.1),(.12,.13,1.2),'wood')
  elif kind=='Hammer':cube('Head',(0,0,.53),(.9,.45,.55),'steel');cube('Haft',(0,0,.1),(.12,.13,1.2),'wood')
  else:
   cube('Blade',(0,0,.25),(.28,.13,.95),'cyan' if kind=='MagicWeapon' else 'steel');ball('Tip',(0,0,.78),(.14,.065,.23),'steel')
   if kind=='Daggers':cube('SecondBlade',(.48,.1,.18),(.18,.12,.85),'steel')
 elif kind=='Bow':
  for z in (-.55,.55):o=cube('Bow',(-.15,0,z),(.15,.15,.8),'edge');o.rotation_euler[1]=-.55 if z<0 else .55
  cube('String',(.05,0,0),(.02,.02,1.65),'bone');cube('Arrow',(.4,0,0),(.08,.08,1.3),'steel')
 elif kind=='Staff':cyl('Staff',(0,0,0),.07,1.8,'wood');ball('Gem',(0,0,.9),(.28,.22,.35),'cyan')
 elif kind=='Grimoire':cube('Book',(0,0,0),(.95,.3,1.1),'cloth');cube('Pages',(0,-.03,0),(.86,.30,.98),'bone');cube('Cover',(0,-.2,0),(.95,.1,1.1),'cloth');cube('Sigil',(0,-.27,0),(.13,.03,.7),'gold')
 elif kind=='Shield':ball('Shield',(0,0,0),(.6,.19,.8),'steel');cube('Cross',(0,-.19,0),(.14,.08,1.1),'gold');cube('Cross2',(0,-.2,.15),(.8,.08,.15),'gold')
 elif kind=='Armor':cube('Chest',(0,0,0),(.8,.4,.9),'steel');ball('Shoulder',(-.55,0,.25),(.35,.3,.25),'gold');ball('Shoulder',(.55,0,.25),(.35,.3,.25),'gold')
 elif kind=='Accessory':
  bpy.ops.mesh.primitive_torus_add(major_segments=16,minor_segments=6,location=(0,0,0),rotation=(math.pi/2,0,0),major_radius=.48,minor_radius=.10);bpy.context.object.data.materials.append(M['gold']);ball('Gem',(0,-.06,.5),(.23,.16,.23),'cyan')
 else:ball('Artifact',(0,0,0),(.45,.3,.65),'cyan');cyl('Base',(0,0,-.6),.55,.14,'gold')
for k in ('supply','crate','barrel','seal','well','rock','bush','mushrooms','banner'):prop(k)
for k in ('raider','guardian','archer','unstable','summoner','elite_warden','elite_shaman','elite_reaver'):enemy(k)
# Render actual original meshes for icons; no copied textures or icon packs.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=12;scene.cycles.use_denoising=True
scene.render.resolution_x=128;scene.render.resolution_y=128;scene.render.resolution_percentage=100;scene.render.film_transparent=True
scene.world.color=(.35,.35,.35)
bpy.ops.object.camera_add(location=(2.3,-5,2));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=2.7;scene.camera=cam
for pwr,pos in [(550,(1,-3,4)),(300,(-3,-1,1))]:
 bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.data.energy=pwr;o.data.shape='DISK';o.data.size=4;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
for kind in ('Sword','Axe','Hammer','Bow','Daggers','Staff','Grimoire','Shield','MagicWeapon','Armor','Accessory','Artifact'):
 before=set(scene.objects);weapon(kind);objects=[o for o in scene.objects if o not in before]
 scene.render.filepath=str(OUT/('Icon_'+kind+'.png'));bpy.ops.render.render(write_still=True)
 for o in objects:o.hide_render=True;o.hide_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(Path(__file__).with_name('ExpeditionSet.blend')))
print('EXPEDITION_ASSETS_CREATED models=17 icons=12 source=original')
