import bpy, bmesh, math, random, json, csv, os
from mathutils import Vector
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
for d in ['Modelos','Previews','Documentacao']: (ROOT/d).mkdir(parents=True,exist_ok=True)
random.seed(51126)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
COLORS=[('Madeira escura',(0.13,.065,.035,1)),('Madeira mel',(.38,.20,.085,1)),('Madeira clara',(.58,.35,.15,1)),('Reboco quente',(.65,.54,.36,1)),('Pedra',(.30,.34,.34,1)),('Pedra clara',(.48,.51,.47,1)),('Telha azul',(.055,.22,.25,1)),('Telha clara',(.09,.32,.34,1)),('Metal',(.12,.16,.18,1)),('Bronze',(.57,.34,.09,1)),('Folha escura',(.035,.16,.095,1)),('Folha verde',(.09,.30,.15,1)),('Folha clara',(.24,.43,.19,1)),('Magia turquesa',(.10,.72,.73,1)),('Flor violeta',(.43,.20,.63,1)),('Sombra janela',(.035,.065,.075,1)),('Tecido',(.19,.34,.37,1)),('Agua',(.07,.39,.48,1))]
mats=[]
for n,c in COLORS:
 m=bpy.data.materials.new(n); m.diffuse_color=c; m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=c; p.inputs['Roughness'].default_value=.82
 if n=='Magia turquesa': p.inputs['Emission Color'].default_value=c; p.inputs['Emission Strength'].default_value=.5
 mats.append(m)
parts=[]; records=[]; catalog=[]
def mesh(n,v,f,mat):
 # Opposite coincident triangles create self-shadow artifacts. Give sheet geometry real thickness.
 unique=[];seen=set();sheet=False
 for face in f:
  key=tuple(sorted(face))
  if key in seen:sheet=True;continue
  seen.add(key);unique.append(face)
 me=bpy.data.meshes.new(n); me.from_pydata(v,[],unique); me.update(); ob=bpy.data.objects.new(n,me); bpy.context.collection.objects.link(ob); ob.data.materials.append(mats[mat]); parts.append(ob)
 if sheet:
  bpy.context.view_layer.objects.active=ob
  mod=ob.modifiers.new('Espessura real','SOLIDIFY');mod.thickness=.008
  bpy.ops.object.modifier_apply(modifier=mod.name)
 bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(ob.data);bm.free()
 return ob
def box(n,p,s,mat,bev=0):
 x,y,z=p; a,b,c=[q/2 for q in s]
 o=mesh(n,[(dx*a,dy*b,dz*c) for dx,dy,dz in [(-1,-1,-1),(-1,-1,1),(-1,1,-1),(-1,1,1),(1,-1,-1),(1,-1,1),(1,1,-1),(1,1,1)]],[(0,4,6,2),(1,3,7,5),(0,1,5,4),(2,6,7,3),(0,2,3,1),(4,5,7,6)],mat)
 o.location=(x,y,z)
 if bev:
  mod=o.modifiers.new('Bordas trabalhadas','BEVEL'); mod.width=bev; mod.segments=1
  bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
def tube(n,points,radii,mat,sides=8):
 v=[]
 for i,p in enumerate(points):
  tangent=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(i-1,0)])
  tangent.normalize(); a=tangent.cross(Vector((0,0,1)))
  if a.length<.01:a=Vector((1,0,0))
  a.normalize(); b=tangent.cross(a).normalized()
  for j in range(sides):v.append(tuple(Vector(p)+radii[i]*(math.cos(j*math.tau/sides)*a+math.sin(j*math.tau/sides)*b)))
 f=[tuple(range(sides-1,-1,-1))]
 for i in range(len(points)-1):
  for j in range(sides): k=i*sides+j; q=i*sides+(j+1)%sides; f.append((k,q,q+sides,k+sides))
 f.append(tuple((len(points)-1)*sides+j for j in range(sides)))
 return mesh(n,v,f,mat)
def beam(n,a,b,w,mat):return tube(n,[a,b],[w,w],mat,4)
def leaf(p,length,width,angle,mat=11):
 p=Vector(p); d=Vector((math.cos(angle),math.sin(angle),.40)); side=Vector((-math.sin(angle),math.cos(angle),0))*width
 v=[p,p+d*length*.45+side,p+d*length+Vector((0,0,-.12*length)),p+d*length*.45-side,p+d*length*.45+Vector((0,0,.12*length))]
 return mesh('Folha dobrada',[tuple(q) for q in v],[(0,1,4),(1,2,4),(2,3,4),(3,0,4),(4,1,0),(4,2,1),(4,3,2),(4,0,3)],mat)
def ring(n,z,r,h,mat,sides=12):return tube(n,[(0,0,z),(0,0,z+h)],[r,r],mat,sides)
def foliage(center,count=75,spread=1.2):
 for i in range(count):
  a=random.random()*math.tau; r=math.sqrt(random.random())*spread
  p=(center[0]+math.cos(a)*r,center[1]+math.sin(a)*r,center[2]+random.uniform(-.3,.3))
  leaf(p,random.uniform(.28,.55),.11,a,random.choice([10,11,11,12]))
def bush(variant):
 for i in range(7):
  a=i*math.tau/7; end=(math.cos(a)*.55,math.sin(a)*.55,.55+random.random()*.3)
  tube('Ramo',[(0,0,0),(.1*math.cos(a),.1*math.sin(a),.3),end],[.05,.035,.012],0,6)
  foliage(end,18,.36)
 if variant==1:
  for i in range(12):
   a=i*math.tau/12; leaf((math.cos(a)*.5,math.sin(a)*.5,.7),.22,.07,a,14)
def tree(v):
 tube('Tronco retorcido',[(0,0,0),(-.15,.1,1),(.18,0,2.1),(.05,.15,3.1)],[.40,.30,.22,.12],0,10)
 for i in range(7):
  a=i*math.tau/7
  tube('Raiz',[(math.cos(a)*.95,math.sin(a)*.95,.04),(math.cos(a)*.4,math.sin(a)*.4,.18),(0,0,.5)],[.025,.13,.21],0,6)
 for i in range(9):
  a=i*2.4; z=1.5+i*.17; end=(math.cos(a)*(1.2+i*.045),math.sin(a)*(1.2+i*.045),z+.9)
  tube('Galho tortuoso',[(.1,0,z),(end[0]*.55,end[1]*.55,z+.5),end],[.15,.09,.025],0,7)
  foliage(end,65,.8)
  if v==1:
   for j in range(3):leaf((end[0],end[1],end[2]-.15-j*.15),.28,.08,a+j,13)
def fern():
 for i in range(9):
  a=i*math.tau/9; end=(math.cos(a)*.9,math.sin(a)*.9,.45)
  tube('Haste',[(0,0,.02),(end[0]*.5,end[1]*.5,.55),end],[.015,.012,.004],11,5)
  for j in range(1,9):
   t=j/9; p=(end[0]*t,end[1]*t,math.sin(t*math.pi)*.5)
   for sign in [-1,1]:leaf(p,.3*(1-t)+.05,.06,a+sign*1.1,12 if j%3==0 else 11)
def grass():
 for i in range(32):
  a=random.random()*math.tau; r=random.random()*.35; leaf((math.cos(a)*r,math.sin(a)*r,0),random.uniform(.25,.6),.025,a,random.choice([11,12]))
def flowers():
 grass()
 for i in range(9):
  a=i*2.4; x=math.cos(a)*.3; y=math.sin(a)*.3; z=random.uniform(.35,.65)
  tube('Haste floral',[(x,y,0),(x+.04,y,z)],[.012,.008],11,5)
  for j in range(5):leaf((x+.04,y,z),.16,.065,j*math.tau/5,14)
def mushroom():
 for x,y,h in [(0,0,.7),(.4,.25,.4),(-.3,.2,.28)]:
  tube('Pe do cogumelo',[(x,y,0),(x+.04,y,h)],[h*.12,h*.08],5,8)
  v=[(x,y,h+.2*h)]; N=12
  for r,z in [(h*.32,h+.12*h),(h*.50,h-.06*h),(h*.44,h-.12*h)]:
   v +=[(x+math.cos(j*math.tau/N)*r,y+math.sin(j*math.tau/N)*r,z) for j in range(N)]
  f=[(0,1+j,1+(j+1)%N) for j in range(N)]
  for k in range(2):f +=[(1+k*N+j,1+k*N+(j+1)%N,1+(k+1)*N+(j+1)%N,1+(k+1)*N+j) for j in range(N)]
  mesh('Chapeu esculpido',v,f,14)
def window(x,y,z):
 box('Recesso da janela',(x,y,z),(1.0,.12,1.15),15,.04)
 for xx in [-.52,.52]:box('Batente',(x+xx,y-.08,z),(.10,.16,1.3),0,.025)
 for zz in [-.62,.62,0]:box('Travessa',(x,y-.09,z+zz),(1.15,.16,.08),1,.02)
 box('Montante',(x,y-.10,z),(.07,.17,1.18),9,.01)
 box('Peitoril',(x,y-.18,z-.67),(1.3,.4,.12),5,.03)
def door(x,y,z):
 for i in range(7):box('Tabua da porta',(x+(i-3)*.14,y,z+.95),(.134,.13,1.9),1 if i%2 else 2,.018)
 for xx in [-.57,.57]:box('Batente de pedra',(x+xx,y+.03,z+1.05),(.20,.3,2.1),5,.035)
 box('Lintel',(x,y+.03,z+2.15),(1.4,.32,.24),5,.04)
 for zz in [.35,1.45]:box('Ferragem',(x,y-.09,z+zz),(1,.06,.10),8,.015)
 tube('Puxador',[(x+.32,y-.12,z+.9),(x+.32,y-.23,z+.9)],[.065,.065],9,8)
def house(kind):
 W=5.6; D=4.3; H=3.1
 box('Embasamento',(0,0,.22),(W+.25,D+.25,.44),4,.10)
 box('Alvenaria',(0,0,1.73),(W,D,2.9),3,.10)
 for row in range(2):
  for i in range(12):box('Pedra de fundacao',(-2.65+i*.48,-D/2-.10,.15+row*.22),(.45,.19,.20),random.choice([4,5]),.025)
 for x in [-2.65,0,2.65]:
  for y in [-2.18,2.18]:box('Pilar de madeira',(x,y,1.8),(.20,.22,3.15),0,.04)
 for z in [.55,2.9]:box('Viga frontal',(0,-2.2,z),(5.6,.20,.18),0,.035)
 for x in [-1.35,1.35]:
  beam('Contraventamento',(x-.55,-2.2,2.0),(x+.55,-2.2,2.9),.065,1)
 # front and rear gables
 for y in [-D/2,D/2]:mesh('Oitao',[(-W/2,y,H),(W/2,y,H),(0,y,H+1.85)],[(0,1,2),(2,1,0)],3)
 # individual curved, overlapping shingle strips, not a flat roof plate
 for side in [-1,1]:
  for row in range(7):
   x0=side*row*.45; x1=side*(row+1)*.45
   def roofz(x):return H+1.9-abs(x)*.56-.08*(abs(x)/3)**2
   for col in range(12):
    y=-2.65+col*.46+(row%2)*.05
    z0=roofz(x0); z1=roofz(x1)
    vs=[(x0,y,z0),(x0,y+.48,z0),(x1,y+.48,z1),(x1,y,z1),(x0,y,z0-.06),(x0,y+.48,z0-.06),(x1,y+.48,z1-.06),(x1,y,z1-.06)]
    mesh('Telha individual',vs,[(0,1,2,3),(4,7,6,5),(0,4,5,1),(3,2,6,7),(1,5,6,2),(0,3,7,4)],random.choice([6,6,7]))
 for y in [-2.7,2.7]:
  beam('Testeira esquerda',(-3.2,y,3.08),(0,y,5.04),.09,0);beam('Testeira direita',(0,y,5.04),(3.2,y,3.08),.09,0)
 tube('Cumeeira',[(0,-2.8,5.02),(0,2.8,5.02)],[.12,.12],6,8)
 door(0,-2.28,.42);window(-1.65,-2.23,1.85);window(1.65,-2.23,1.85)
 for i in range(3):box('Degrau',(0,-2.55-i*.28,.32-i*.1),(1.6,.40,.18),5,.03)
 if kind==1:
  box('Chamine',(1.45,.7,4.15),(.75,.75,2.0),4,.06)
  for z in [4.3,4.65,5]:box('Junta chamine',(1.45,.7,z),(.79,.79,.06),5,.01)
  box('Chapeu chamine',(1.45,.7,5.2),(1,1,.16),5,.04)
 if kind==2:
  for x in [-1.65,1.65]:
   box('Floreira',(x,-2.55,1.13),(1.2,.48,.30),1,.04)
   for j in range(9):leaf((x+random.uniform(-.5,.5),-2.55,1.3),.25,.08,random.random()*6.28,11 if j%2 else 14)
def bench():
 for i in range(4):box('Tabua assento',(0,(i-1.5)*.13,.52),(1.9,.12,.09),2,.025)
 for x in [-.7,.7]:
  for y in [-.16,.16]:box('Pe',(x,y,.26),(.13,.13,.52),0,.02)
  beam('Suporte encosto',(x,.2,.3),(x,.32,1.2),.055,0)
 for z in [.83,1.08]:box('Tabua encosto',(0,.31,z),(1.9,.1,.19),1,.025)
def fence():
 for x in [-1.2,1.2]:
  box('Mourao',(x,0,.65),(.18,.18,1.3),0,.025)
  tube('Ponta talhada',[(x,0,1.3),(x,0,1.45)],[.12,0],1,4)
 for z in [.42,.97]:box('Travessa',(0,0,z),(2.5,.12,.14),1,.025)
 for x in [-.8,-.4,0,.4,.8]:box('Ripa',(x,.02,.64),(.10,.10,.85),2,.018)
def lamp():
 box('Base',(0,0,.12),(.5,.5,.24),4,.05)
 tube('Poste',[(0,0,.2),(0,0,2.3),(.25,0,2.5),(.55,0,2.4)],[.10,.085,.07,.06],8,8)
 box('Vidro da lanterna',(.55,0,2.05),(.32,.32,.48),13,.025)
 for x in [.36,.74]:
  for y in [-.19,.19]:beam('Grade',(x,y,1.78),(x,y,2.32),.024,9)
 box('Base lanterna',(.55,0,1.78),(.45,.45,.08),8,.03)
 mesh('Telhado lanterna',[(.25,-.3,2.34),(.85,-.3,2.34),(.85,.3,2.34),(.25,.3,2.34),(.55,0,2.60)],[(0,1,4),(1,2,4),(2,3,4),(3,0,4),(3,2,1,0)],8)
def well():
 for row in range(4):
  for j in range(14):
   a=(j+row%2*.5)*math.tau/14
   o=box('Bloco do poco',(math.cos(a)*.8,math.sin(a)*.8,.14+row*.23),(.33,.30,.22),4 if j%3 else 5,.03);o.rotation_euler.z=a+math.pi/2
 for x in [-1,1]:box('Suporte',(x,0,1.2),(.15,.18,2.4),0,.025)
 tube('Eixo',[(-1.15,0,1.55),(1.15,0,1.55)],[.10,.10],1,10)
 tube('Corda',[(0,0,1.55),(0,0,.3)],[.025,.025],2,6)
 for side in [-1,1]:mesh('Cobertura do poco',[(0,-.8,2.7),(0,.8,2.7),(side*1.3,.8,2.1),(side*1.3,-.8,2.1)],[(0,1,2,3),(3,2,1,0)],6)
def fountain():
 for j in range(20):
  a=j*math.tau/20; o=box('Borda da fonte',(math.cos(a)*1.1,math.sin(a)*1.1,.25),(.37,.25,.5),5,.03);o.rotation_euler.z=a+math.pi/2
 ring('Agua',.12,1.06,.035,17,24)
 tube('Coluna esculpida',[(0,0,.12),(0,0,.5),(0,0,.7),(0,0,1.1)],[.4,.25,.32,.12],4,10)
 tube('Cristal central',[(0,0,1.0),(0,0,1.3),(0,0,1.8)],[0,.23,0],13,6)
def sign():
 box('Poste',(0,0,.9),(.18,.18,1.8),0,.03)
 for z in [1.2,1.48]:box('Placa talhada',(.25,0,z),(1.25,.17,.24),1,.04)
 for z in [1.2,1.48]:beam('Ornamento',(-.2,-.10,z),(.65,-.10,z),.016,9)
def planter():
 tube('Vaso talhado',[(0,0,0),(0,0,.08),(0,0,.55),(0,0,.62)],[.29,.32,.43,.46],5,10)
 foliage((0,0,.8),85,.55)
def console():
 box('Base antiga',(0,0,.12),(1.4,.9,.24),4,.06)
 tube('Suporte facetado',[(0,0,.24),(0,0,.8)],[.5,.35],8,6)
 o=box('Painel',(0,0,.9),(1.25,.75,.15),8,.05);o.rotation_euler.x=.25
 for x in [-.43,0,.43]:
  tube('Cristal de controle',[(x,0,1.0),(x,0,1.16),(x,0,1.36)],[0,.09,0],13,5)
 for x in [-.5,.5]:box('Filete de bronze',(x,-.16,1.02),(.035,.45,.025),9)
def marker():
 tube('Marco',[(0,0,0),(0,0,.2),(0,0,1.3),(0,0,1.5)],[.35,.4,.24,.10],4,6)
 for z in [.65,.85,1.05]:box('Gravura luminosa',(0,-.235,z),(.20,.025,.025),13)
def cart():
 for i in range(6):box('Assoalho',((i-2.5)*.19,0,.65),(.18,1.65,.10),2,.02)
 for x in [-.62,.62]:
  for z in [.84,1.07]:box('Lateral',(x,0,z),(.10,1.7,.18),1,.02)
 for x in [-.8,.8]:
  pts=[(x,math.cos(i*math.tau/16)*.43,.48+math.sin(i*math.tau/16)*.43) for i in range(17)]
  tube('Aro da roda',pts,[.055]*17,0,6)
  for i in range(8):beam('Raio',(x,0,.48),(x,math.cos(i*math.tau/8)*.43,.48+math.sin(i*math.tau/8)*.43),.022,2)
 for x in [-.4,.4]:beam('Braco',(x,.5,.65),(x,2.25,.4),.05,0)
def stall():
 for x in [-1.2,1.2]:
  for y in [-.55,.55]:box('Poste',(x,y,1.15),(.12,.12,2.3),0,.02)
 for i in range(9):box('Balcao',((i-4)*.28,0,.9),(.26,1.3,.13),2,.025)
 for i in range(9):
  x=(i-4)*.29
  mesh('Toldo curvo',[(x-.15,-.85,1.95),(x+.15,-.85,1.95),(x+.15,0,2.55),(x-.15,0,2.55),(x-.15,.85,1.95),(x+.15,.85,1.95)],[(0,1,2,3),(3,2,5,4),(3,2,1,0),(4,5,2,3)],16 if i%2 else 3)
def wall():
 box('Reboco',(0,0,1.5),(2.8,.25,3),3,.035)
 for x in [-1.35,1.35]:box('Viga',(x,0,1.5),(.18,.36,3),0,.025)
 for z in [.15,2.85]:box('Travessa',(0,0,z),(2.8,.36,.18),1,.025)
 beam('Diagonal',(-1.25,-.2,.3),(1.25,-.2,2.7),.07,0)
def logpile():
 for x,z in [(-.35,.20),(.05,.20),(.45,.20),(-.15,.55),(.25,.55),(.05,.9)]:
  tube('Lenha',[(x,-.6,z),(x+.04,.65,z)],[.19,.17],0,9)
  tube('Corte da lenha',[(x,-.615,z),(x,-.625,z)],[.14,.14],2,9)
def basket():
 for j in range(16):
  a=j*math.tau/16
  tube('Trama vertical',[(math.cos(a)*.25,math.sin(a)*.25,.05),(math.cos(a)*.4,math.sin(a)*.4,.55)],[.025,.025],1,5)
 for z in [.08,.17,.26,.35,.44,.55]:
  r=.25+(z/.55)*.15
  tube('Trama horizontal',[(math.cos(i*math.tau/20)*r,math.sin(i*math.tau/20)*r,z) for i in range(21)],[.018]*21,2,5)
 ring('Fundo',.02,.25,.03,1,16)
def altar():
 box('Pedestal',(0,0,.12),(1.5,1.0,.24),4,.06)
 box('Corpo',(0,0,.55),(1.2,.7,.7),5,.06)
 box('Mesa',(0,0,.96),(1.6,1.1,.18),4,.05)
 for x in [-.55,.55]:tube('Cristal',[(x,0,1.04),(x,0,1.28),(x,0,1.65)],[0,.14,0],13,5)
 for x in [-.4,0,.4]:box('Gravura',(x,-.36,.6),(.04,.02,.35),9)
def stump():
 tube('Toco irregular',[(0,0,0),(.08,0,.3),(0,0,.8)],[.48,.36,.33],0,10)
 ring('Corte',.8,.30,.015,2,10)
 for i in range(5):
  a=i*math.tau/5;tube('Raiz',[(math.cos(a)*.7,math.sin(a)*.7,.02),(0,0,.35)],[.025,.17],0,6)
def bridge():
 for i in range(15):box('Prancha',(0,(i-7)*.25,.14+math.sin(i/14*math.pi)*.12),(1.8,.23,.12),1 if i%3 else 2,.02)
 for x in [-.92,.92]:
  for y in [-1.8,0,1.8]:box('Estaca',(x,y,.65),(.14,.14,1.3),0,.025)
  tube('Corrimao',[(x,-1.9,1.1),(x,0,.95),(x,1.9,1.1)],[.035,.035,.035],2,7)
def stairs():
 for i in range(5):box('Degrau',(0,i*.35,(i+1)*.16/2),(2,.36,(i+1)*.16),5,.025)
def trellis():
 for x in [-.8,.8]:box('Suporte',(x,0,1.2),(.13,.13,2.4),0,.02)
 for z in [.5,1,1.5,2,2.4]:box('Travessa',(0,0,z),(1.75,.10,.08),1,.02)
 for i in range(6):
  z=i*.33;tube('Trepadeira',[(-.7,0,z),(.7,-.04,z+.25)],[.025,.012],11,5)
  for j in range(6):leaf((-.6+j*.22,-.05,z+.1),.26,.08,j*2.4,11 if j%2 else 12)
def canopy():
 for x in [-1,1]:box('Poste',(x,0,1.1),(.12,.12,2.2),0,.02)
 for i in range(8):
  x=-1+i*.285
  mesh('Faixa de tecido',[(x,-.1,2.3),(x+.28,-.1,2.3),(x+.28,1.3,1.9),(x,1.3,1.9)],[(0,1,2,3),(3,2,1,0)],16 if i%2 else 3)
def crystalcluster():
 for x,y,h in [(0,0,1.3),(.35,.1,.75),(-.4,-.1,.65),(.1,.4,.55)]:
  tube('Cristal',[(x,y,.02),(x,y,h*.7),(x+.05,y,h)],[.18,.20,0],13,5)
def hangingbanner():
 box('Suporte',(0,0,1.8),(1,.12,.10),0,.02)
 mesh('Estandarte',[(-.42,0,1.75),(.42,0,1.75),(.42,-.05,.4),(0,-.08,.16),(-.42,-.05,.4)],[(0,1,2,3,4),(4,3,2,1,0)],16)
 for x in [-.35,.35]:beam('Borda bordada',(x,-.09,.45),(x,-.03,1.65),.012,9)
def rack():
 for x in [-.8,.8]:box('Pe',(x,0,.75),(.13,.4,1.5),0,.025)
 for z in [.15,.8,1.45]:box('Prateleira',(0,0,z),(1.85,.6,.10),1,.02)
 for x in [-.5,0,.5]:tube('Pote',[(x,0,.85),(x,0,1.08),(x,0,1.15)],[.14,.18,.10],5,8)
def waystone():
 tube('Base',[(0,0,0),(0,0,.12)],[.70,.65],4,8)
 for i in range(8):
  a=i*math.tau/8;box('Marca do anel',(math.cos(a)*.48,math.sin(a)*.48,.14),(.10,.10,.025),13)
 tube('Nucleo',[(0,0,.12),(0,0,.5),(0,0,1.0)],[0,.21,0],13,6)
def finish(name,fn,role):
 global parts
 parts=[]; random.seed(name); fn()
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name+'_LOD0'
 bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 # A shared palette atlas: one material/submesh, deterministic UV per original material.
 uv=o.data.uv_layers.new(name='Paleta')
 for poly in o.data.polygons:
  idx=mats.index(o.material_slots[poly.material_index].material);u=(idx+.5)/len(mats)
  for li in poly.loop_indices:uv.data[li].uv=(u,.5)
 o.data.materials.clear();o.data.materials.append(atlasmat)
 for p in o.data.polygons:p.material_index=0
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 tris=sum(len(p.vertices)-2 for p in o.data.polygons)
 lod=o.copy();lod.data=o.data.copy();bpy.context.collection.objects.link(lod);lod.name=name+'_LOD1'
 bpy.context.view_layer.objects.active=lod
 if any(tag in name for tag in ['Arvore','Arbusto','Flores','grama','Samambaia','Floreira','Trepadeira']):
  bm=bmesh.new();bm.from_mesh(lod.data);unseen=set(bm.verts);remove=[];idx=0
  while unseen:
   seed=unseen.pop();component={seed};todo=[seed]
   while todo:
    vtx=todo.pop()
    for e in vtx.link_edges:
     other=e.other_vert(vtx)
     if other in unseen:unseen.remove(other);component.add(other);todo.append(other)
   if len(component)<=10:
    idx+=1
    if idx%3:remove.extend(component)
  bmesh.ops.delete(bm,geom=remove,context='VERTS');bm.to_mesh(lod.data);bm.free()
 dec=lod.modifiers.new('LOD simplificado','DECIMATE');dec.ratio=.45;bpy.ops.object.modifier_apply(modifier=dec.name)
 lodtris=sum(len(p.vertices)-2 for p in lod.data.polygons)
 for obj,label in [(o,'LOD0'),(lod,'LOD1')]:
  bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
  bpy.ops.export_scene.fbx(filepath=str(ROOT/'Modelos'/f'{name}_{label}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=True)
 lod.hide_render=True;lod.hide_set(True)
 bounds=[Vector(v) for v in o.bound_box];dim=o.dimensions[:]
 catalog.append({'asset':name,'triangulos_LOD0':tris,'triangulos_LOD1':lodtris,'largura_m':round(dim[0],2),'profundidade_m':round(dim[1],2),'altura_m':round(dim[2],2),'materiais':1,'funcao':role})
 records.append(o)
 print('ASSET_OK',name,tris,lodtris,flush=True)
 # isolate preview on studio floor
 for old in records[:-1]:old.hide_render=True
 sz=max(dim[0],dim[1],dim[2]);target=Vector((0,0,dim[2]*.43))
 cam.location=target+Vector((1.15,-1.7,1.05))*sz;cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=sz*1.65
 if not os.environ.get('SKIP_RENDER'):
  scene.render.filepath=str(ROOT/'Previews'/f'{name}.png');bpy.ops.render.render(write_still=True)
 o.hide_render=True
 return o
# Atlas with color bands, giving portable FBX materials rather than Blender-only nodes.
atlas=bpy.data.images.new('Paleta_original',width=len(mats)*8,height=8)
pixels=[]
for y in range(8):
 for x in range(len(mats)*8):pixels.extend(COLORS[x//8][1])
atlas.pixels=pixels;atlas.filepath_raw=str(ROOT/'Modelos'/'Paleta_original.png');atlas.file_format='PNG';atlas.save();atlas.pack()
atlasmat=bpy.data.materials.new('Atlas original - um material');atlasmat.use_nodes=True
nd=atlasmat.node_tree.nodes.new('ShaderNodeTexImage');nd.image=atlas;nd.interpolation='Closest';atlasmat.node_tree.links.new(nd.outputs['Color'],atlasmat.node_tree.nodes.get('Principled BSDF').inputs['Base Color']);atlasmat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.8
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True
scene.render.resolution_x=700;scene.render.resolution_y=700;scene.render.resolution_percentage=100
scene.world.color=(.20,.20,.20)
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam
for loc,power,size in [((3,-4,8),1600,7),((-4,-1,5),1000,6),((2,5,6),1700,5)]:
 bpy.ops.object.light_add(type='AREA',location=loc);l=bpy.context.object;l.data.energy=power;l.data.shape='DISK';l.data.size=size;l.rotation_euler=(-l.location).to_track_quat('-Z','Y').to_euler()
parts=[];floor=box('Piso apenas da previa',(0,0,-.12),(200,200,.15),4);parts=[]
jobs=[('Casa_do_artesao',lambda:house(1),'Edificio exterior com chamine'),('Casa_do_herbalista',lambda:house(2),'Edificio exterior com floreiras'),('Parede_enxaimel',wall,'Modulo de fachada'),('Porta_de_madeira',lambda:door(0,0,0),'Porta separada, sem animacao'),('Janela_trabalhada',lambda:window(0,0,.7),'Modulo de janela'),('Arbusto_folhoso',lambda:bush(0),'Vegetacao com folhas modeladas'),('Arbusto_florido',lambda:bush(1),'Vegetacao com flores'),('Arvore_copa_aberta',lambda:tree(0),'Arvore com raizes e galhos'),('Arvore_luminosa',lambda:tree(1),'Variante fantastica turquesa'),('Samambaia',fern,'Vegetacao baixa'),('Tufo_de_grama',grass,'Vegetacao baixa'),('Flores_violetas',flowers,'Vegetacao florida'),('Cogumelos_do_breu',mushroom,'Conjunto de cogumelos'),('Banco_de_madeira',bench,'Mobiliario urbano'),('Cerca_de_madeira',fence,'Modulo 2.5m'),('Lanterna_de_rua',lamp,'Luminaria sem luz de runtime'),('Poco_de_pedra',well,'Decoracao urbana'),('Fonte_de_cristal',fountain,'Marco urbano sem VFX de agua'),('Placa_de_caminho',sign,'Sinalizacao sem texto fixo'),('Floreira_folhosa',planter,'Vaso com folhas'),('Console_antigo',console,'Tecnologia antiga sem logica'),('Marco_antigo',marker,'Pista visual sem logica'),('Carrinho_de_feira',cart,'Prop urbano'),('Banca_de_mercado',stall,'Prop urbano')]
jobs += [('Lenha_empilhada',logpile,'Decoracao de oficina'),('Cesto_trancado',basket,'Recipiente urbano'),('Altar_antigo',altar,'Prop interativo sem logica'),('Toco_cortado',stump,'Vegetacao morta'),('Ponte_de_madeira',bridge,'Passagem modular 3.8m'),('Escada_de_pedra',stairs,'Cinco degraus'),('Trepadeira_em_trelica',trellis,'Vegetacao vertical'),('Toldo_de_fachada',canopy,'Modulo de fachada'),('Cristais_arcanos',crystalcluster,'Decoracao fantastica'),('Estandarte_de_tecido',hangingbanner,'Decoracao sem simulacao'),('Estante_de_potes',rack,'Decoracao de oficina'),('Marco_de_transporte',waystone,'Prop visual sem teleporte')]
for n,f,r in jobs:finish(n,f,r)
floor.hide_render=True
# Gallery collection laid out for inspection, not an implemented game map.
for i,o in enumerate(records):o.hide_render=False;o.location=((i%6)*9,(i//6)*10,0)
with open(ROOT/'Documentacao'/'CATALOGO.csv','w',newline='',encoding='utf-8-sig') as f:
 w=csv.DictWriter(f,fieldnames=catalog[0].keys());w.writeheader();w.writerows(catalog)
(ROOT/'Documentacao'/'metricas.json').write_text(json.dumps(catalog,indent=2,ensure_ascii=False),encoding='utf8')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Kit_original_cidade_e_vegetacao.blend'))
print('KIT_COMPLETO',len(catalog),flush=True)
