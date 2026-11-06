import bpy, bmesh, math, random, json, csv, os
from pathlib import Path
from mathutils import Vector
# Reuse the existing modeling library, without executing its old asset jobs or touching its outputs.
source=Path(__file__).resolve().parents[2]/'SettlementKit'/'Ferramentas'/'gerar_kit.py'
lib=source.read_text(encoding='utf-8-sig').split('# Atlas with color bands,')[0]
lib=lib.replace(' # A shared palette atlas:', ' clean(o)\n # A shared palette atlas:')
lib=lib.replace("dec.ratio=.45", "dec.ratio=.55")
lib=lib.replace(' lodtris=sum(', ' clean(lod)\n lodtris=sum(')
exec(compile(lib,str(source),'exec'),globals())
for name,color in [('Tecido vinho',(.48,.08,.075,1)),('Tecido creme',(.85,.72,.45,1)),('Tecido verde',(.10,.34,.22,1)),('Tecido violeta',(.30,.11,.44,1)),('Fruta dourada',(.83,.46,.065,1)),('Pao',(.60,.32,.10,1))]:
 COLORS.append((name,color));m=bpy.data.materials.new(name);m.diffuse_color=color;m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=color;mats.append(m)

def clean(o):
 bm=bmesh.new();bm.from_mesh(o.data)
 bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000001)
 bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.0000001)
 bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.calc_area()<1e-12],context='FACES_ONLY')
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free();o.data.update()

def arch(cx,cy,base,r,thick,depth,mat=5,N=14):
 for i in range(N):
  a=i*math.pi/N;b=(i+1)*math.pi/N;verts=[]
  for y in [cy-depth/2,cy+depth/2]:
   for radius,angle in [(r,a),(r+thick,a),(r+thick,b),(r,b)]:verts.append((cx+math.cos(angle)*radius,y,base+math.sin(angle)*radius))
  mesh('Pedra radial',verts,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat if i%4 else 4)

def hoop(n,center,r,t,mat,axis='Y',steps=24):
 pts=[]
 for i in range(steps+1):
  a=i*math.tau/steps;v=Vector((math.cos(a)*r,0,math.sin(a)*r)) if axis=='Y' else Vector((math.cos(a)*r,math.sin(a)*r,0));pts.append(tuple(Vector(center)+v))
 tube(n,pts,[t]*len(pts),mat,6)

def gate():
 for s in [-1,1]:
  x=s*3.7
  box('Fundacao',(x,0,.25),(1.65,2,.5),4,.12)
  box('Pilar aparelhado',(x,0,2.45),(1.2,1.4,4.4),5,.07)
  for z in [1,2.4,4.4]:box('Cinta do pilar',(x,-.02,z),(1.4,1.55,.20),9,.04)
  box('Capitel',(x,0,4.9),(1.65,1.8,.35),4,.08)
  tube('Telhado do pilar',[(x,0,5.05),(x,0,5.6)],[1.05,0],6,4)
  box('Faixa colorida',(x,-.76,3.1),(.55,.06,1.9),16,.025)
 arch(0,0,3.1,3.1,.6,1.2,5,16)
 for x in [-2,-1,0,1,2]:box('Pedra de coroamento',(x,0,6.65-abs(x)*.12),(.8,1.1,.4),4,.08)
 hoop('Selo do portao',(0,-.68,5.95),.35,.05,9)

def doorleaf():
 # Pivot at hinge / ground. Width extends to +X; mirror the other leaf when mounting.
 for i in range(7):box('Prancha arqueada',(.21+i*.43,0,1.95),(.41,.20,3.9-abs(3-i)*.10),1 if i%2 else 0,.025)
 for z in [.6,2.2,3.4]:box('Ferragem',(1.5,-.13,z),(3,.10,.14),8,.02)
 for z in [.7,3.3]:box('Dobradiça',(0,-.15,z),(.18,.20,.45),9,.025)
 hoop('Argola da porta',(2.65,-.21,1.8),.13,.035,9,steps=12)

def challenge():
 # Front-facing open arch. Architectural width 11m; no wall across the entrance.
 for s in [-1,1]:
  box('Torre da estacao',(s*4.2,0,2.7),(2.3,2.2,5.4),4,.12)
  box('Revestimento',(s*4.2,-1.15,2.7),(1.7,.15,4.5),5,.05)
  box('Canal antigo',(s*4.2,-1.26,3),(.12,.05,3.1),13)
  for z in [.4,2.1,4.9]:box('Cinta tecnica',(s*4.2,-1.33,z),(2.3,.18,.2),9,.025)
  tube('Coracao gasto',[(s*4.2,0,5.4),(s*4.2,0,6.3)],[1.4,.9],5,4)
 arch(0,0,2.8,2.8,.75,1.9,5,16)
 hoop('Engrenagem antiga',(0,-1.05,5.9),.65,.10,9)
 for i in range(10):
  a=i*math.tau/10;box('Dente',(math.cos(a)*.8,-1.1,5.9+math.sin(a)*.8),(.18,.16,.3),8,.02)
 for s in [-1,1]:
  tube('Raiz envolvendo a torre',[(s*5.2,-1.35,.04),(s*4.8,-1.4,1),(s*5.1,-1.3,2.5),(s*4.6,-1.3,4.4)],[.26,.22,.14,.04],0,7)
  for j in range(8):leaf((s*(4.8+random.random()*.5),-1.45,random.uniform(.6,4)),.4,.14,random.random()*math.tau,11)
 for x in [-1,0,1]:box('Trilho de entrada',(x,-1.6,.055),(.10,2.2,.10),8)

def stallvariant(kind):
 fabric=[18,19,20,16,21,6][kind]
 for x in [-1.35,1.35]:
  for y in [-.65,.8]:box('Poste entalhado',(x,y,1.5),(.14,.14,3),0,.025)
  beam('Escora',(x,-.65,2.25),(x,.0,2.95),.055,1)
 for y in [-.58,.53]:box('Longarina',(0,y,.65),(2.7,.12,.14),0,.02)
 for i in range(9):box('Tabua da banca',(-1.2+i*.3,0,1.02),(.29,1.4,.12),1 if i%3 else 2,.015)
 for i in range(10):
  x=-1.6+i*.32;verts=[(x,-1.08,2.55),(x+.32,-1.08,2.55),(x+.32,-.25,2.95),(x,-.25,2.95),(x,.98,2.7),(x+.32,.98,2.7)]
  mesh('Toldo curvo',verts,[(0,1,2,3),(3,2,5,4),(3,2,1,0),(4,5,2,3)],fabric if i%2 else 19)
  mesh('Saia do toldo',[(x,-1.08,2.55),(x+.32,-1.08,2.55),(x+.27,-1.10,2.28),(x+.05,-1.10,2.28)],[(0,1,2,3),(3,2,1,0)],fabric)
 box('Placa suspensa',(0,-.84,2.02),(.75,.09,.38),0,.04)
 # A modeled sign symbol, no baked illegible text.
 if kind==0:
  for x in [-.85,0,.85]:
   box('Bandeja',(x,0,1.12),(.72,.9,.12),0,.02)
   for i in range(8):tube('Fruta',[(x+(i%3-1)*.18,(i//3-1)*.2,1.2),(x+(i%3-1)*.18,(i//3-1)*.2,1.43)],[.11,.07],22 if i%2 else 18,7)
 elif kind==1:
  for i in range(9):tube('Pao rustico',[(-1+i%3*.65,-.35+i//3*.35,1.1),(-1+i%3*.65,-.35+i//3*.35,1.37)],[.22,.16],23,8)
 elif kind==2:
  for x in [-.9,0,.9]:
   tube('Vaso de ervas',[(x,0,1.1),(x,0,1.4)],[.19,.24],3,8)
   for i in range(10):leaf((x,0,1.4),.36,.06,i*2.4,11 if i%2 else 12)
 elif kind==3:
  for x in [-.8,0,.8]:
   beam('Cabo ferramenta',(x,-.3,1.1),(x,.3,1.1),.04,0);box('Cabeca ferramenta',(x,.28,1.16),(.45,.16,.15),8,.03)
  for x in [-1.1,1.1]:hoop('Roda de equipamento',(x,.72,1.7),.35,.07,9,steps=12)
 elif kind==4:
  for i in range(5):tube('Rolo de tecido',[(-1+i*.5,-.45,1.2),(-1+i*.5,.4,1.2)],[.17,.17],fabric if i%2 else 16,8)
  for i in range(4):box('Tecido dobrado',(0,.15,1.1+i*.11),(1,.65,.1),19 if i%2 else 21,.02)
 else:
  for i in range(4):
   x=-1+i*.65;tube('Base de reliquia',[(x,0,1.1),(x,0,1.23)],[.22,.22],9,8)
   tube('Fragmento antigo',[(x,0,1.23),(x,0,1.7),(x+.05,0,1.85)],[.12,.15,0],13,5)
 for i in range(3):box('Frente em paineis',(-.87+i*.87,-.66,.6),(.8,.1,.75),fabric if i==1 else 1,.025)

def protector():
 for z,r,h,m in [(0,2.5,.25,4),(.25,2.2,.20,5),(.45,1.75,.18,9)]:ring('Base octogonal',z,r,h,m,16)
 for s in [-1,1]:
  pts=[(s*1.5,0,.6),(s*1.75,0,1.6),(s*1.3,0,3.4),(s*.7,0,4.6),(s*.45,0,5)]
  tube('Braco antigo curvo',pts,[.28,.32,.28,.18,.04],5,6)
  tube('Insercao de bronze',[(x,-.29,z) for x,y,z in pts],[.06,.06,.06,.04,.01],9,5)
 hoop('Mecanismo do nucleo',(0,0,2.9),1.05,.10,9)
 hoop('Anel externo',(0,.05,2.9),1.25,.07,8)
 tube('Cristal do protetor',[(0,0,1.9),(0,0,2.9),(0,0,4)],[0,.40,0],13,6)
 for i in range(8):
  a=i*math.tau/8;box('Insercao da base',(math.cos(a)*1.7,math.sin(a)*1.7,.68),(.18,.18,.05),13,.015)

def eventpavilion():
 for x in [-2.5,2.5]:
  for y in [-1.8,1.8]:box('Pilar do pavilhao',(x,y,1.9),(.22,.22,3.8),0,.04)
 for i in range(14):box('Palco em madeira',(0,-1.8+i*.28,.15),(5.4,.27,.3),1 if i%2 else 2,.02)
 for s in [-1,1]:
  mesh('Cobertura do pavilhao',[(0,-2.2,4.4),(0,2.2,4.4),(s*2.9,2.2,3.6),(s*2.9,-2.2,3.6)],[(0,1,2,3),(3,2,1,0)],18)
  box('Viga decorativa',(s*2.5,0,3.5),(.16,4.2,.2),9,.03)
 box('Painel para anuncio',(0,1.85,2.35),(2.3,.14,1.4),16,.05)

def noticeboard():
 for x in [-1,1]:box('Suporte',(x,0,1.25),(.18,.18,2.5),0,.03)
 box('Moldura do quadro',(0,0,1.55),(2.4,.22,1.65),1,.06);box('Fundo',(0,-.14,1.55),(2.1,.04,1.35),0,.02)
 for i in range(6):
  o=box('Aviso de papel',(-.7+i%3*.7,-.18,1.2+i//3*.65),(.48,.025,.5),19,.01);o.rotation_euler.y=random.uniform(-.08,.08)
  box('Selo',(-.7+i%3*.7,-.21,1.1+i//3*.65),(.1,.03,.1),18)
 for s in [-1,1]:mesh('Telhado',[(-1.35,0,2.7),(1.35,0,2.7),(1.35,s*.45,2.4),(-1.35,s*.45,2.4)],[(0,1,2,3),(3,2,1,0)],6)

def pennants():
 for x in [-3.5,3.5]:tube('Poste',[(x,0,0),(x,0,3.8)],[.1,.07],0,6)
 pts=[(-3.5+i*.5,0,3.8-.5*math.sin(i/14*math.pi)) for i in range(15)];tube('Corda',pts,[.018]*len(pts),2,5)
 for i in range(1,14):
  x,y,z=pts[i];mesh('Bandeirola',[(x-.18,0,z),(x+.18,0,z),(x,0,z-.6)],[(0,1,2),(2,1,0)],18 if i%2 else 19)

def ancientroot():
 for s in [-1,1]:box('Maquina partida',(s*.8,0,.8),(.6,1.5,1.6),5,.1)
 hoop('Peca enterrada',(0,0,1.1),.65,.12,9)
 for i in range(5):
  a=i*1.4;tube('Raiz colonizadora',[(math.cos(a)*2,math.sin(a)*1.4,.05),(math.cos(a),math.sin(a)*.5,.4),(0,0,1.65)],[.06,.16,.12],0,7)
  for j in range(4):leaf((math.cos(a)*1.1,math.sin(a)*.8,.4),.45,.15,a+j,11 if j%2 else 12)

def flowerarch():
 for s in [-1,1]:tube('Tronco do arco',[(s*1.6,0,0),(s*1.7,0,1.5),(s*1.1,0,2.8),(0,0,3.3)],[.18,.15,.1,.05],0,7)
 for i in range(26):
  a=i*math.pi/25;p=(math.cos(a)*1.65,0,1.6+math.sin(a)*1.65)
  for j in range(3):leaf(p,.35,.12,i+j*2,11 if j else 14)

atlas=bpy.data.images.new('Paleta_cidade_viva',width=len(mats)*8,height=8);pixels=[]
for y in range(8):
 for x in range(len(mats)*8):pixels.extend(COLORS[x//8][1])
atlas.pixels=pixels;atlas.filepath_raw=str(ROOT/'Modelos'/'Paleta_cidade_viva.png');atlas.file_format='PNG';atlas.save();atlas.pack()
atlasmat=bpy.data.materials.new('Cidade viva - paleta compartilhada');atlasmat.use_nodes=True
nd=atlasmat.node_tree.nodes.new('ShaderNodeTexImage');nd.image=atlas;nd.interpolation='Closest';atlasmat.node_tree.links.new(nd.outputs['Color'],atlasmat.node_tree.nodes['Principled BSDF'].inputs['Base Color']);atlasmat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.8
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.render.engine='CYCLES';scene.cycles.samples=12;scene.cycles.use_denoising=True
scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=100;scene.world.color=(.25,.25,.25)
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam
for loc,power,size in [((4,-6,10),1700,7),((-4,-2,7),1300,6),((2,5,8),2000,5)]:
 bpy.ops.object.light_add(type='AREA',location=loc);l=bpy.context.object;l.data.energy=power;l.data.shape='DISK';l.data.size=size;l.rotation_euler=(-l.location).to_track_quat('-Z','Y').to_euler()
parts=[];floor=box('Piso de estudio',(0,0,-.2),(200,200,.2),4);parts=[]
jobs=[('Portao_da_cidade',gate,'Arco aberto 6.2m; sem portas embutidas'),('Folha_portao',doorleaf,'Pivo na dobradica, modelo separado'),('Entrada_estacao_inv adida'.replace(' ',''),challenge,'Fachada aberta do desafio; raizes e mecanismo'),('Protetor_da_praca',protector,'Monumento visual; sem logica de protecao'),('Pavilhao_de_eventos',eventpavilion,'Estrutura aberta de evento, sem comercio'),('Quadro_de_avisos',noticeboard,'Quadro de NPC/missoes; texto de runtime separado'),('Bandeirolas_de_rua',pennants,'Decoracao sobre caminho; vao livre'),('Raizes_na_maquina',ancientroot,'Natureza envolvendo tecnologia'),('Arco_do_jardim',flowerarch,'Passagem de jardim')]
for i,name in enumerate(['Banca_de_frutas','Banca_do_padeiro','Banca_de_ervas','Banca_de_ferramentas','Banca_de_tecidos','Banca_de_reliquias']):jobs.append((name,lambda i=i:stallvariant(i),'Banca decorativa identificavel; nenhum comercio ficticio'))
for name,fn,role in jobs:finish(name,fn,role)
floor.hide_render=True
for i,o in enumerate(records):o.hide_render=False;o.location=((i%4)*15,(i//4)*13,0)
for area in bpy.context.screen.areas:
 if area.type=='VIEW_3D':area.spaces.active.region_3d.view_location=(20,20,0);area.spaces.active.region_3d.view_distance=70
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Cidade_viva_editavel.blend'))
with open(ROOT/'Documentacao'/'CATALOGO.csv','w',newline='',encoding='utf-8-sig') as f:
 w=csv.DictWriter(f,fieldnames=catalog[0].keys());w.writeheader();w.writerows(catalog)
(ROOT/'Documentacao'/'metricas.json').write_text(json.dumps(catalog,indent=2,ensure_ascii=False),encoding='utf8')
print('CITY_LIFE_COMPLETE',len(catalog),flush=True)
