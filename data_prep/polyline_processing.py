# -*- coding: utf-8 -*-
"""
Created on Sun Apr  9 18:12:12 2023

@author: joelk
"""
#%% Setup
import os
import fiona, geopandas as gpd, numpy as np
import rasterio

f=r'D:\Isabel\LDEO\FlightLines'
os.chdir(f)
del f

#%% Project KMLs to EPSG:3413
def project_kml(epsg=3413):
    """
    Projects KML files to the proper coordinate system.

    Parameters
    ----------
    epsg : int, optional
        The EPSG code of the desired coordinate system. The default is 3413.

    Returns
    -------
    None. Outputs a shapefile.

    """
    
    if 'KML' not in fiona.drvsupport.supported_drivers:
        fiona.drvsupport.supported_drivers['KML'] = 'rw'
    
    for file in os.listdir('KML'):
        gdf = gpd.read_file(os.path.join('KML', file), driver='KML')
        try:
            gdf = gdf.to_crs(epsg) # TODO: why does it say xyz not same size??
            gdf.to_file(os.path.join('Projected', file.replace('.kml','.shp')))
            print(f'Projected {file}')
        except:
            try:
                for line in gdf.geometry:
                    coords = np.array(line.coords)
                    print('lmao')
                    # TODO: try converting to points and projecting
            except Exception as e:
                print(f'Failed on {file}: {e}')

#%% Convert polylines to OBJ
def GDF_to_OBJ(gdf, drape, merge):
    """
    Converts the GeoDataFrame of the projected polylines to an OBJ file.

    Parameters
    ----------
    gdf : gpd.GeoDataFrame
        The smoothed polyline of the plane flight.
    drape : bool
        Whether to modify the Z values of the polylines.
    merge : bool
        Whether to output a single OBJ for the whole flight instead of one OBJ
        per feature.

    Returns
    -------
    None. Writes the output to a file.

    """
    
    all_coords = list()
    all_lines = list()
    
    if drape:
        raster = rasterio.open(r'..\DEMS\Petermann\Surface\MEASURES_DEM_CLIP.tif')
        dem = raster.read()[0]
    
    v = 0
    for _, name, line in gdf[['Name', 'geometry']].itertuples():
        
        coords = np.array(line.coords)
        
        if drape:
            coords = drape_polylines(coords, dem, raster)
            
        coords = coords[coords[:, 2] >= 0].astype(str)
        
        if merge:
            all_coords.append(coords)
            line_idxs = [str(i) for i in range(v+1, v+len(coords))]
            all_lines.append(' '.join(line_idxs))
            v += len(coords)
        else:
            vertices = '\nv '.join([' '.join(row) for row in coords.tolist()])
            line_idxs = [str(i) for i in range(1, coords.shape[0]+1)]
            filename = f'FlightLine_{name}.obj'
            with open(os.path.join('OBJ', filename), 'w') as f:
                f.write(f"v {vertices}\nl {' '.join(line_idxs)}")
    
    if merge:
        vertices = '\nv '.join([' '.join(row) for row in np.vstack(all_coords)])
        lines = '\nl '.join(all_lines)
        with open(os.path.join('OBJ', f'{file[:-4]}.obj'), 'w') as f:
            f.write(f"v {vertices}\nl {lines}")

#%% Drape polylines on DEM
def drape_polylines(coords, dem, raster):
    """
    Sets Z values of polyline to that of the closest point on the DEM. This is
    intuitively like having the polyline represent where on the ground the
    plane flew right above, as opposed to where the plane itself actually flew.

    Parameters
    ----------
    coords : np.array
        A Nx3 array of coordinate triples.
    dem : np.array
        A 2D array of elevation values.
    raster : io.DatasetReader
        The raster metadata.

    Returns
    -------
    new : np.array
        The modified Nx3 array of coordinates with altered Z values.

    """
    
    new = coords.copy()
    
    dem_idxs = np.array([raster.index(x, y) for x, y, _ in coords])
    new_idxs = np.where(
            (dem_idxs[:, 0] > 0) & (dem_idxs[:, 0] < raster.width) & 
            (dem_idxs[:, 1] > 0) & (dem_idxs[:, 1] < raster.height)
        )[0]
    if new_idxs.any():
        new[new_idxs, 2] = dem[dem_idxs[new_idxs, 0], dem_idxs[new_idxs, 1]]
    
    return new
    

#%% Run
if __name__ == '__main__':
    
    for file in os.listdir('Projected'):
        
        if file.endswith('.shp'):
            
            gdf = gpd.read_file(os.path.join('Projected', file), driver='ESRI')
            
            GDF_to_OBJ(gdf, True, False)