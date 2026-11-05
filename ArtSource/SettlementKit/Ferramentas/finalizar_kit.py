import bpy,bmesh,json,csv
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(root/'Kit_original_cidade_e_vegetacao.blend'))
catalog=json.loads((root/'Documentacao'/'metricas.json').read_text(encoding='utf8'))
clean=[]
for row in catalog:
 for level in ['LOD0','LOD1']:
  name=row['asset']+'_'+level;o=bpy.data.objects[name]
  bm=bmesh.new();bm.from_mesh(o.data)
  bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000001)
  bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.0000001)
  bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.calc_area()<1e-12],context='FACES_ONLY')
  bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free();o.data.update()
  count=sum(len(p.vertices)-2 for p in o.data.polygons);row['triangulos_'+level]=count
  loc=o.location.copy();o.location=(0,0,0);o.hide_set(False)
  bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
  bpy.ops.export_scene.fbx(filepath=str(root/'Modelos'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=True)
  o.location=loc;o.hide_set(level=='LOD1')
  clean.append({'mesh':name,'triangulos':count,'faces_area_zero':sum(p.area<1e-12 for p in o.data.polygons)})
# Keep the .blend usable as a gallery when opened, without altering game files.
from mathutils import Vector
for area in bpy.context.screen.areas:
 if area.type=='VIEW_3D':
  area.spaces.active.region_3d.view_location=(22,25,0)
  area.spaces.active.region_3d.view_distance=65
  area.spaces.active.region_3d.view_rotation=Vector((1,-1,1)).to_track_quat('Z','Y')
  area.spaces.active.shading.type='MATERIAL'
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(root/'Kit_original_cidade_e_vegetacao.blend'))
with open(root/'Documentacao'/'CATALOGO.csv','w',newline='',encoding='utf-8-sig') as f:
 w=csv.DictWriter(f,fieldnames=catalog[0].keys());w.writeheader();w.writerows(catalog)
(root/'Documentacao'/'metricas.json').write_text(json.dumps(catalog,indent=2,ensure_ascii=False),encoding='utf8')
(root/'Documentacao'/'limpeza_malhas.json').write_text(json.dumps(clean,indent=2),encoding='utf8')
print('MALHAS_FINALIZADAS',len(clean),'FACES_ZERO',sum(r['faces_area_zero'] for r in clean),flush=True)
