"""Original Prisma model, rig, keyed actions, FBX and studio preview. Blender 4.5 LTS."""
import bpy, bmesh, math, json, sys
from pathlib import Path
from mathutils import Vector, Quaternion, Euler

ROOT = Path(__file__).resolve().parents[3]
SOURCE = Path(__file__).resolve().parent
OUTPUT = ROOT / 'Assets/ForjaDasMares/Art/Characters/Prisma'
OUTPUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for action in list(bpy.data.actions): bpy.data.actions.remove(action)
palette = {
 'Skin': ((.56,.32,.18,1),.8,0), 'SkinLight': ((.68,.43,.27,1),.8,0),
 'Hair': ((.07,.026,.10,1),.85,0), 'Cloth': ((.11,.035,.23,1),.9,0),
 'Teal': ((.025,.25,.29,1),.86,0), 'Lining': ((.29,.09,.39,1),.9,0),
 'Gold': ((.7,.41,.13,1),.4,.65), 'Leather': ((.10,.045,.055,1),.85,0),
 'Dark': ((.025,.025,.055,1),.9,0), 'EyeWhite': ((.88,.85,.74,1),.5,0),
 'Crystal': ((.13,.88,1,1),.25,.2), 'Violet': ((.65,.24,.95,1),.35,.2),
}
materials={}
for name,(color,rough,metal) in palette.items():
    m=bpy.data.materials.new('Prisma_'+name);m.diffuse_color=color;m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=color
    bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
    materials[name]=m
parts=[];weapon=[]
def finish(obj,name,mat,bone='Spine',is_weapon=False,smooth=False):
    obj.name=name;obj.data.materials.append(materials[mat])
    bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for poly in obj.data.polygons:poly.use_smooth=smooth
    group=obj.vertex_groups.new(name=bone);group.add(list(range(len(obj.data.vertices))),1,'REPLACE')
    (weapon if is_weapon else parts).append(obj);obj.select_set(False);return obj
def mesh(name,verts,faces,mat,bone='Spine',is_weapon=False):
    data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update()
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj)
    return finish(obj,name,mat,bone,is_weapon)
def oval(name,pos,scale,mat,bone='Spine',segments=12,rings=8,smooth=False,is_weapon=False):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,location=pos)
    obj=bpy.context.object;obj.scale=scale;return finish(obj,name,mat,bone,is_weapon,smooth)
def tube(name,a,b,r1,r2,mat,bone='Spine',n=10,is_weapon=False):
    a,b=Vector(a),Vector(b);direction=b-a
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=direction.length,location=(a+b)*.5)
    obj=bpy.context.object;obj.rotation_mode='QUATERNION';obj.rotation_quaternion=direction.to_track_quat('Z','Y')
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=False)
    return finish(obj,name,mat,bone,is_weapon)
def box(name,pos,scale,mat,bone='Spine',bevel=.025,is_weapon=False):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);obj=bpy.context.object;obj.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=obj.modifiers.new('Cut edges','BEVEL');mod.width=bevel;mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return finish(obj,name,mat,bone,is_weapon)
def rings(name,rows,mat,bone='Spine',count=12):
    verts=[];faces=[]
    for z,cx,cy,rx,ry in rows:
        for i in range(count):
            a=2*math.pi*i/count;verts.append((cx+rx*math.sin(a),cy-ry*math.cos(a),z))
    for j in range(len(rows)-1):
        for i in range(count):
            a=j*count+i;b=j*count+(i+1)%count;c=b+count;d=a+count
            faces.extend([(a,b,c),(a,c,d)])
    faces.append(tuple(reversed(range(count))));faces.append(tuple(range((len(rows)-1)*count,len(rows)*count)))
    return mesh(name,verts,faces,mat,bone)

# Original split tunic and hood silhouette; short coat tails leave both feet visible.
rings('Tunic',[(.92,0,0,.25,.16),(1.12,0,0,.26,.165),(1.40,0,0,.32,.18),(1.54,0,0,.31,.17),(1.60,0,0,.19,.12)],'Cloth')
tube('Neck',(0,0,1.52),(0,-.005,1.77),.105,.11,'Skin','Neck')
rings('Face',[(1.71,0,-.03,.11,.10),(1.78,0,-.025,.16,.135),(1.94,0,-.01,.195,.16),(2.09,0,.01,.18,.15),(2.17,0,.02,.12,.10)],'SkinLight','Head',16)
# Hood is an open shell, not a sphere over the face.
verts=[];faces=[]
for z,rx,ry,cy in [(1.73,.24,.18,.045),(1.92,.285,.23,.025),(2.10,.25,.21,.03),(2.24,.16,.15,.055),(2.29,.05,.06,.08)]:
    for i in range(13):
        angle=math.radians(55+250*i/12);verts.append((rx*math.sin(angle),cy-ry*math.cos(angle),z))
for row in range(4):
    for i in range(12):
        a=row*13+i;faces.append((a,a+1,a+14,a+13))
hood=mesh('Open hood',verts,faces,'Cloth','Head')
mod=hood.modifiers.new('Woven thickness','SOLIDIFY');mod.thickness=.025;bpy.context.view_layer.objects.active=hood;bpy.ops.object.modifier_apply(modifier=mod.name)
for side in [-1,1]:
    for row in range(4):
        a=verts[row*13+(0 if side>0 else 12)];b=verts[(row+1)*13+(0 if side>0 else 12)];tube('Hood seam',a,b,.012,.012,'Gold','Head',6)
    x=side*.085
    oval('Eye white',(x,-.162,1.971),(.047,.014,.031),'EyeWhite','Head',12,6,True)
    oval('Iris',(x,-.176,1.974),(.023,.006,.027),'Teal','Head',12,6,True)
    oval('Pupil',(x,-.181,1.976),(.01,.003,.019),'Dark','Head',10,6,True)
    oval('Glint',(x-.006,-.184,1.985),(.006,.002,.007),'EyeWhite','Head',8,5)
    tube('Brow',(side*.035,-.185,2.025),(side*.143,-.166,2.039),.013,.017,'Hair','Head',6)
mesh('Nose',[(-.025,-.163,1.98),(.025,-.163,1.98),(0,-.215,1.92),(-.03,-.177,1.91),(.03,-.177,1.91)],[(0,1,2),(0,2,3),(1,4,2),(2,4,3),(0,3,4,1)],'SkinLight','Head')
tube('Mouth',(-.048,-.171,1.84),(.048,-.171,1.84),.005,.005,'Leather','Head',6)
# Silver-violet locks are visible at the forehead and under the hood.
for i in range(5):
    x=-.14+i*.067
    mesh('Swept lock',[(x-.04,-.14,2.14),(x+.04,-.14,2.14),(x+.025,-.183,2.02),(x-.025,-.19,2.07),(x,-.20,2.12)],[(0,1,4),(1,2,4),(2,3,4),(3,0,4)],'Hair','Head')
for sign,side in [(-1,'L'),(1,'R')]:
    thigh='Thigh.'+side;shin='Shin.'+side
    tube('Trousers',(sign*.16,0,.97),(sign*.22,0,.54),.12,.105,'Dark',thigh)
    tube('Boot',(sign*.22,0,.52),(sign*.22,0,.14),.105,.095,'Leather',shin)
    oval('Boot toe',(sign*.22,-.09,.105),(.12,.23,.105),'Leather','Foot.'+side,12,6)
    tube('Boot cuff',(sign*.22,0,.44),(sign*.22,0,.49),.112,.115,'Gold',shin,10)
    # Split panels deform with the thighs, no skirt tube joining the legs.
    rings('Side tabard',[(.64,sign*.23,.07,.14,.16),(.75,sign*.205,.035,.15,.17),(1.02,sign*.14,0,.13,.155)],'Teal',thigh,8)
    tube('Sleeve',(sign*.32,0,1.49),(sign*.61,0,1.17),.14,.115,'Cloth','UpperArm.'+side,10)
    tube('Sleeve cuff',(sign*.61,0,1.17),(sign*.66,-.01,1.07),.13,.135,'Teal','Forearm.'+side,10)
    tube('Forearm',(sign*.63,0,1.14),(sign*.71,-.025,.94),.08,.06,'Leather','Forearm.'+side,10)
    oval('Hand',(sign*.72,-.035,.89),(.078,.068,.10),'SkinLight','Hand.'+side,12,6)
    for i in range(3):tube('Fingers',(sign*(.68+i*.025),-.078,.89),(sign*(.68+i*.025),-.09,.83),.016,.014,'Skin','Hand.'+side,6)
    oval('Shoulder cape',(sign*.29,.025,1.52),(.21,.20,.105),'Teal','Spine',12,6)
    tube('Epaulet trim',(sign*.15,-.12,1.56),(sign*.43,-.11,1.48),.016,.016,'Gold','Spine',6)
# Collar, sash, belt, pouches and triangular crystal insignia.
rings('Belt',[(.98,0,0,.27,.175),(1.06,0,0,.27,.175)],'Leather','Hips')
box('Buckle',(0,-.185,1.025),(.13,.035,.11),'Gold','Hips',.01)
box('Buckle inset',(0,-.209,1.025),(.055,.018,.055),'Crystal','Hips',.01)
for sign in [-1,1]:
    tube('Sash',(sign*.22,-.16,1.49),(0,-.19,1.20),.038,.025,'Gold','Spine',6)
    box('Scroll pouch',(sign*.27,.045,1.02),(.12,.15,.18),'Leather','Hips',.02)
    tube('Scroll',(sign*.29,.06,1.0),(sign*.29,.06,1.25),.035,.035,'EyeWhite','Hips',8)
# Two tapered mantle tails have separate animated bones.
for sign,side in [(-1,'L'),(1,'R')]:
    mesh('Mantle tail',[(sign*.05,.19,1.54),(sign*.29,.18,1.50),(sign*.34,.25,.78),(sign*.10,.28,.61)],[(0,1,2,3),(3,2,1,0)],'Lining','Cape.'+side)
    tube('Mantle piping',(sign*.29,.18,1.50),(sign*.34,.25,.78),.012,.01,'Gold','Cape.'+side,6)
# Faceted prism emblem and fallback staff; equipped weapons replace the latter.
oval('Brooch',(0,-.21,1.43),(.085,.035,.10),'Gold','Spine',8,4)
oval('Prism insignia',(0,-.25,1.43),(.045,.025,.075),'Crystal','Spine',6,4)
tube('Staff shaft',(.72,-.07,.28),(.72,-.07,1.93),.035,.035,'Leather','Hand.R',10,True)
for z in [.37,.89,1.64,1.83]:tube('Staff band',(.72,-.07,z),(.72,-.07,z+.055),.049,.049,'Gold','Hand.R',8,True)
for sign in [-1,1]:tube('Staff crown',(.72,-.07,1.81),(.72+sign*.12,-.07,2.06),.035,.019,'Gold','Hand.R',6,True)
oval('Staff crystal',(.72,-.07,2.04),(.09,.09,.19),'Crystal','Hand.R',6,4,is_weapon=True)
# Bind a compact skeleton; armor is rigid-weighted to its corresponding limb.
arm_data=bpy.data.armatures.new('PrismaSkeleton');arm=bpy.data.objects.new('PrismaRig',arm_data);bpy.context.collection.objects.link(arm)
bpy.context.view_layer.objects.active=arm;arm.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
bones={
 'Root':((0,0,0),(0,0,.35),None),
 'Hips':((0,0,.95),(0,0,1.1),'Root'), 'Spine':((0,0,1.1),(0,0,1.56),'Hips'),
 'Neck':((0,0,1.56),(0,0,1.74),'Spine'), 'Head':((0,0,1.74),(0,0,2.16),'Neck'),
}
for sign,side in [(-1,'L'),(1,'R')]:
    bones.update({'UpperArm.'+side:((sign*.40,0,1.48),(sign*.61,0,1.17),'Spine'),
      'Forearm.'+side:((sign*.61,0,1.17),(sign*.71,-.025,.94),'UpperArm.'+side),
      'Hand.'+side:((sign*.71,-.025,.94),(sign*.735,-.04,.81),'Forearm.'+side),
      'Thigh.'+side:((sign*.16,0,.98),(sign*.22,0,.54),'Hips'),
      'Shin.'+side:((sign*.22,0,.54),(sign*.22,0,.14),'Thigh.'+side),
      'Foot.'+side:((sign*.22,0,.14),(sign*.22,-.25,.10),'Shin.'+side)})
for side,sign in [('L',-1),('R',1)]:bones['Cape.'+side]=((sign*.18,.20,1.51),(sign*.22,.25,.75),'Spine')
for name,(head,tail,parent) in bones.items():
    b=arm_data.edit_bones.new(name);b.head=head;b.tail=tail
    if parent:b.parent=arm_data.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');arm.select_set(False)
def join_objects(objects,name):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();obj=bpy.context.object;obj.name=name
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    mod=obj.modifiers.new('Prisma skin','ARMATURE');mod.object=arm;obj.parent=arm
    return obj
body=join_objects(parts,'PrismaBody');sword=join_objects(weapon,'PrismaStaff')
for obj in [body,sword]:
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
for bone in arm.pose.bones:bone.rotation_mode='QUATERNION'
def rotate(name,angles):
    b=arm.pose.bones[name];rest=b.bone.matrix_local.to_quaternion()
    b.rotation_quaternion=rest.conjugated()@Euler(angles,'XYZ').to_quaternion()@rest
def pose_reset():
    for b in arm.pose.bones:b.location=(0,0,0);b.rotation_quaternion=Quaternion();b.scale=(1,1,1)
# Coherent ready pose, exact looping endpoints and distinct attacks.
# Positive world-X bends a shin back; the previous negative bend inverted knees.
def ready():
    rotate('UpperArm.R',(-.24,0,-.10));rotate('UpperArm.L',(-.12,0,.08))
    rotate('Forearm.R',(-.28,0,0));rotate('Forearm.L',(-.16,0,0))

def ease(x):
    x=max(0,min(1,x));return x*x*(3-2*x)

def envelope(t,contact=.3):
    if t<.16:return ease(t/.16),0
    if t<contact:return 1,ease((t-.16)/(contact-.16))
    return 1-ease((t-contact)/(1-contact)),1-ease((t-contact)/(1-contact))

def animate_pose(name,t):
    pose_reset();ready();wave=math.sin(t*math.tau)
    if name=='Idle':
        # Relaxed guard: supported knees, shifting weight and a breathing chest.
        arm.pose.bones['Hips'].location.y=-.065+.006*wave
        arm.pose.bones['Hips'].location.x=.012*wave
        rotate('Spine',(-.025+.025*wave,.012*wave,.025*wave))
        rotate('Head',(.015*wave,0,-.045*wave))
        rotate('UpperArm.R',(-.30+.035*wave,0,-.12))
        rotate('UpperArm.L',(-.16-.045*wave,0,.12))
        rotate('Forearm.R',(-.38+.035*wave,0,0));rotate('Forearm.L',(-.27-.035*wave,0,0))
        for side in ['L','R']:
            rotate('Thigh.'+side,(-.20,0,0));rotate('Shin.'+side,(.40,0,0));rotate('Foot.'+side,(-.20,0,0))
    elif name in ['Walk','Run']:
        running=name=='Run';stance=.28 if running else .60
        travel=.80 if running else .64
        flight=abs(math.sin(t*math.tau))
        hip_z=.86+.055*flight if running else .89+.009*math.cos(t*math.tau*2)
        arm.pose.bones['Hips'].location.y=hip_z-.98
        arm.pose.bones['Hips'].location.x=.012*wave
        for sign,side in [(-1,'L'),(1,'R')]:
            cycle=(t+(0 if side=='L' else .5))%1
            if cycle<stance:
                ankle_y=-travel/2+travel*cycle/stance;ankle_z=.14
            else:
                u=(cycle-stance)/(1-stance)
                ankle_y=travel/2-travel*ease(u);ankle_z=.14+(.22 if running else .12)*math.sin(math.pi*u)
            dy,dz=ankle_y,ankle_z-hip_z;distance=min(.839,math.hypot(dy,dz))
            direction=math.atan2(dy,-dz);dy=math.sin(direction)*distance;dz=-math.cos(direction)*distance
            along=(.44**2-.4**2+distance**2)/(2*distance)
            height=math.sqrt(max(0,.44**2-along**2))
            knee_y=dy/distance*along+dz/distance*height
            knee_z=hip_z+dz/distance*along-dy/distance*height
            thigh=math.atan2(knee_y,hip_z-knee_z)
            shin=math.atan2(ankle_y-knee_y,knee_z-ankle_z)
            rotate('Thigh.'+side,(thigh,0,0));rotate('Shin.'+side,(shin-thigh,0,0));rotate('Foot.'+side,(-shin,0,0))
            swing=wave*sign
            rotate('UpperArm.'+side,(-.30+swing*(.55 if running else .30),0,-sign*.10))
            rotate('Forearm.'+side,(-.65 if running else -.35,0,0))
        rotate('Spine',(-.12 if running else -.045,0,.055*wave));rotate('Head',(.055 if running else .02,0,-.03*wave))
    elif name in ['Attack','Attack2','Attack3','Charged','Skill','Super']:
        impact=.5 if name in ['Charged','Super','Skill'] else .3
        anticipation,strike=envelope(t,impact)
        turn=-1 if name=='Attack2' else 1
        overhead=name in ['Attack3','Charged','Skill','Super']
        if overhead:
            rotate('UpperArm.R',(-.24-1.45*anticipation+.95*strike,.08,-.10+.28*strike))
            rotate('Forearm.R',(-.28-.68*anticipation+.65*strike,0,0))
            rotate('Spine',(-.12*anticipation+.25*strike,0,-.16*anticipation+.22*strike))
            rotate('UpperArm.L',(-.12-.55*anticipation+.35*strike,0,.08))
        else:
            rotate('UpperArm.R',(-.24-.65*anticipation+.2*strike,.14*anticipation,-.10+turn*(-.65*anticipation+1.25*strike)))
            rotate('Forearm.R',(-.28-.35*anticipation+.24*strike,0,0))
            rotate('Spine',(.09*strike,0,turn*(-.22*anticipation+.48*strike)))
            rotate('UpperArm.L',(-.12-.20*strike,0,.08-.20*strike))
        rotate('Head',(.04*strike,0,.10*anticipation-.12*strike))
        rotate('Thigh.L',(-.10*strike,0,0));rotate('Shin.L',(.13*strike,0,0))
        if name=='Super':
            rotate('UpperArm.L',(-.12-1.0*anticipation+.7*strike,0,.08-.3*anticipation))
            arm.pose.bones['Hips'].location.y=-.09*anticipation+.09*strike
    elif name=='Dodge':
        pulse=math.sin(math.pi*t)
        rotate('Spine',(-.35*pulse,0,0))
        for side in ['L','R']:
            rotate('Thigh.'+side,(-.45*pulse,0,0));rotate('Shin.'+side,(.65*pulse,0,0))
        arm.pose.bones['Hips'].location.y=-.12*pulse
    elif name=='Hit':
        pulse=math.sin(math.pi*t);rotate('Spine',(.22*pulse,0,.07*pulse));rotate('Head',(.08*pulse,0,0))
    elif name=='Dead':
        fall=ease(t/.8);rotate('Hips',(-1.48*fall,0,.08*fall));arm.pose.bones['Hips'].location.y=-.77*fall
        rotate('UpperArm.R',(-.25*fall,0,.55*fall));rotate('Head',(.18*fall,0,0))
    elif name=='Victory':
        raised=math.sin(math.pi*t)**2
        rotate('UpperArm.R',(-.24-1.85*raised,0,-.1));rotate('Forearm.R',(-.28-.25*raised,0,0))
        rotate('Spine',(-.06*raised,0,.025*wave));rotate('Head',(-.1*raised,0,0))


common_pose=animate_pose
def animate_pose(name,t):
    base={'StaffBolt':'Attack','StaffReturn':'Attack2','StaffRelease':'Attack3','HeavyStaff':'Charged',
      'BookWord':'Attack','BookPage':'Attack2','BookSeal':'Attack3','HeavyBook':'Charged',
      'OrbCast':'Attack','OrbReturn':'Attack2','OrbBurst':'Attack3','HeavyOrb':'Charged'}.get(name,name)
    common_pose(base,t)
    if base in ['Idle','Walk','Run']:
        wave=math.sin(t*math.tau)
        rotate('UpperArm.R',(-.28+.09*wave,0,-.10));rotate('Forearm.R',(-.55+.08*wave,0,0))
        rotate('UpperArm.L',(-.15-.12*wave,0,.10));rotate('Forearm.L',(-.34-.08*wave,0,0))
    if base in ['Attack','Attack2','Attack3','Charged','Skill','Super']:
        charge,release=envelope(t,.5 if base in ['Charged','Skill','Super'] else .3)
        book=name.startswith('Book') or name=='HeavyBook';orb=name.startswith('Orb') or name=='HeavyOrb'
        power=base in ['Charged','Skill','Super'];both=power or book or base=='Attack3'
        rotate('Spine',(-.06*charge+.13*release,0,(-.12 if base=='Attack2' else .12)*(charge-release)))
        rotate('Head',(-.05*charge+.07*release,0,-.06*charge))
        rotate('UpperArm.R',(-.28-(.65 if book else 1.05)*charge+.45*release,0,-.10+(.18 if orb else -.12)*charge))
        rotate('Forearm.R',(-.55-.35*charge+.65*release,0,.15*charge if book else 0))
        rotate('Hand.R',(-.16*release,0,(.32 if base=='Attack3' else .15)*charge))
        rotate('UpperArm.L',(-.15-(1.05 if both else .40)*charge+.35*release,0,.12-(.32 if base=='Super' else .14)*charge))
        rotate('Forearm.L',(-.34-.65*charge+.5*release,0,0))
        rotate('Hand.L',(-.25*charge,0,-.15*charge))
        if base=='Super':
            rotate('UpperArm.R',(-.28-1.85*charge+.65*release,0,-.28*charge))
            rotate('UpperArm.L',(-.15-1.85*charge+.65*release,0,.28*charge))
            arm.pose.bones['Hips'].location.y=-.075*charge+.05*release
    for side,sign in [('L',-1),('R',1)]:
        flutter=math.sin(t*math.tau+sign*.4)*(.14 if base in ['Walk','Run'] else .035)
        rotate('Cape.'+side,(flutter,sign*.035,sign*.02))

actions={};lengths={'Idle':49,'Walk':25,'Run':25,'Attack':25,'Attack2':25,'Attack3':25,'Charged':33,'Skill':33,'Super':41,'Dodge':19,'Hit':13,'Dead':31,'Victory':49}
lengths.update({name:33 if name.startswith('Heavy') else 25 for name in ['StaffBolt','StaffReturn','StaffRelease','HeavyStaff','BookWord','BookPage','BookSeal','HeavyBook','OrbCast','OrbReturn','OrbBurst','HeavyOrb']})
arm.animation_data_create()
for name,length in lengths.items():
    action=bpy.data.actions.new(name);action.use_fake_user=True;arm.animation_data.action=action;actions[name]=action
    # Every frame is keyed; endpoints are evaluated rather than copying frame N-1.
    for frame in range(1,length+1):
        t=(frame-1)/(length-1);animate_pose(name,t)
        for bone in arm.pose.bones:
            bone.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=bone.name)
            bone.keyframe_insert(data_path='location',frame=frame,group=bone.name)
arm.animation_data.action=actions['Idle'];bpy.context.scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT')
for obj in [arm,body,sword]:obj.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=str(OUTPUT/'Prisma.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},
  add_leaf_bones=False,armature_nodetype='NULL',use_armature_deform_only=False,bake_anim=True,
  bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,
  bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',path_mode='AUTO')
(OUTPUT/'palette.json').write_text(json.dumps({'materials':[{'name':'Prisma_'+k,'color':list(v[0]),'roughness':v[1],'metallic':v[2]} for k,v in palette.items()]},indent=2))
report={'vertices':sum(len(o.data.vertices) for o in [body,sword]),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in [body,sword]),'bones':len(bones),'actions':lengths,'materials':len(palette)}
(SOURCE/'model_report.json').write_text(json.dumps(report,indent=2));print('PRISMA_MODEL_REPORT '+json.dumps(report))

# Studio lights and camera are source/preview only, never exported into the game.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True
scene.render.resolution_x=720;scene.render.resolution_y=880;scene.render.resolution_percentage=100
scene.world.color=(.14,.14,.14);scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
scene.view_settings.view_transform='AgX'
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.015));floor=bpy.context.object;floor.name='StudioFloor'
floor_material=bpy.data.materials.new('StudioFloor');floor_material.diffuse_color=(.09,.105,.10,1);floor.data.materials.append(floor_material)
def area(name,pos,power,size,color):
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size;data.color=color
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);obj.location=pos;obj.rotation_euler=(Vector((0,0,1.15))-obj.location).to_track_quat('-Z','Y').to_euler()
area('Key',(3,-4,5),600,4,(1,.83,.65));area('Fill',(-3,-2,3),400,3,(.65,.81,1));area('Rim',(1,3,4),800,3,(.81,1,.94))
camera_data=bpy.data.cameras.new('PreviewCamera');camera=bpy.data.objects.new('PreviewCamera',camera_data);bpy.context.collection.objects.link(camera)
camera.location=(3,-5.5,2.9);camera.rotation_euler=(Vector((0,-.1,1.10))-camera.location).to_track_quat('-Z','Y').to_euler();camera_data.type='ORTHO';camera_data.ortho_scale=2.9;scene.camera=camera
scene.render.fps=24;scene.frame_start=1;scene.frame_end=48
bpy.context.preferences.filepaths.save_version=0
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            space=area.spaces.active;space.shading.type='SOLID';space.shading.color_type='MATERIAL';space.overlay.show_overlays=False
            space.region_3d.view_location=(0,-.1,1.1);space.region_3d.view_distance=3.85
            space.region_3d.view_rotation=camera.rotation_euler.to_quaternion();space.region_3d.view_perspective='ORTHO'
bpy.ops.object.select_all(action='DESELECT');body.select_set(True);bpy.context.view_layer.objects.active=body
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Prisma.blend'))
scene.render.filepath=str(SOURCE/'Prisma_preview.png');bpy.ops.render.render(write_still=True)
print('PRISMA_EXPORT_READY')
