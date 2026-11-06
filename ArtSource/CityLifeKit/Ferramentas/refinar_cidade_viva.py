# Refinement pass: preserves the original 0.29 source and exports an explicitly named active revision.
import bpy,bmesh,math,random,json,csv,os
from pathlib import Path
from mathutils import Vector
BASE=Path(__file__).resolve().parents[1]
source=(BASE/'Ferramentas'/'gerar_cidade_viva.py').read_text(encoding='utf-8-sig').split('atlas=bpy.data.images.new')[0]
exec(compile(source,str(BASE/'Ferramentas'/'gerar_cidade_viva.py'),'exec'),globals())
bpy.ops.wm.open_mainfile(filepath=str(BASE/'Cidade_viva_editavel.blend'))
ROOT=BASE/'Refinado'
for d in ['Modelos','Previews','Documentacao']:(ROOT/d).mkdir(parents=True,exist_ok=True)
mats=[]
for n,c in COLORS:
 m=bpy.data.materials.get(n)
 if m is None:
  m=bpy.data.materials.new(n);m.diffuse_color=c;m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=c
 mats.append(m)
atlasmat=bpy.data.materials['Cidade viva - paleta compartilhada'];scene=bpy.context.scene;cam=scene.camera
records=[];catalog=[];parts=[];os.environ['SKIP_RENDER']='1'
for o in scene.objects:
 if o.type=='MESH':o.hide_render=True

def shop_sign(kind):
 box('Moldura entalhada',(0,0,.75),(1.35,.18,1.3),0,.08)
 box('Painel',(0,-.12,.75),(1.12,.09,1.08),1,.04)
 for x in [-.48,.48]:
  tube('Suspensao',[(x,0,1.38),(x,0,1.72)],[.035,.035],9,6)
 if kind==0:
  # Bread relief across the sign, with distinct segmented scoring.
  tube('Pao em relevo',[(-.4,-.24,.68),(-.25,-.27,.81),(.22,-.27,.81),(.4,-.24,.68)],[.04,.18,.18,.04],23,10)
  for x in [-.18,0,.18]:beam('Corte no pao',(x,-.45,.76),(x+.08,-.42,.88),.015,19)
 elif kind==1:
  tube('Ramo simbolo',[(0,-.24,.3),(0,-.24,.65),(.08,-.24,1.14)],[.025,.025,.01],12,6)
  for s in [-1,1]:
   for j in range(3):
    x=s*.26;z=.48+j*.19
    mesh('Folha em relevo',[(0,-.26,z),(x,-.28,z+.03),(x*1.25,-.26,z+.21),(x*.3,-.31,z+.16)],[(0,1,2,3)],12 if j%2 else 11)
 else:
  beam('Cabo do martelo',(-.28,-.25,.3),(.25,-.25,1.08),.05,2)
  o=box('Cabeca do martelo',(.25,-.25,1.08),(.55,.22,.22),8,.04);o.rotation_euler.y=-.5
  for x in [-.48,.48]:box('Rebite',(x,-.18,.32),(.08,.05,.08),9,.02)
for i,name in enumerate(['Placa_da_padaria','Placa_de_ervas','Placa_da_oficina']):finish(name,lambda i=i:shop_sign(i),'Placa com simbolo modelado, sem texto ou loja ficticia')
# A tiled painted-style atlas, preserving one material/submesh. Procedural surface detail, not AI art.
COLS,ROWS,TILE=6,4,128
W,H=COLS*TILE,ROWS*TILE
pixels=[0.0]*(W*H*4)
for idx,(name,color) in enumerate(COLORS):
 r,g,b,a=color
 if idx==4:r,g,b=.36,.37,.33
 if idx==5:r,g,b=.58,.56,.46
 for y in range(TILE):
  for x in range(TILE):
   u=x/(TILE-1);v=y/(TILE-1)
   noise=((math.sin(x*12.9898+y*78.233+idx*19.4)*43758.5453)%1)-.5
   broad=math.sin(u*9+idx)*math.cos(v*7+idx*.3)
   shade=1+.035*noise+.035*broad
   if idx in [0,1,2]:
    grain=math.sin(u*110+math.sin(v*8)*2+math.sin(v*31)*.15)
    shade+=.075*grain+.035*math.sin(u*49+v*.7)
    knot=math.sqrt(((u-.65)*1.5)**2+((v-.55)*.45)**2)
    if knot<.2:shade+=.10*math.sin(knot*130)
   elif idx in [4,5]:
    shade+=.06*math.sin(u*23+v*9)*math.sin(v*29-u*3)
    crack=abs(math.sin(u*7.1+math.sin(v*6)*.35+idx))
    if crack<.025:shade-=.14
   elif idx in [8,9]:
    shade+=.04*math.sin(v*32+u*4)
    if u<.12 or u>.88 or v<.12:shade-=.10
   elif idx in [16,18,19,20,21]:
    shade+=.018*math.sin(x*math.pi)+.018*math.sin(y*math.pi)+.06*math.cos(u*math.pi*2)
   elif idx in [6,7]:shade+=.055*math.sin(v*15)+.04*broad
   # Soft painted edge wear, not heavy black outlines.
   edge=min(u,1-u,v,1-v)
   if idx not in [10,11,12,13,14,17] and edge<.045:shade+=.055
   px=(idx%COLS)*TILE+x;py=(idx//COLS)*TILE+y;k=(py*W+px)*4
   pixels[k:k+4]=[max(0,min(1,r*shade)),max(0,min(1,g*shade)),max(0,min(1,b*shade)),1]
image=bpy.data.images.new('Cidade_viva_superficies',width=W,height=H);image.pixels=pixels;image.filepath_raw=str(ROOT/'Modelos'/'Paleta_cidade_viva.png');image.file_format='PNG';image.save();image.pack()
for node in atlasmat.node_tree.nodes:
 if node.type=='TEX_IMAGE':node.image=image;node.interpolation='Linear'
# Smooth curved surfaces while keeping architectural edges sharp. No geometry subdivision.
all_high=sorted([o for o in scene.objects if o.type=='MESH' and o.name.endswith('_LOD0')],key=lambda o:o.name)
base_metrics=json.loads((BASE/'Documentacao'/'metricas.json').read_text(encoding='utf-8-sig'))
roles={r['asset']:r['funcao'] for r in base_metrics};roles.update({r['asset']:r['funcao'] for r in catalog});catalog=[]
for index,high in enumerate(all_high):
 name=high.name[:-5];dim=high.dimensions[:];counts=[]
 for level in ['LOD0','LOD1']:
  o=bpy.data.objects[name+'_'+level];o.hide_set(False);uv=o.data.uv_layers.active
  for poly in o.data.polygons:
   old=uv.data[poly.loop_indices[0]].uv.x;idx=min(23,max(0,int(old*24)))
   normal=poly.normal;axis=max(range(3),key=lambda i:abs(normal[i]));axes=[i for i in range(3) if i!=axis]
   coords=[(o.data.vertices[o.data.loops[li].vertex_index].co[axes[0]],o.data.vertices[o.data.loops[li].vertex_index].co[axes[1]]) for li in poly.loop_indices]
   mn=[min(c[i] for c in coords) for i in [0,1]];mx=[max(c[i] for c in coords) for i in [0,1]]
   for li,co in zip(poly.loop_indices,coords):
    p=[(co[i]-mn[i])/max(mx[i]-mn[i],.00001) for i in [0,1]]
    # Generous gutters reduce mip bleed at the game camera's scale.
    uv.data[li].uv=((idx%COLS+.08+p[0]*.84)/COLS,(idx//COLS+.08+p[1]*.84)/ROWS)
  bm=bmesh.new();bm.from_mesh(o.data)
  for f in bm.faces:f.smooth=True
  for e in bm.edges:e.smooth=e.is_manifold and e.calc_face_angle(0)<math.radians(38)
  bm.to_mesh(o.data);bm.free();o.data.update();clean(o)
  counts.append(sum(len(p.vertices)-2 for p in o.data.polygons))
  old_location=o.location.copy();o.location=(0,0,0);bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
  bpy.ops.export_scene.fbx(filepath=str(ROOT/'Modelos'/f'{name}_{level}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=True)
  o.location=old_location;o.hide_render=level=='LOD1';o.hide_set(level=='LOD1')
 catalog.append({'asset':name,'triangulos_LOD0':counts[0],'triangulos_LOD1':counts[1],'largura_m':round(dim[0],2),'profundidade_m':round(dim[1],2),'altura_m':round(dim[2],2),'materiais':1,'funcao':roles[name]})
# Real previews using the refined material, not mockups.
for o in scene.objects:
 if o.type=='MESH':o.hide_render=True
floor=bpy.data.objects.get('Piso de estudio')
if floor:floor.hide_render=False
scene.render.resolution_x=640;scene.render.resolution_y=640;scene.cycles.samples=12
for o in all_high:
 loc=o.location.copy();o.location=(0,0,0);o.hide_render=False;size=max(o.dimensions);target=Vector((0,0,o.dimensions.z*.43));cam.location=target+Vector((1.15,-1.7,1.05))*size;cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=size*1.65
 scene.render.filepath=str(ROOT/'Previews'/f'{o.name[:-5]}.png');bpy.ops.render.render(write_still=True);o.hide_render=True;o.location=loc
for i,o in enumerate(all_high):o.hide_render=False;o.location=((i%5)*14,(i//5)*13,0)
if floor:floor.hide_render=True
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Cidade_viva_refinada.blend'))
with open(ROOT/'Documentacao'/'CATALOGO.csv','w',newline='',encoding='utf-8-sig') as f:
 w=csv.DictWriter(f,fieldnames=catalog[0].keys());w.writeheader();w.writerows(catalog)
(ROOT/'Documentacao'/'metricas.json').write_text(json.dumps(catalog,indent=2,ensure_ascii=False),encoding='utf8')
print('CITY_REFINED',len(catalog),'ATLAS',W,H,flush=True)
