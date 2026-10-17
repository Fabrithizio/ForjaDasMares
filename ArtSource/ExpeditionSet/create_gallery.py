"""Read exported models and make an editable gallery/overview without changing Unity assets."""
import bpy,math
from mathutils import Vector
from pathlib import Path
folder=Path(__file__).resolve().parent
assets=folder.parents[1]/'Assets/ForjaDasMares/Art/ExpeditionSet'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
names=['supply','crate','barrel','seal','well','rock','bush','mushrooms','banner','raider','guardian','archer','unstable','summoner','elite_warden','elite_shaman','elite_reaver']
for i,name in enumerate(names):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(assets/(name+'.fbx')))
 added=[o for o in bpy.context.scene.objects if o not in before];group=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(group)
 for o in added:
  if o.parent not in added:o.location+=Vector(((i%5)*3,(i//5)*3,0))
  for c in list(o.users_collection):c.objects.unlink(o)
  group.objects.link(o)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=20;scene.cycles.use_denoising=True
scene.render.resolution_x=1200;scene.render.resolution_y=900;scene.render.resolution_percentage=100;scene.world.color=(.25,.25,.25)
bpy.ops.object.camera_add(location=(16,-17,20));cam=bpy.context.object;target=Vector((6,4.5,.8));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=20;scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(3,-2,13));light=bpy.context.object;light.data.energy=3000;light.data.size=12;light.rotation_euler=(target-light.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.mesh.primitive_plane_add(size=200,location=(6,4,-.02));ground=bpy.context.object;mat=bpy.data.materials.new('GalleryGround');mat.diffuse_color=(.08,.11,.13,1);ground.data.materials.append(mat)
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':area.spaces.active.region_3d.view_distance=21;area.spaces.active.region_3d.view_location=target
scene.render.filepath=str(folder/'ExpeditionSet_Preview.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(folder/'ExpeditionSet_Gallery.blend'))
print('GALLERY_READY models=17')
