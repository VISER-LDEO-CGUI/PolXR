import os
import pandas as pd
import open3d as o3d

os.chdir(r'C:\Users\AntARctica\Documents\Petermann\DEM')

df = pd.read_csv('BEDMACHINE.csv', sep=' ')
df.columns = ['xyz']
df = pd.DataFrame([r.split(',') for r in df['xyz'].values])
for c in df.columns:
    df[c] = df[c].astype(float)
        
pcd = o3d.geometry.PointCloud()
pcd.points = o3d.utility.Vector3dVector(df.to_numpy())
del df

pcd.estimate_normals()

#%% Create mesh
mesh, densities = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(
        pcd, depth=9
    )

#%% Output OBJ file
o3d.io.write_triangle_mesh("BEDMACHINE_DEM.obj", mesh)

#%% Simplify to lower LODs?
ds = pcd # need to downsample

mesh = o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(
    ds, o3d.utility.DoubleVector([30, 45, 60, 90]))

o3d.io.write_triangle_mesh("BEDMACHINE_DEM_ball.obj", mesh)