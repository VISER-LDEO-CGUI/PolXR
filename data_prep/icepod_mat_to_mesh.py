# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import os
import pandas as pd
import geopandas as gpd
import numpy as np
import matplotlib.pyplot as plt
from scipy.io import loadmat

# Constants
cAir = 299792458  # m/s
cIce = 1.68e8	  # m/s


def cresis_to_mesh(
    matfile,
    outdir,
    crs_old,
    crs_new,
    set_origin=False,
    clip_noise=False
):

    # Read mat
    mat = loadmat(matfile)

    # Convert arrays to dataframes
    data = None
    singletons = dict()
    horizontal = dict()
    vertical = dict()
    layerInd = dict()

    for k, v in mat.items():
        if isinstance(v, np.ndarray):
            
            # Search by key
            if v.shape == (1, 1):
                singletons[k] = v
            elif k.lower() == 'data':
                data = pd.DataFrame(v).T
            elif k.lower() in {'time', 'depth'}:
                vertical[k] = v.flatten()
            else:
                horizontal[k] = v.flatten()
  
    # Check for IcePod / Rosetta variables
    try:
        var_exists = mat.get('FlightNo')
    except NameError:
        var_exists = False
        print('No FlightNo variable detected. This is not IcePod radar data. Continuing with CReSIS presets.\n')
    else:
        var_exists = True
        print('This is IcePod radar data. Arranging the variables appropriately...\n')
        layerId = mat['layerInd']
        horizontal.pop('layerInd')

    # Convert coordinates to points
    horizontal['geometry'] = gpd.GeoSeries.from_xy(
        horizontal['Longitude'],
        horizontal['Latitude'],
        crs=crs_old)

    # Create GeoDataFrames so that everything is in Pandas
    hdf = gpd.GeoDataFrame(horizontal, crs=crs_old, geometry='geometry')
    vdf = pd.DataFrame(vertical)

    # Calculate z values
    surface = np.nan_to_num(
        hdf['Surface'],
        copy=False,
        nan=np.nanmean(hdf['Surface']))
    time = np.tile(vdf['Time'], (surface.shape[0], 1))
    top = np.array(hdf['Elevation'] - (0.5 * cAir * np.mean(surface))).reshape(-1, 1)
    z = top - 0.5*(cIce * (time - np.mean(surface.reshape(-1, 1))))
    
    try:
        var_exists = mat.get('Surf_Elev')
    except NameError:
        var_exists = False
        print('No Surf_Elev variable detected. Continuing with CReSIS presets.\n')
    else:
        var_exists = True
        print('IcePod variable Surf_Elev added to HDF☺...\n')
        surfElev = np.nan_to_num(
            hdf['Surf_Elev'],
            copy=False,
            nan=np.nanmean(hdf['Surf_Elev']))

    # Project coordinates to new coordinate system
    if crs_new != crs_old:
        hdf = hdf.to_crs(crs_new)
    x = np.tile(hdf.geometry.x.to_numpy().reshape(-1, 1), (1, z.shape[1]))
    y = np.tile(hdf.geometry.y.to_numpy().reshape(-1, 1), (1, z.shape[1]))
    
    arr = np.empty((*x.shape, 4),dtype = 'complex_')
    #TODO: then make the arrays real again using .astype(float) somewhere
    arr[:, :, 0] = x
    arr[:, :, 1] = y
    arr[:, :, 2] = z
    arr[:, :, 3] = data
    
    # clip empty space
    if clip_noise:
        k = 5 # after this many empty rows, assume we're just seeing noise
        mask = []
        empty_rows = 0
        remove_rows = False
        for row in arr:
            if np.all(row == 0): # we could change this to look for max or mean
                empty_rows += 1
                if empty_rows == k:
                    remove_rows = True 
            else:
                empty_rows = 0
            mask.append(not remove_rows)
        arr = arr[mask, :, :]

    # create mesh
    return create_mesh(matfile, outdir, arr, set_origin)


def create_mesh(matfile, outdir, arr, set_origin=False):
    """
    Creates a triangle mesh out of the data.

    Parameters
    ----------
    matfile : str
        Filepath of the mat file.
    outdir : str
        Directory for the output files to be put into.
    arr : np.array
        The array computed in the previous function.
    set_origin : bool, optional
        Whether to reset the coordinates so they intersect with the origin. 
        The default is False.
        This should only be set to True while debugging.

    Returns
    -------
    out_obj : str
        The filepath of the outputted obj file.

    """

    # filenames
    basename = os.path.basename(matfile).replace('.mat', '')
    basepath = os.path.join(outdir, basename)
    out_png = f'{basepath}.png'
    out_mtl = f'{basepath}.mtl'
    out_obj = f'{basepath}.obj'
    out_line = f'{basepath.replace(basename, f"FlightLine_{basename}")}.obj'

    # reset origin maybe
    if set_origin:
        arr[:, :, 0] -= arr[:, :, 0].min()
        arr[:, :, 1] -= arr[:, :, 1].min()

    # write png file
    cvals = 20 * np.log10(np.abs(arr[:, :, 3]))
    c = np.abs((cvals - np.min(cvals)) / (np.max(cvals) - np.min(cvals))) * 256
    plt.imsave(out_png, np.flipud(c.T), cmap='gray')

    # write mtl file
    with open(out_mtl, 'w') as mtl:
        mtl.write(f'newmtl {basename}\n')
        mtl.write('Ka  0.0000  0.0000  0.0000\n')
        mtl.write('Kd  1.0000  1.0000  1.0000\n')
        mtl.write('illum 1\n')
        mtl.write(f'map_Kd {out_png}')

    # write obj file
    polyline = list()
    with open(out_obj, 'w') as obj:
        
        # include material
        obj.write(f'mtllib {out_mtl}\n')
        
        # iterate through horizontal coordinates
        for i in range(arr.shape[0]-1):
            
            # extract coordinate triples for the corners of the next rectangle
            ul = ' '.join(arr[i, 0, :3].astype(str))        # upper left
            ll = ' '.join(arr[i, -1, :3].astype(str))       # lower left
            lr = ' '.join(arr[i+1, -1, :3].astype(str))     # lower right
            ur = ' '.join(arr[i+1, 0, :3].astype(str))      # upper right
            
            # add vertices
            for vertex in (ul, ll, lr, ur):
                obj.write(f'v {vertex}\n')
            
            # add uv mapping coordinates
            obj.write(f'vt {i/(arr.shape[0]-1)} 0.0\n')       # upper left
            obj.write(f'vt {i/(arr.shape[0]-1)} 1.0\n')       # lower left
            obj.write(f'vt {(i+1)/(arr.shape[0]-1)} 1.0\n')   # lower right
            obj.write(f'vt {(i+1)/(arr.shape[0]-1)} 0.0\n')   # upper right
            
            # store points for polyline
            if not i:
                polyline.append(ul)
            polyline.append(ur)
        
        # create faces
        obj.write('g mesh\n')
        for i in range(arr.shape[0]-1):
            
            # create two triangles
            for idxs in [(0, 1, 2), (0, 2, 3)]:
                face_string = ' '.join([f'{i*4+j+1}/{i*4+j+1}' for j in idxs])
                obj.write(f'f {face_string}\n')
        
        obj.write('g\n')
        obj.write(f'usemtl {basename}\n\n')
        
    # write polyline file
    # TODO: should this be done elsewhere since we have one polyline per flight
    with open(out_line, 'w') as obj2:
        
        for pt in polyline:
            obj2.write(f'v {pt}\n')
        obj2.write(f'l {" ".join([str(i+1) for i in range(len(polyline))])}')

    return out_obj

if __name__ == '__main__':
    folder = r'D:\data_process\cresis_mesh\20100324_01\mat'
    # matfile = os.path.join(folder, 'Data_img_01_20100324_01_013.mat')
    for matfile in os.listdir(folder):
        matfile = os.path.join(folder, matfile)
        crs_old = 4326  #WGS84
        #crs_new = 3413  #NSIDC Sea Ice Polar Stereographic North
        crs_new = 3031  #Antarctic Polar Stereographic
        outdir = folder.replace('mat', 'mesh')
        cresis_to_mesh(matfile, outdir, crs_old, crs_new, False)
