"""Original keyed animation set for the supplied Meshy rig. No runtime bone driver.
Run with Blender 4.5 -b -t 2 -P this_file. Original FBX is never overwritten.
"""
import bpy, math, json, sys
from pathlib import Path
from mathutils import Vector, Quaternion, Euler, Matrix

ROOT=Path(__file__).resolve().parents[3]
WARRIOR='--warrior' in sys.argv
OUT=ROOT/('Assets/ForjaDasMares/Art/Characters/EmberHero' if WARRIOR else 'Assets/ForjaDasMares/Art/Characters/SapphireMage')
SOURCE=ROOT/('ArtSource/Characters/EmberHero' if WARRIOR else 'ArtSource/Characters/SapphireMage');SOURCE.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(OUT/('EmberHero.fbx' if WARRIOR else 'SapphireMage.fbx')),use_anim=False)
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
rig.animation_data_clear()
for a in list(bpy.data.actions):bpy.data.actions.remove(a)
bones={b.name.split(':')[-1]:b for b in rig.pose.bones}
for b in bones.values():b.rotation_mode='QUATERNION'
rest={n:b.bone.matrix_local.to_quaternion() for n,b in bones.items()}
def rot(n,x=0,y=0,z=0):
    if n in bones:bones[n].rotation_quaternion=rest[n].conjugated()@Euler((x,y,z),'XYZ').to_quaternion()@rest[n]
def shift(n,v):bones[n].location=rest[n].conjugated()@Vector(v)
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
def bell(t,c=.3):
    # Separate anticipation, snap and delayed recovery; contact at c.
    if t<c*.65:return smooth(t/(c*.65)),0
    if t<c:return 1,smooth((t-c*.65)/(c*.35))
    return 1-smooth((t-c)/(1-c)),1-smooth((t-c)/(1-c))
def solve_leg(side,target):
    bpy.context.view_layer.update()
    thigh=bones[side+'UpLeg'];shin=bones[side+'Leg'];foot=bones[side+'Foot']
    origin=thigh.head.copy();upper=(shin.head-origin).length;lower=(foot.head-shin.head).length
    delta=Vector(target)-origin;distance=max(.001,min(delta.length,upper+lower-.003));direction=delta.normalized()
    pole=Vector((0,0,1));pole=(pole-direction*pole.dot(direction)).normalized()
    along=(upper*upper-lower*lower+distance*distance)/(2*distance)
    knee=origin+direction*along+pole*math.sqrt(max(0,upper*upper-along*along))
    for bone,point in [(thigh,knee),(shin,origin+direction*distance)]:
        bpy.context.view_layer.update();m=bone.matrix.copy();q=(bone.tail-bone.head).rotation_difference(point-bone.head)
        bone.matrix=Matrix.Translation(bone.head)@(q@m.to_quaternion()).to_matrix().to_4x4()
    bpy.context.view_layer.update()
    foot.matrix=Matrix.Translation(foot.head)@rest[side+'Foot'].to_matrix().to_4x4()
    bpy.context.view_layer.update()
base_feet={s:bones[s+'Foot'].head.copy() for s in ['Left','Right']}
def pose(name,t):
    for b in bones.values():b.location=(0,0,0);b.rotation_quaternion=Quaternion();b.scale=(1,1,1)
    wave=math.sin(t*math.tau);breath=math.sin(t*math.tau)
    rot('RightArm',-.10,0,.22);rot('LeftArm',-.12,0,-.20)
    rot('RightForeArm',-.68);rot('LeftForeArm',-.38)
    rot('Spine',.02+breath*.012,breath*.012,breath*.008)
    rot('Head',-.015*breath,-.02*breath)
    targets={s:base_feet[s].copy() for s in base_feet}
    shift('Hips',(wave*.006,-.025+breath*.004,0))
    moving=name in ['Walk','Run']
    if moving:
        running=name=='Run';stance=.28 if running else .60;travel=.68 if running else .46
        shift('Hips',(wave*.014,-.065+abs(wave)*(.032 if running else .012),0))
        for side,sign,phase in [('Left',1,t),('Right',-1,t+.5)]:
            phase%=1
            if phase<stance:forward=travel*.5-travel*phase/stance;lift=0
            else:u=(phase-stance)/(1-stance);forward=-travel*.5+travel*smooth(u);lift=math.sin(u*math.pi)*(.17 if running else .09)
            targets[side]+=Vector((0,lift,forward))
            rot(side+'Arm',-.20+wave*sign*(.22 if running else .12),0,-sign*.19)
            rot(side+'ForeArm',-.72 if side=='Right' else -.48)
        rot('Spine',.10 if running else .04,wave*.045,wave*.025);rot('Head',-.07 if running else -.02,-wave*.025)
    is_cast=name in ['Attack','Attack2','Attack3','Charged','Skill','Super'] or name.startswith(('Staff','Book','Orb','Heavy'))
    if is_cast:
        strong=name in ['Charged','Skill','Super'] or name.startswith('Heavy');c=.5 if strong else .3
        prep,release=bell(t,c);book=name.startswith('Book') or name=='HeavyBook';orb=name.startswith('Orb') or name=='HeavyOrb'
        variant=1 if name.endswith(('Return','Page')) or name=='Attack2' else 2 if name.endswith(('Release','Seal','Burst')) or name=='Attack3' else 0
        side=-1 if variant==1 else 1
        rot('Spine',-.10*prep+.18*release,side*(-.16*prep+.30*release),-.035*prep)
        rot('Spine1',.035*release,side*.08*release);rot('Head',.035*release,-side*.10*prep)
        # Staff drives a forward release; book supports the tome and casts with the free hand.
        rot('RightArm',-.12-(.55 if book else 1.45)*prep+(.15 if book else .95)*release,side*.13*prep,.22-(.10 if book else .20)*release)
        rot('RightForeArm',-.68-(.58 if book else .42)*prep+(.12 if book else .60)*release)
        rot('RightHand',-.08*release,side*.10*prep,.08*prep)
        rot('LeftArm',-.12-(1.80 if book or strong else .70)*prep+.65*release,side*-.18*prep,-.20+.15*prep)
        rot('LeftForeArm',-.38-.80*prep+.73*release);rot('LeftHand',-.24*prep,0,.18*release)
        shift('Hips',(side*.018*prep,-.045-(.065 if strong else .015)*prep,.025*release))
        if variant==2:rot('Spine2',-.05*prep,.15*release)
        if name=='Super':
            rot('RightArm',-1.60*prep+.30*release,0,.22);rot('LeftArm',-1.65*prep+.30*release,0,-.22)
            rot('RightForeArm',-.45*prep);rot('LeftForeArm',-.45*prep);rot('Head',-.18*prep)
    if WARRIOR and (name.startswith(('Sword','Axe','Hammer','Heavy')) or name in ['Attack','Attack2','Attack3','Charged','Skill','Super']):
        heavy=name.startswith('Heavy') or name in ['Charged','Skill'];axe=name.startswith('Axe') or name=='HeavyAxe'
        prep,hit=bell(t,.5 if heavy else .3);reverse=-1 if name.endswith(('Return','Backhand','Back')) or name=='Attack2' else 1
        rot('Spine',-.16*prep+.28*hit,reverse*(-.42*prep+.88*hit));rot('Spine1',.10*hit,reverse*.12*hit);rot('Head',-.06*prep,-reverse*.30*hit)
        rot('RightArm',-.35-.50*prep-.25*hit,.15*prep,.22+reverse*(-.9*prep+1.5*hit));rot('RightForeArm',-.70-.75*prep+.95*hit);rot('RightHand',-.10*prep+.20*hit)
        rot('LeftArm',-.25-.45*prep,0,-.20+reverse*(.25*prep-.45*hit));rot('LeftForeArm',-.55-.35*prep+.2*hit);rot('LeftHand',0)
        if heavy:
            rot('RightArm',-.35-1.75*prep+1.85*hit,0,.22);rot('RightForeArm',-.70-.65*prep+.80*hit)
            rot('LeftArm',-.25-1.35*prep+1.30*hit,0,-.18);rot('LeftForeArm',-.55-.65*prep+.70*hit);rot('Spine',-.22*prep+.38*hit,-.12*prep+.25*hit)
        if name in ['SwordThrust','Attack3']:
            rot('RightArm',-.35-.6*prep-.8*hit,0,.22);rot('RightForeArm',-.7-1.05*prep+1.50*hit);rot('Spine',.22*hit,-.35*prep+.65*hit)
        if name=='AxeSpin':
            rot('Spine',.08,math.tau*smooth((t-.10)/.70));rot('RightArm',-.55,0,1.0*math.sin(t*math.pi));rot('LeftArm',-.25,0,-.55*math.sin(t*math.pi))
        shift('Hips',(reverse*.035*(prep-hit),-.065-.05*prep,.035*hit))
    if name=='Dodge':
        p=math.sin(math.pi*t);shift('Hips',(0,-.19*p,0));rot('Spine',.45*p);rot('Head',-.18*p)
        rot('RightArm',.45*p,0,.22);rot('LeftArm',.40*p,0,-.22)
    if name=='Hit':
        p=math.sin(math.pi*t);rot('Spine',-.27*p,.08*p);rot('Head',-.15*p);shift('Hips',(0,-.04,-.025*p))
    if name=='Victory':
        p=math.sin(math.pi*t)**2;rot('LeftArm',-1.8*p,0,-.20);rot('LeftForeArm',-.5);rot('Head',-.13*p)
    if name=='Dead':
        p=smooth(t/.8);rot('Hips',-1.45*p,0,.12*p);shift('Hips',(0,-.77*p,.12*p));rot('RightArm',.2*p,0,.35*p);rot('LeftArm',.2*p,0,-.4*p)
    else:
        for side,target in targets.items():solve_leg(side,target)
    # Secondary overlap on hair and coat; bounded, subtle, phase delayed.
    for n in bones:
        if any(k in n.lower() for k in ['hair','coat']):rot(n,.025*math.sin(t*math.tau-.7),0,.018*wave)

lengths={'Idle':91,'Walk':31,'Run':25,'Attack':31,'Attack2':31,'Attack3':31,'Charged':43,'Skill':43,'Super':61,'Dodge':19,'Hit':16,'Dead':49,'Victory':91}
for n in (['SwordSlash','SwordReturn','SwordThrust','HeavySword','AxeCleave','AxeBackhand','AxeSpin','HeavyAxe','HammerSweep','HammerBack','HammerSlam','HeavyHammer'] if WARRIOR else ['StaffBolt','StaffReturn','StaffRelease','HeavyStaff','BookWord','BookPage','BookSeal','HeavyBook','OrbCast','OrbReturn','OrbBurst','HeavyOrb']):lengths[n]=43 if n.startswith('Heavy') else 31
rig.animation_data_create();actions={}
for name,length in lengths.items():
    action=bpy.data.actions.new(name);action.use_fake_user=True;rig.animation_data.action=action;actions[name]=action
    for frame in range(1,length+1):
        # Do not let the partially keyed action overwrite the pose during IK updates.
        rig.animation_data.action=None
        pose(name,(frame-1)/(length-1))
        values={n:(b.rotation_quaternion.copy(),b.location.copy()) for n,b in bones.items()}
        rig.animation_data.action=action
        for n,b in bones.items():b.rotation_quaternion=values[n][0];b.location=values[n][1]
        for b in bones.values():
            b.keyframe_insert('rotation_quaternion',frame=frame,group=b.name);b.keyframe_insert('location',frame=frame,group=b.name)
    print('SAPPHIRE_ACTION',name,length,flush=True)
if WARRIOR:
    # Bake the actual running clip supplied with this character onto its identical rig.
    before=set(bpy.data.objects);old_actions=set(bpy.data.actions)
    bpy.ops.import_scene.fbx(filepath=str(OUT/'EmberRunningSource.fbx'),use_anim=True)
    added=set(bpy.data.objects)-before;source_rig=next(o for o in added if o.type=='ARMATURE')
    source_action=source_rig.animation_data.action;first,last=map(int,source_action.frame_range)
    rig.animation_data.action=None;bpy.data.actions.remove(actions['Run']);run=bpy.data.actions.new('Run');run.use_fake_user=True;start=None
    for frame in range(first,last+1):
        bpy.context.scene.frame_set(frame);bpy.context.view_layer.update();rig.animation_data.action=None
        values={n:(source_rig.pose.bones[b.name].rotation_quaternion.copy(),source_rig.pose.bones[b.name].location.copy()) for n,b in bones.items()}
        if start is None:start=values['Hips'][1].copy()
        delta=rest['Hips']@(values['Hips'][1]-start);values['Hips']=(values['Hips'][0],start+rest['Hips'].conjugated()@Vector((0,delta.y,0)))
        rig.animation_data.action=run
        for n,b in bones.items():
            b.rotation_quaternion=values[n][0];b.location=values[n][1];b.keyframe_insert('rotation_quaternion',frame=frame-first+1,group=b.name);b.keyframe_insert('location',frame=frame-first+1,group=b.name)
    actions['Run']=run;lengths['Run']=last-first+1
    for obj in added:bpy.data.objects.remove(obj,do_unlink=True)
    for action in set(bpy.data.actions)-old_actions:
        if action!=run:bpy.data.actions.remove(action)
rig.animation_data.action=actions['Idle'];bpy.context.scene.render.fps=30;bpy.context.scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);mesh.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(OUT/('EmberAnimated.fbx' if WARRIOR else 'SapphireAnimated.fbx')),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,armature_nodetype='NULL',use_armature_deform_only=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y')
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/('EmberAnimated.blend' if WARRIOR else 'SapphireAnimated.blend')),compress=True)
(SOURCE/'animation_report.json').write_text(json.dumps({'clips':lengths,'fps':30,'source':'Original keyframed poses on supplied Meshy skeleton; two-bone foot solves baked into clips.','root_motion':False},indent=2))
print('SAPPHIRE_ANIMATION_EXPORT_READY',flush=True)
