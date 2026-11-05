import bpy,bmesh
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(root/'Kit_original_cidade_e_vegetacao.blend'))
for name in ['Casa_do_artesao','Casa_do_herbalista']:
 for level in ['LOD0','LOD1']:
  o=bpy.data.objects[name+'_'+level];bm=bmesh.new();bm.from_mesh(o.data);unseen=set(bm.verts)
  while unseen:
   seed=unseen.pop();todo=[seed];component={seed}
   while todo:
    v=todo.pop()
    for e in v.link_edges:
     other=e.other_vert(v)
     if other in unseen:unseen.remove(other);component.add(other);todo.append(other)
   xs=[v.co.x for v in component];ys=[v.co.y for v in component];zs=[v.co.z for v in component]
   if max(zs)<.5 and max(xs)-min(xs)<.6 and max(ys)<-2:
    for v in component:v.co.y-=.07
  bm.to_mesh(o.data);bm.free()
  loc=o.location.copy();o.location=(0,0,0);o.hide_set(False)
  bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
  bpy.ops.export_scene.fbx(filepath=str(root/'Modelos'/f'{name}_{level}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=True)
  o.location=loc;o.hide_set(level=='LOD1')
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(root/'Kit_original_cidade_e_vegetacao.blend'))
print('FUNDACOES_CORRIGIDAS')
