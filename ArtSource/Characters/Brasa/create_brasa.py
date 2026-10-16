"""Original Brasa model, rig, keyed actions, FBX and studio preview. Blender 4.5 LTS."""
import bpy, bmesh, math, json, sys
from pathlib import Path
from mathutils import Vector, Quaternion, Euler

ROOT = Path(__file__).resolve().parents[3]
SOURCE = Path(__file__).resolve().parent
OUTPUT = ROOT / 'Assets/ForjaDasMares/Art/Characters/Brasa'
OUTPUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for action in list(bpy.data.actions): bpy.data.actions.remove(action)
palette = {
 'Skin': ((.43,.225,.105,1),.75,0), 'SkinLight': ((.47,.255,.13,1),.7,0),
 'Hair': ((.025,.016,.01,1),.85,0), 'Ash': ((.12,.15,.13,1),.8,0),
 'Steel': ((.29,.36,.38,1),.36,.72), 'Copper': ((.49,.245,.09,1),.4,.65),
 'Leather': ((.12,.067,.031,1),.9,0), 'Cloth': ((.035,.19,.19,1),.92,0),
 'Dark': ((.035,.045,.04,1),.85,0), 'EyeWhite': ((.85,.78,.61,1),.45,0),
 'Iris': ((.28,.12,.027,1),.35,0), 'Pupil': ((.009,.012,.009,1),.4,0),
 'Ember': ((1,.32,.045,1),.4,.25),
}
materials={}
for name,(color,rough,metal) in palette.items():
    m=bpy.data.materials.new('Brasa_'+name);m.diffuse_color=color;m.use_nodes=True
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

# Torso silhouette and anatomy; soft volumes are actual faceted geometry.
rings('Torso',[(.94,0,0,.245,.145),(1.06,0,0,.27,.15),(1.18,0,0,.29,.16),(1.38,0,0,.385,.18),(1.51,0,0,.40,.17),(1.60,0,.01,.25,.13)],'Skin')
for sign in [-1,1]:
    oval('Pectoral',(.18*sign,-.145,1.39),(.185,.035,.095),'Skin')
    oval('Shoulder',(.405*sign,0,1.47),(.155,.15,.17),'Skin','UpperArm.'+('R' if sign>0 else 'L'))
    for z,width in [(1.25,.105),(1.13,.092)]:oval('Abdominal',(.10*sign,-.147,z),(width,.025,.055),'Skin','Spine')
tube('Neck',(0,0,1.53),(0,-.006,1.76),.113,.116,'Skin','Neck')
rings('Head',[(1.72,0,-.02,.115,.12),(1.78,0,-.01,.175,.15),(1.88,0,0,.215,.17),(2.0,0,0,.226,.183),(2.10,0,.015,.205,.17),(2.18,0,.025,.155,.12),(2.21,0,.03,.075,.06)],'SkinLight','Head',16)
for sign in [-1,1]:
    oval('Ear',(.225*sign,.005,1.94),(.043,.043,.079),'Skin','Head',10,6)
    oval('Ear fold',(.249*sign,-.027,1.942),(.013,.017,.046),'SkinLight','Head',8,5)
    x=.091*sign
    oval('Eye socket',(x,-.165,1.984),(.064,.008,.039),'Skin','Head',12,6)
    oval('Eye white',(x,-.164,1.978),(.047,.012,.028),'EyeWhite','Head',16,8,True)
    oval('Iris',(x,-.177,1.979),(.019,.004,.023),'Iris','Head',12,8,True)
    oval('Pupil',(x,-.181,1.979),(.009,.002,.016),'Pupil','Head',12,8,True)
    oval('Eye glint',(x-.006,-.184,1.986),(.004,.002,.005),'EyeWhite','Head',8,5,True)
    tube('Brow',(sign*.034,-.198,2.034),(sign*.157,-.164,2.05),.017,.022,'Hair','Head',5)
mesh('Nose',[(-.026,-.169,2.00),(.026,-.169,2.00),(-.032,-.191,1.916),(.032,-.191,1.916),(0,-.231,1.93),(0,-.208,1.902)],[(0,1,4),(0,4,2),(1,3,4),(2,4,5),(4,3,5),(0,2,3,1),(2,5,3)],'SkinLight','Head')
tube('Mouth',(-.062,-.168,1.851),(0,-.191,1.842),.004,.005,'Leather','Head',5)
tube('Mouth',(0,-.191,1.842),(.064,-.168,1.855),.005,.003,'Leather','Head',5)
# Short angular beard, cropped mane and a small copper-bound braid.
for sign in [-1,1]:
    mesh('Jaw beard',[(sign*.18,-.11,1.91),(sign*.105,-.167,1.87),(sign*.09,-.183,1.77),(sign*.155,-.116,1.73),(sign*.205,-.065,1.84)],[(0,1,2,3),(0,3,4)],'Hair','Head')
    tube('Moustache',(0,-.207,1.89),(sign*.075,-.178,1.875),.015,.007,'Hair','Head',6)
mesh('Chin beard',[(-.09,-.18,1.80),(.09,-.18,1.80),(.07,-.19,1.72),(0,-.205,1.68),(-.07,-.19,1.72),(0,-.23,1.76)],[(0,1,5),(1,2,5),(2,3,5),(3,4,5),(4,0,5)],'Hair','Head')
oval('Hair crown',(0,.037,2.12),(.232,.188,.132),'Hair','Head',12,6)
for i in range(9):
    x=-.19+i*.046; y=-.055+abs(x)*.23
    a=(x,y,2.17);b=(x-.04,-.12,2.23);c=(x+.045,-.13,2.21);d=(x+.035,-.19,2.04+abs(x)*.15)
    mesh('Swept hair', [a,b,c,d,(x,.02,2.26)],[(0,1,2),(0,2,3),(0,3,1),(1,4,2)],'Ash' if i==2 else 'Hair','Head')
for sign in [-1,1]:
    for z in [2.04,1.99,1.94]:
        mesh('Sideburn',[(sign*.20,-.06,z+.055),(sign*.25,.02,z+.02),(sign*.23,-.045,z-.07)],[(0,1,2)],'Hair','Head')
for i in range(6):oval('Braid',(-.16,.16,1.97-i*.045),(.036,.035,.043),'Hair','Head',8,5)
for z in [1.79,1.94]:tube('Braid clasp',(-.16,.16,z-.014),(-.16,.16,z+.014),.038,.038,'Copper','Head',8)

# Legs, trousers, boots and forged kneecaps.
for sign,side in [(-1,'L'),(1,'R')]:
    upper='Thigh.'+side;lower='Shin.'+side;foot='Foot.'+side
    tube('Trousers',(sign*.16,0,1.01),(sign*.22,0,.54),.153,.117,'Dark',upper,12)
    oval('Knee',(sign*.22,-.005,.54),(.122,.125,.12),'Leather',lower)
    tube('Boot shaft',(sign*.22,.005,.55),(sign*.22,-.005,.14),.118,.082,'Leather',lower,10)
    box('Boot sole',(sign*.22,-.105,.08),(.23,.38,.115),'Dark',foot,.03)
    box('Boot toe',(sign*.22,-.16,.135),(.235,.29,.15),'Steel',foot,.045)
    for z in [.24,.41]:
        tube('Boot wrap',(sign*.22,0,z-.027),(sign*.22,0,z+.027),.107,.11,'Leather',lower,10)
        box('Boot clasp',(sign*.22,-.104,z),(.08,.025,.044),'Copper',lower,.008)
    oval('Knee plate',(sign*.22,-.105,.545),(.127,.055,.145),'Steel',lower,8,5)
    mesh('Knee copper inlay',[(sign*.22-.05,-.157,.56),(sign*.22,-.17,.60),(sign*.22+.05,-.157,.56),(sign*.22,-.16,.49)],[(0,1,3),(1,2,3)],'Copper',lower)

# Arms and hands in a relaxed A pose, with separate visible fingers.
for sign,side in [(-1,'L'),(1,'R')]:
    upper='UpperArm.'+side;lower='Forearm.'+side;hand='Hand.'+side
    shoulder=(sign*.40,0,1.48);elbow=(sign*.61,0,1.17);wrist=(sign*.71,-.025,.94)
    tube('Upper arm',shoulder,elbow,.142,.095,'Skin',upper,12)
    oval('Biceps',(sign*.52,-.045,1.32),(.108,.10,.15),'Skin',upper)
    oval('Elbow',elbow,(.095,.10,.095),'Skin',lower)
    tube('Forearm',elbow,wrist,.106,.073,'SkinLight',lower,10)
    tube('Bracer',(sign*.647,-.01,1.10),(sign*.70,-.024,.97),.106,.083,'Leather',lower,10)
    box('Bracer guard',(sign*.69,-.094,1.045),(.14,.06,.19),'Steel',lower,.018)
    for z,x in [(1.025,.694),(1.09,.665)]:box('Bracer band',(sign*x,-.115,z),(.15,.026,.022),'Copper',lower,.005)
    oval('Hand palm',(sign*.735,-.035,.864),(.080,.055,.10),'Skin',hand,10,6)
    for i in range(4):
        x=sign*(.68+i*.032);z=.80+(i in [0,3])*.013
        tube('Finger',(x,-.054,.88),(x,-.085,z),.017,.014,'SkinLight',hand,6)
        oval('Knuckle',(x,-.083,.867),(.018,.016,.023),'SkinLight',hand,8,5)
    tube('Thumb',(sign*.674,-.04,.89),(sign*.663,-.095,.843),.026,.02,'Skin',hand,7)

# Asymmetric armor: copper-edged right pauldron, leather strap, teal tabard.
oval('Shoulder underlay',(.415,.015,1.515),(.215,.21,.13),'Leather','UpperArm.R',10,5)
oval('Forged shoulder',(.43,-.005,1.56),(.211,.192,.115),'Steel','UpperArm.R',8,5)
for x,z in [(.34,1.634),(.47,1.643),(.58,1.59)]:oval('Shoulder rivet',(x,-.12,z),(.017,.012,.017),'Copper','UpperArm.R',8,5)
for i in range(2):
    box('Pauldron skirt',(.46+i*.045,-.13,1.50-i*.055),(.24,.055,.09),'Copper' if i==0 else 'Steel','UpperArm.R',.017)
strap=box('Crossbody strap',(0,-.194,1.32),(.075,.035,.64),'Leather','Spine',.01);strap.rotation_euler[1]=-.55
oval('Strap clasp',(-.145,-.225,1.55),(.045,.016,.045),'Copper','Spine',8,5)
for sign in [-1,1]:
    cloth=mesh('Tabard',[(sign*.025,-.175,.98),(sign*.255,-.15,.99),(sign*.28,-.16,.63),(sign*.11,-.205,.56),(sign*.04,-.20,.69)],[(0,1,2),(0,2,3),(0,3,4)],'Cloth','Hips')
    mod=cloth.modifiers.new('Cloth thickness','SOLIDIFY');mod.thickness=.014
    bpy.context.view_layer.objects.active=cloth;bpy.ops.object.modifier_apply(modifier=mod.name)
    box('Side leather tasset',(sign*.29,.02,.85),(.095,.255,.29),'Leather','Hips',.02)
tube('Broad belt',(0,0,.99),(0,0,1.085),.29,.29,'Leather','Hips',12)
oval('Belt buckle',(0,-.295,1.04),(.083,.035,.074),'Copper','Hips',8,5)
mesh('Buckle ember',[(-.027,-.333,1.04),(0,-.34,1.078),(.027,-.333,1.04),(0,-.34,1.005)],[(0,1,2),(0,2,3)],'Ember','Hips')
mesh('Back cloth',[(-.22,.15,1.0),(.22,.15,1.0),(.20,.20,.63),(0,.23,.57),(-.20,.20,.68)],[(0,1,2),(0,2,3),(0,3,4)],'Cloth','Hips')

# Original broad blade. Separate skinned object can be replaced by equipment.
guard=Vector((.735,-.12,.80));axis=Vector((.06,-.8,-.6)).normalized();right=Vector((1,0,0));right=(right-axis*right.dot(axis)).normalized();normal=axis.cross(right).normalized()
verts=[]
for distance,width in [(0,.075),(.12,.12),(.78,.095),(1.05,0)]:
    center=guard+axis*distance
    verts.extend([center+right*width,center+normal*.026,center-right*width,center-normal*.026])
faces=[]
for row in range(3):
    for i in range(4):faces.append((row*4+i,row*4+(i+1)%4,(row+1)*4+(i+1)%4,(row+1)*4+i))
mesh('GreatSword',verts,faces,'Steel','Hand.R',True)
tube('Sword grip',guard-axis*.22,guard,.035,.035,'Leather','Hand.R',10,True)
tube('Sword guard',guard-right*.22,guard+right*.22,.04,.028,'Copper','Hand.R',6,True)
oval('Sword pommel',guard-axis*.26,(.055,.055,.055),'Copper','Hand.R',8,5,is_weapon=True)
oval('Guard ember',guard+normal*.055,(.04,.04,.04),'Ember','Hand.R',8,5,is_weapon=True)

# Bind a compact skeleton; armor is rigid-weighted to its corresponding limb.
arm_data=bpy.data.armatures.new('BrasaSkeleton');arm=bpy.data.objects.new('BrasaRig',arm_data);bpy.context.collection.objects.link(arm)
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
for name,(head,tail,parent) in bones.items():
    b=arm_data.edit_bones.new(name);b.head=head;b.tail=tail
    if parent:b.parent=arm_data.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');arm.select_set(False)
# Unite the torso/arm volumes into continuous low-poly anatomy, rather than
# leaving intersecting spheres at the chest and shoulder joints.
skin_names={'Torso','Pectoral','Shoulder','Abdominal','Neck','Upper arm','Biceps','Elbow','Forearm'}
skin_parts=[o for o in parts if o.name.split('.')[0] in skin_names]
parts=[o for o in parts if o not in skin_parts]
bpy.ops.object.select_all(action='DESELECT')
for obj in skin_parts:obj.select_set(True)
bpy.context.view_layer.objects.active=skin_parts[0];bpy.ops.object.join();skin=bpy.context.object;skin.name='UnifiedAnatomy'
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
remesh=skin.modifiers.new('Anatomy union','REMESH');remesh.mode='VOXEL';remesh.voxel_size=.018
bpy.ops.object.modifier_apply(modifier=remesh.name)
smooth=skin.modifiers.new('Anatomy blend','SMOOTH');smooth.factor=.65;smooth.iterations=4;bpy.ops.object.modifier_apply(modifier=smooth.name)
reduce=skin.modifiers.new('Mobile facets','DECIMATE');reduce.ratio=.45;reduce.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=reduce.name)
skin.vertex_groups.clear()
skin_bones={name:skin.vertex_groups.new(name=name) for name in bones if name in {'Hips','Spine','Neck'} or name.startswith(('UpperArm','Forearm'))}
def segment_distance(point,start,end):
    a,b=Vector(start),Vector(end);d=b-a;t=max(0,min(1,(point-a).dot(d)/d.length_squared))
    return (point-(a+d*t)).length
def smooth_range(low,high,value):
    t=max(0,min(1,(value-low)/(high-low)));return t*t*(3-2*t)
# Spatially continuous anatomical weights. Central chest belongs to the spine,
# never to a nearby arm/neck merely because its bone segment is closer.
for vertex in skin.data.vertices:
    x,y,z=vertex.co;side='R' if x>=0 else 'L'
    arm_weight=smooth_range(.29,.45,abs(x))
    forearm=1-smooth_range(1.10,1.25,z)
    hips=1-smooth_range(1.01,1.18,z)
    neck=smooth_range(1.51,1.72,z)*(1-smooth_range(.12,.24,abs(x)))
    weights={'Hips':(1-arm_weight)*hips,
             'Spine':(1-arm_weight)*(1-hips)*(1-neck),
             'Neck':(1-arm_weight)*(1-hips)*neck,
             'UpperArm.'+side:arm_weight*(1-forearm),
             'Forearm.'+side:arm_weight*forearm}
    total=sum(weights.values())
    for name,weight in weights.items():
        if weight>0:skin_bones[name].add([vertex.index],weight/total,'REPLACE')
parts.append(skin)
def join_objects(objects,name):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();obj=bpy.context.object;obj.name=name
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    mod=obj.modifiers.new('Brasa skin','ARMATURE');mod.object=arm;obj.parent=arm
    return obj
body=join_objects(parts,'BrasaBody');sword=join_objects(weapon,'GreatSword')
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
        rotate('Spine',(.012*wave,0,0));rotate('Head',(0,0,.012*wave))
    elif name=='Walk':
        # Analytic two-segment legs: stance foot stays on the floor and only
        # the swinging foot lifts. The knee always bends toward the front.
        hip_z=.98-.04+.004*math.cos(t*math.tau*2)
        arm.pose.bones['Hips'].location.y=hip_z-.98
        for sign,side in [(-1,'L'),(1,'R')]:
            cycle=(t+(0 if side=='L' else .5))%1
            if cycle<.5:
                ankle_y=-.18+.36*cycle*2;ankle_z=.14
            else:
                u=(cycle-.5)*2;ankle_y=.18-.36*ease(u);ankle_z=.14+.09*math.sin(math.pi*u)
            dy,dz=ankle_y,ankle_z-hip_z;distance=math.hypot(dy,dz)
            along=(.44**2-.4**2+distance**2)/(2*distance)
            height=math.sqrt(max(0,.44**2-along**2))
            knee_y=dy/distance*along+dz/distance*height
            knee_z=hip_z+dz/distance*along-dy/distance*height
            thigh=math.atan2(knee_y,hip_z-knee_z)
            shin=math.atan2(ankle_y-knee_y,knee_z-ankle_z)
            rotate('Thigh.'+side,(thigh,0,0));rotate('Shin.'+side,(shin-thigh,0,0));rotate('Foot.'+side,(-shin,0,0))
            swing=wave*sign
            rotate('UpperArm.'+side,(-.18+swing*.22,0,-sign*.08));rotate('Forearm.'+side,(-.22,0,0))
        rotate('Spine',(-.035,0,.035*wave));rotate('Head',(.02,0,-.02*wave))
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

actions={};lengths={'Idle':49,'Walk':25,'Attack':25,'Attack2':25,'Attack3':25,'Charged':33,'Skill':33,'Super':41,'Dodge':19,'Hit':13,'Dead':31,'Victory':49}
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
bpy.ops.export_scene.fbx(filepath=str(OUTPUT/'Brasa.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},
  add_leaf_bones=False,armature_nodetype='NULL',use_armature_deform_only=False,bake_anim=True,
  bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,
  bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',path_mode='AUTO')
(OUTPUT/'palette.json').write_text(json.dumps({'materials':[{'name':'Brasa_'+k,'color':list(v[0]),'roughness':v[1],'metallic':v[2]} for k,v in palette.items()]},indent=2))
report={'vertices':sum(len(o.data.vertices) for o in [body,sword]),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in [body,sword]),'bones':len(bones),'actions':lengths,'materials':len(palette)}
(SOURCE/'model_report.json').write_text(json.dumps(report,indent=2));print('BRASA_MODEL_REPORT '+json.dumps(report))

# Studio lights and camera are source/preview only, never exported into the game.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=900;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
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
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Brasa.blend'))
scene.render.filepath=str(SOURCE/'Brasa_preview.png');bpy.ops.render.render(write_still=True)
print('BRASA_EXPORT_READY')
