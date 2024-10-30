# -*- coding: utf-8 -*-
"""
Created on Mon May 08 15:20:00 2024

@author: joelk, leahk, isabelc
"""
#%% Setup
import os
import pandas as pd
import open3d as o3d
import numpy as np

os.chdir(r'D:\data_process\dem_mesh\petermann\base')   # Add path to DEM XYZ File or TIF

#%% In QGIS, merge TIF files, clip them, and convert them to XYZ based on How-To Build Instructions.
## Once complete, run the following code

#%% Create point cloud
#FutureWarning: The 'delim_whitespace' keyword in pd.read_table is deprecated and will be removed in a future version. 
#Use ``sep='\s+'`` instead df = pd.read_table('BedMachine_Base_Petermann.xyz', delim_whitespace=True,

df = pd.read_table('BedMachine_Base_Petermann.xyz', sep='\s+',       # Add name of XYZ file to convert
                   names = ['x','y','z'])
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
print('Removing low density vertices...\n')
vertices_to_remove = densities < np.quantile(densities, 0.04) # 0.04 for BedMachine, 0.04 or 0.05 for MEASURES
mesh.remove_vertices_by_mask(vertices_to_remove)
print(mesh)

#%% Output OBJ file
o3d.io.write_triangle_mesh("BedMachine_Base_Petermann_DEM.obj", mesh)  # Change name of OBJ depending on DEM


