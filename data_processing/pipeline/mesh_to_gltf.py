import bpy
import os
import glob
#To execute code, follow this order (MacOS):
#alias blender="/Applications/Blender.app/Contents/MacOS/Blender"
#source ~/.zshrc
#blender --background --python mesh_to_gltf.py

# Path to your directory containing .obj files
input_dir = 'Resources/mesh'  # Update this path

# Get all .obj files in the directory
obj_files = glob.glob(os.path.join(input_dir, '*.obj'))

for obj_file in obj_files:
    #print(obj_files)
    basename = os.path.basename(obj_file)
    name, ext = os.path.splitext(basename)
    collection_name = name  # Use filename as collection name (e.g., 'YYYYMMDD_#Deployment')

    # Create a new collection for each flight data
    new_collection = bpy.data.collections.new(collection_name)
    bpy.context.scene.collection.children.link(new_collection)

    # Keep track of objects before import
    before_objects = set(bpy.data.objects)

    try:
        # Import the .obj file
        bpy.ops.wm.obj_import(filepath=obj_file)
        #print(obj_file)
        #bpy.ops.import_scene.obj(filepath=obj_file)
    except Exception as e:
        print(f"Failed to import {obj_file}: {e}")
        continue  # Skip this file and proceed with others

    # Identify new objects imported
    after_objects = set(bpy.data.objects)
    new_objects = after_objects - before_objects

    for obj in new_objects:
        # Ensure the object is linked to the new collection
        if obj.users_collection:  # If the object is already in a collection
            for coll in obj.users_collection:
                coll.objects.unlink(obj)  # Unlink it from the existing collection

        new_collection.objects.link(obj)  # Link it to the new collection

        # Scale the object
        obj.scale = (0.0001, 0.0001, 0.001)

        # Translate the object
        obj.location.x += 20
        obj.location.y += 100
        obj.location.z += 0

# Export each collection to GLTF or FBX
for collection in bpy.data.collections:
    if collection.name.startswith('YYYY'):  # Example condition
        bpy.ops.object.select_all(action='DESELECT')  # Deselect all objects

        for obj in collection.objects:
            obj.select_set(True)  # Select objects in the collection

        output_dir = 'Resources/fbx'  # Update this path
        output_filename = os.path.join(output_dir, f'{collection.name}.glb')  # GLTF output

        try:
            bpy.ops.export_scene.gltf(
                filepath=output_filename,
                export_format='GLB',
                use_selection=True
            )
        except Exception as e:
            print(f"Failed to export collection {collection.name}: {e}")
