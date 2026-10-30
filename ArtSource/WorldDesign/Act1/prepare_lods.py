"""Blender batch conversion: three static LODs, shared UVs, no source changes."""
import bpy, json
from pathlib import Path
from mathutils import Vector

root = Path(__file__).parent
report = []
for job in json.loads((root / 'jobs.json').read_text()):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=job['source'])
    meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    corners = [o.matrix_world @ Vector(c) for o in meshes for c in o.bound_box]
    lo = Vector(tuple(min(c[i] for c in corners) for i in range(3)))
    hi = Vector(tuple(max(c[i] for c in corners) for i in range(3)))
    center = Vector(((lo.x + hi.x)/2, (lo.y + hi.y)/2, lo.z))
    factor = 8 / max(hi.x-lo.x, hi.y-lo.y, .01)
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes: o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.object.join()
    base = bpy.context.object
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    for v in base.data.vertices: v.co = (base.matrix_world @ v.co - center) * factor
    base.location = (0,0,0); base.rotation_euler = (0,0,0); base.scale = (1,1,1)
    base.data.materials.clear(); base.name = 'LOD0'
    triangles = sum(len(p.vertices)-2 for p in base.data.polygons)
    if triangles > 10000:
        dec = base.modifiers.new('MobileBudget', 'DECIMATE'); dec.ratio = 10000/triangles
        bpy.ops.object.modifier_apply(modifier=dec.name)
    objects = [base]
    for index, ratio in [(1,.45),(2,.18)]:
        o = base.copy(); o.data = base.data.copy(); bpy.context.collection.objects.link(o)
        o.name = 'LOD'+str(index); bpy.context.view_layer.objects.active = o
        dec = o.modifiers.new('Reduced', 'DECIMATE'); dec.ratio = ratio
        bpy.ops.object.modifier_apply(modifier=dec.name); objects.append(o)
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects: o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=job['output'], use_selection=True, object_types={'MESH'},
        add_leaf_bones=False, bake_anim=False, path_mode='STRIP', axis_forward='-Z', axis_up='Y')
    report.append({'name':job['name'],'triangles':[sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects]})
(root/'lod_report.json').write_text(json.dumps(report,indent=2))
print('ACT1_LODS_READY',len(report))
