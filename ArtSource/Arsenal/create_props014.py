import bpy,math
from pathlib import Path
root=Path(__file__).resolve().parents[2];out=root/'Assets/ForjaDasMares/Art/Expedition014';out.mkdir(parents=True,exist_ok=True);source=root/'ArtSource/Environment';source.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*color,1);m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.8;return m
wood=material('Bridge_Oak',(.28,.13,.055));edge=material('Bridge_LightWood',(.49,.29,.1));stone=material('Cracked_Stone',(.34,.38,.34));iron=material('Bridge_Iron',(.18,.2,.21))
def cube(name,p,s,m):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m);bevel=o.modifiers.new('WornEdges','BEVEL');bevel.width=.025;bevel.segments=1;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name);return o
def export(name,objects):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',bake_space_transform=True,use_mesh_modifiers=True,add_leaf_bones=False)
 for o in objects:o.hide_set(True)
bridge=[]
for x in [-.9,.9]:bridge.append(cube('LoadBeam', (x,0,.13),(.2,4,.26),wood))
for i in range(12):
 y=-1.85+i*.335;bridge.append(cube('Plank_%02d'%i,(0,y,.27),(2.35,.30,.18),edge if i%3 else wood))
 for x in [-.88,.88]:bridge.append(cube('IronPin',(x,y,.372),(.065,.065,.03),iron))
export('FragileBridge',bridge)
wall=[]
for z in range(5):
 for x in range(4):
  gap=.018;px=-1.13+x*.75+(.14 if z%2 else -.03);wall.append(cube('LooseStone_%d_%d'%(z,x),(px,0,.23+z*.45),(.70,.42,.41),stone))
export('CrackedWall',wall)
for o in bpy.data.objects:o.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=str(source/'Expedition014.blend'))
print('EXPEDITION014_MODELS_READY bridge=',len(bridge),'wall=',len(wall))
