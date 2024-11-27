def center_obj(file_path):
    """
    Centers the given .obj file's vertices around (0, 0, 0) in-place and saves transformation metadata as comments
    in the .obj file.

    Args:
        file_path (str): Path to the input .obj file (modifies it directly).
    """
    vertices = []
    other_lines = []

    # Read the .obj file
    with open(file_path, 'r') as file:
        for line in file:
            if line.startswith('v '):  # Vertex line
                parts = line.split()
                x, y, z = map(float, parts[1:4])
                vertices.append((x, y, z))
            else:
                other_lines.append(line.strip())

    if not vertices:
        print("No vertices found in the .obj file.")
        return

    # Calculate the centroid (offsets)
    centroid_x = sum(v[0] for v in vertices) / len(vertices)
    centroid_y = sum(v[1] for v in vertices) / len(vertices)
    centroid_z = sum(v[2] for v in vertices) / len(vertices)

    # Adjust vertices to center the object
    centered_vertices = [
        (x - centroid_x, y - centroid_y, z - centroid_z) for x, y, z in vertices
    ]

    # Write the updated .obj file with metadata as comments
    with open(file_path, 'w') as file:
        # Write metadata as comments at the top of the .obj file
        file.write("# Transformation Metadata\n")
        file.write("# ========================\n")
        file.write(f"# Original Centroid (Offset):\n")
        file.write(f"#   X: {centroid_x:.15f}\n")
        file.write(f"#   Y: {centroid_y:.15f}\n")
        file.write(f"#   Z: {centroid_z:.15f}\n")
        file.write("#\n")
        file.write("# Note: To revert to original coordinates, add the offsets back to the vertex coordinates.\n")
        file.write("#\n")
        
        # Write updated vertices
        for x, y, z in centered_vertices:
            file.write(f"v {x:.15f} {y:.15f} {z:.15f}\n")

        # Write other lines (faces, normals, etc.)
        for line in other_lines:
            file.write(line + '\n')