"""Read-only source inspection; renders and report are written to this audit folder."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
out=Path(__file__).resolve().parent
source=Path(r'C:/Users/Fabricio_S/Desktop/imagens refenteica/assets gerado por ia/asstes/MagoMagicoInimigo/Meshy_AI_Crystal_Arcane_Sentinel_20261029_texture_fbx')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(next(source.glob('*.fbx'))))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];rigs=[o for o in bpy.context.scene.objects if o.type=='ARMATURE']
report={'source':str(source),'animations':[a.name for a in bpy.data.actions],'rigs':[],'meshes':[]}
for rig in rigs:
 report['rigs'].append({'name':rig.name,'bones':[{'name':b.name,'parent':b.parent.name if b.parent else None,'head':list(b.head_local),'tail':list(b.tail_local)} for b in rig.data.bones]})
for o in meshes:
 edges={tuple(sorted(e.vertices)):[0,e.index] for e in o.data.edges}
 for p in o.data.polygons:
  for e in p.edge_keys:
   if e in edges:edges[e][0]+=1
 weighted=sum(1 for v in o.data.vertices if any(g.weight>.0001 for g in v.groups))
 adjacency=[[] for _ in o.data.vertices]
 for e in o.data.edges:a,b=e.vertices;adjacency[a].append(b);adjacency[b].append(a)
 unseen=set(range(len(adjacency)));components=[]
 while unseen:
  seed=unseen.pop();stack=[seed];count=0
  while stack:
   n=stack.pop();count+=1
   for k in adjacency[n]:
    if k in unseen:unseen.remove(k);stack.append(k)
  components.append(count)
 report['meshes'].append({'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'dimensions':list(o.dimensions),'vertexGroups':[g.name for g in o.vertex_groups],'weightedVertices':weighted,'modifiers':[{'type':m.type,'target':m.object.name if m.type=='ARMATURE' and m.object else None} for m in o.modifiers],'boundaryEdges':sum(c[0]==1 for c in edges.values()),'nonManifoldEdges':sum(c[0]>2 for c in edges.values()),'connectedComponents':sorted(components,reverse=True)[:20]})
print('SENTINEL_AUDIT',json.dumps(report));(out/'report.json').write_text(json.dumps(report,indent=2))
material=bpy.data.materials.new('SourcePreview');material.use_nodes=True;nodes=material.node_tree.nodes;bs=nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.7
base=next((p for p in source.glob('*.png') if not p.stem.endswith(('_normal','_metallic','_roughness'))),None)
if base:
 tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(base));material.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
for o in meshes:o.data.materials.clear();o.data.materials.append(material)
points=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box];lo=Vector([min(v[i] for v in points) for i in range(3)]);hi=Vector([max(v[i] for v in points) for i in range(3)]);center=(lo+hi)*.5;size=max(hi-lo)
bpy.context.scene.world=bpy.data.worlds.new("AuditWorld");bpy.context.scene.world.color=(.23,.23,.23)
camera=bpy.data.objects.new('AuditCamera',bpy.data.cameras.new('AuditCamera'));bpy.context.collection.objects.link(camera);bpy.context.scene.camera=camera;camera.data.type='ORTHO';camera.data.ortho_scale=size*1.25
for name,loc,power in [('Key',(3,-4,6),900),('Fill',(-4,-2,3),650),('Rim',(2,4,4),850)]:
 light=bpy.data.objects.new(name,bpy.data.lights.new(name,'AREA'));bpy.context.collection.objects.link(light);light.location=center+Vector(loc)*size/3;light.data.energy=power*size*size*.15;light.data.shape='DISK';light.data.size=size*2;light.rotation_euler=(center-light.location).to_track_quat('-Z','Y').to_euler()
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=900;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
for name,direction in [('front',(0,-1,0)),('side',(1,0,0))]:
 camera.location=center+Vector(direction)*size*3;camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
