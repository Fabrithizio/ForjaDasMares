"""Refine sword/axe actions without rebuilding the stable warrior mesh or locomotion."""
import bpy,math
from pathlib import Path
from mathutils import Euler,Quaternion
ROOT=Path(__file__).resolve().parents[3]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'ArtSource/Arsenal/Brasa_Arsenal.blend'))
rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
def rot(n,x=0,y=0,z=0):
 b=rig.pose.bones.get(n)
 if b:q=b.bone.matrix_local.to_quaternion();b.rotation_quaternion=q.conjugated()@Euler((x,y,z),'XYZ').to_quaternion()@q
def ease(x):x=max(0,min(1,x));return x*x*(3-2*x)
names=['SwordSlash','SwordReturn','SwordThrust','HeavySword','AxeCleave','AxeBackhand','AxeSpin','HeavyAxe']
for name in names:
 old=bpy.data.actions.get(name)
 if old:bpy.data.actions.remove(old)
 action=bpy.data.actions.new(name);action.use_fake_user=True;rig.animation_data.action=action
 heavy=name.startswith('Heavy');axe='Axe' in name;reverse=-1 if name in ['SwordReturn','AxeBackhand'] else 1
 length=49 if heavy else 37;c=.5 if heavy else .3
 for frame in range(1,length+1):
  t=(frame-1)/(length-1);prep=ease(t/(c*.65)) if t<c else 1-ease((t-c)/(1-c));strike=ease((t-c*.65)/(c*.35)) if t<c else 1-ease((t-c)/(1-c))
  follow=math.sin(math.pi*max(0,min(1,(t-c)/.25))) if t>c else 0
  for b in rig.pose.bones:b.rotation_mode='QUATERNION';b.location=(0,0,0);b.rotation_quaternion=Quaternion();b.scale=(1,1,1)
  power=1.16 if axe else 1
  rot('UpperArm.R',-.24-.65*prep+.30*strike,.12*prep,-.10+reverse*power*(-.85*prep+1.65*strike))
  rot('Forearm.R',-.38-.65*prep+.63*strike);rot('Hand.R',-.16*prep+.24*strike+.08*follow,0,-reverse*.15*follow)
  rot('UpperArm.L',-.18-.38*prep+.16*strike,0,.1+reverse*(.30*prep-.55*strike));rot('Forearm.L',-.24-.5*prep+.3*strike)
  rot('Spine',.12*strike+.06*follow,0,reverse*power*(-.34*prep+.70*strike));rot('Head',-.05*prep+.09*follow,0,reverse*(.12*prep-.19*strike))
  if heavy:
   rot('UpperArm.R',-.24-1.85*prep+2.08*strike,.10,-.10+.14*strike);rot('Forearm.R',-.38-.82*prep+.70*strike)
   rot('UpperArm.L',-.18-1.32*prep+1.6*strike,0,.10-.20*prep);rot('Forearm.L',-.24-.80*prep+.65*strike)
   rot('Spine',-.22*prep+.48*strike+.08*follow,0,-.10*prep+.13*strike)
  if name=='SwordThrust':
   rot('UpperArm.R',-.24-.65*prep-.50*strike,.10,-.10+.20*prep-.25*strike);rot('Forearm.R',-.38-1.05*prep+1.25*strike);rot('Spine',.15*strike,0,-.3*prep+.5*strike)
  if name=='AxeSpin':
   rot('Spine',.10*math.sin(t*math.pi),0,math.tau*ease((t-.10)/.65));rot('UpperArm.R',-.55,0,-1.05*math.sin(math.pi*t));rot('UpperArm.L',-.3,0,.4*math.sin(math.pi*t))
  for side in ['L','R']:
   rot('Thigh.'+side,-.18*prep-.10*strike);rot('Shin.'+side,.30*prep+.18*strike);rot('Foot.'+side,-.12*prep-.08*strike)
  rig.pose.bones['Hips'].location.y=-.055*prep-.035*strike
  rig.pose.bones['Hips'].location.x=reverse*(.025*prep-.025*strike)
  for b in rig.pose.bones:b.keyframe_insert('rotation_quaternion',frame=frame,group=b.name);b.keyframe_insert('location',frame=frame,group=b.name)
rig.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT')
for name in [rig.name,'BrasaBody','GreatSword']:bpy.data.objects[name].select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/ForjaDasMares/Art/Characters/Brasa/Brasa.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,armature_nodetype='NULL',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y')
print('WARRIOR_POLISH_READY eightClips meshAndLocomotionPreserved')
