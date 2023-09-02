# VISER: XR scientific visualization tool for ice-penetrating radar data in Antarctica and Greenland

This application facilitates the analysis of radar data for polar geophysicists by using Extended Reality (XR) technology to visualize radar returns in an accurate 3D geospatial context.

Project Website: https://pgg.ldeo.columbia.edu/projects/VISER

_Note: This project is still in development_.

***Last Updated 09/02/2023.***

# Team

Developers:
* [Qazi Ashikin](https://github.com/qaziashikin), Columbia University
* [Shengyue Guo](https://github.com/guosy1998), Columbia University
* [Joel Salzman](https://github.com/joelsalzman), Columbia University
* [Sofia Sanchez](https://github.com/sofiasanchez985), Columbia University
* [Ben Yang](https://github.com/benplus1), Columbia University
  
Additional Contributors:
* [Isabel Cordero](https://lamont.columbia.edu/directory/s-isabel-cordero), Lamont-Doherty Earth Observatory
* [Carmine Elvezio](https://carmineelvezio.com/), Columbia University
* [Bettina Schlager](https://www.cs.columbia.edu/~bschlager/), Columbia University

Advisors:
* Dr. [Alexandra Boghosian](https://alexandraboghosian.com/), Lamont-Doherty Earth Observatory
* Professor [Steven Feiner](http://www.cs.columbia.edu/~feiner/), Columbia University
* Professor [Kirsty Tinto](https://people.climate.columbia.edu/users/profile/kirsteen-j-tinto), Lamont-Doherty Earth Observatory

<br />

# Background

Ice-penetrating radar is a powerful tool to study glaciological processes but the data are often difficult to interpret. These radar measurements are taken by flying a plane equipped with sensors along some path (known as the flightline) and plotting the radar returns. These plots (radargrams) must be interpreted, often manually, in order to distinguish features inside the ice. Glaciologists are intensely interested in identifying the ice surface, bedrock depth, and whatever features can be discerned in the subsurface. An essential step in this process is "picking lines," or identifying contiguous curves in radargrams that correspond to real features in the world (as opposed to noise). Current methods are suboptimal.

VISER aims to improve interpretability of radargrams using XR. First, we place the user inside a digital twin of the relevant ice shelf so that all the data are properly contextualized relative to each other in space. Second, we model the radargrams as Unity GameObjects so that the features visible on the plots (which appear as textures on the GameObjects) also appear in the proper 3D geospatial context. Third, we implement numerous interfaces for analysis and manipulation so that users can explore the data.

# Project Architecture

The application contains three scenes.

* Home Menu
* Ross Ice Shelf
* Petermann Glacier

The Home Menu scene is mostly empty except for an XR menu that allows users to load one of the other scenes.
<img src="https://github.com/qaziashikin/polAR/blob/Summer/images/homeMenu.png?raw=true"      alt="Home Menu"      style="float: left; margin-right: 10px;" />
<br />

### Ross Ice Shelf (RIS) Scene

The RIS scene uses the following assets.
| Asset Name | Source | Description |
| :-----------: | ----------- | ----------- |
| Surface DEM | BedMachine | Digital elevation model of ice shelf surface |
| Base DEM | BedMachine | Digital elevation model of bedrock |
| Radar Images | ROSETTA | Planar GameObjects textured with radargrams |
| CSV Picks Containers | ROSETTA | Points on the flightlines |
| Minimap |  | Aerial map that lives on the scene menu and allows users to teleport |
| Bounding Box |  | Bounding box around the entire scene |
| Grid |  | Evenly-spaced graticule grid generated at runtime |

In this scene, radar images are textured on planar GameObjects. Only plots from the portions of the flightline where the plane was flying relatively straight are shown. This scene has the most mature navigation system and includes a minimap.

The flightlines are loaded from CSV files as discrete points.

<img src="https://github.com/qaziashikin/polAR/blob/Summer/images/ris_sceneMenu.png?raw=true"      alt="RIS Scene Menu"      style="float: left; margin-right: 10px;" />
<br />

### Petermann Glacier Scene

The Petermann scene uses the following assets.
| Asset Name | Source | Description |
| :-----------: | ----------- | ----------- |
| Surface DEM | BedMachine | Digital elevation model of glacier surface |
| Base DEM | BedMachine | Digital elevation model of bedrock |
| Flightlines | IceBridge (CReSis) | Polylines corresponding to where the plane flying the radar system went above the surface |
| Radargram meshes | IceBridge (CReSis) | Triangle meshes with radargrams mapped as texture |
| Bounding Box |  | Bounding box around the entire scene |
| Grid |  | Evenly-spaced graticule grid generated at runtime |

<img src="https://github.com/qaziashikin/polAR/blob/Summer/images/petermann_sceneMenu.png?raw=true"      alt="Petermann Scene Menu"      style="float: left; margin-right: 10px;" />
<br />

In this scene, the flightlines and radargrams are generated at runtime as GameObjects rather than living permanently in the scene. The flightlines are rendered as polylines and are broken up into segments during preprocessing. Each radar object is modeled as a triangle mesh, textured with a radargram, and linked to the associated flightline portion. Using meshes enables the entire flightline to be displayed. The flightline coordinates are accurate within the projected coordinate system but the vertical coordinate is snapped to the surface DEM for ease of viewing.

<img src="https://github.com/qaziashikin/polAR/images/blob/Summer/petermann_screenshot.png?raw=true"      alt="Petermann DEM-Radar Intersection"      style="float: left; margin-right: 10px;" />
<br />

The radargram objects are generated in the following way. In preprocessing, the plots are generated in MATLAB and converted into the three separate files by the _greenland_obj.m_ script.

* .obj (contains the geometry of the mesh)
* .mtl (the material file for the mesh)
* .png (the radargram; this is mapped onto the mesh)

The .obj files then need to be decimated in order to improve performance. This is done in Blender and currently requires manual attention to ensure that the meshes do not deform significantly at the boundaries. These simplified .obj files, along with the .mtl and .png files, are added to the Assets folder. Upon loading the scene, the _LoadFlightlines.cs_ script programmatically generates meshes from the file triples and associates each textured mesh with the corresponding flightline polyline. Users can select a flightline portion to load a mesh.

### Code files

| Filename | Description | Scenes |
| :-----------: | ----------- | ----------- |
| HomeMenuEvents | Handles events in the Home Menu scene | Home |
| CSVReadPlot | Reads flightlines from CSV files into the scene | RIS |
| HoverLabel | Manages on-the-fly radar image tickmarks | RIS |
| MinimapControl | Controls the minimap | RIS |
| UpdateLine | Redraws the line between the Mark and Measure objects | RIS |
| RadarEvents2D | Handles events specific to the (2D) radargram planes | RIS |
| LoadFlightLines | Generates radargram meshes from obj/mtl files | Petermann |
| Measurement | Calculates distances used in Measurement Mode | Petermann |
| RadarEvents3D | Handles events specific to the (3D) radargram meshes | Petermann |
| DrawGrid | Generates a graticule grid in the scene | RIS, Petermann |
| DynamicLabel | Manages on-the-fly radar menu updates | RIS, Petermann |
| MarkObj | Handles events associated with the mark (cursor) object | RIS, Petermann |
| MenuEvents | Handles generic events associated with menus | RIS, Petermann |
| RadarEvents | Handles events associated with radargrams that are the same in both scenes | RIS, Petermann |

<br />

# Controls

VISER works on both AR (Hololens) and VR (Quest) headsets. Since the most recent version is optimized for VR, we omit the AR controls in this section.

### Universal Controls 

Here is a list of interactions available everywhere using the Oculus Controllers:

| Interaction | Description |
| :-----------: | ----------- |
| Trigger Buttons | Used to interact with the menus and select radar images |
| Joysticks | Used to move around the entire scene freely |

Movement can be accomplished with the joysticks. Tilt forward to shoot a ray; if the ray is white, release the joystick to teleport there. You can also nudge yourself back by tilting back on the joystick.

The scene bounding box can be used to scale everything inside the scene along any of the three axes. Users can grab the bounding box from the corners or the center of any edge and use standard MRTK manipulation to adjust the box's size.

Here is a list of interactions available with the main menu. All of these interactions can be used with the Oculus Controller trigger buttons:
| Main Menu Interaction Title | Description |
| :-----------: | ----------- |
| Surface DEM | Checking this button turns the Surface DEM on/off |
| Base DEM | Checking this button turns the Base DEM on/off |
| Bounding Box | Checking this button turns the bounding box for the entire scene on/off |
| Vertical Exaggeration | Vertically stretches or shrinks the DEMs (the exaggeration is dynamic and can be repeated limitlessly) |
| Sidebar "X" | Closes the menu |
| Sidebar "Refresh" | Resets the entire scene |
| Sidebar "Pin" | Saves scene information to .txt file; currently non-functional |

<br />

### RIS Scene Controls

Because the Ross Ice Shelf scene uses a different workflow than the Petermann Glacier scene, some of the controls are different. Here are the menu options that are only available in the RIS scene.

<img src="https://github.com/qaziashikin/polAR/blob/Summer/images/ris_radarMenu.png?raw=true"      alt="RIS Radar Menu"      style="float: left; margin-right: 10px;" />
<br />

| Main Menu Interaction Title | Description |
| :-----------: | ----------- |
| All Radar Images | Checking this button turns all the radar lines on/off |
| All CSV Picks | Checking this button turns all the CSV Picks on/off |
| Sidebar "Minimap" | Turns teleport mode on (dot will be green) or off (dot will be red and allows for moving the minimap) |
| Minimap | If teleport mode is enabled, teleports the user to that location in the scene |

Here is a list of interactions available with the line menu. All of these interactions can be used with the Oculus Controller trigger buttons:
| Line Menu Interaction Title | Description |
| :-----------: | ----------- |
| Vertical Exaggeration | Vertically stretches or shrinks the radar image (the exaggeration is dynamic and can be repeated limitlessly) |
| Horizontal Exaggeration | Horizontally stretches or shrinks the radar image (the exaggeration is dynamic and can be repeated limitlessly) |
| Rotation | Rotates the image by the seleced amount of degrees |
| Transparency | Makes the radar image transparent by the selected percent |
| View Radar Image | Checking this button turns the selected radar image on/off |
| View CSV Picks | Checking this button turns the selected CSV Picks on/off |

The line menu has a unique sidebar.
| Main Menu Interaction Title | Description |
| :-----------: | ----------- |
| Measurement Mode | Turns measurent mode on/off (allows user to place two marks on the same image and measure the distance between) |
| Sidebar "X" | Closes the menu |
| Sidebar "Home" | Opens the main menu |
| Sidebar "Refresh" | Resets the radar line, or snap two radar images under measure mode |
| Sidebar "Pin" | Saves the radar information to .txt file |
| Sidebar ">" | Teleports to the location of the radar line in the scene |


<br />

### Petermann Scene Controls

<img src="https://github.com/qaziashikin/polAR/blob/Summer/images/petermann_radarMenu.png?raw=true"      alt="Petermann Radar Menu"      style="float: left; margin-right: 10px;" />
<br />

| Main Menu Interaction Title | Description |
| :-----------: | ----------- |
| Scene Vertical Scaling | Scales everything in the scene along the vertical (Y) axis |
| Radargrams | Checking this button turns all the radargram meshes on/off |
| Bounding Box | Toggles the bounding box around the scene |
| Flightlines | Checking this button turns the flight lines that are currently loaded on/off |
| Surface DEM | Checking this button turns the Surface DEM on/off |
| Base DEM | Checking this button turns the Base DEM on/off |

Here is a list of interactions available with the radar menu. These are all specific to the 3D workflow.
| Radar Menu Interaction Title | Description |
| :-----------: | ----------- |
| Rotation | Rotates the radar mesh by the seleced amount of degrees from its initial orientation |
| Exaggeration Along Axes | Stretches or shrinks the radar image (the exaggeration is dynamic and can be repeated limitlessly); the sliders are in the order Y (vertical), X, Z |
| Transparency | Makes the radar image transparent by the selected percent; currently non-functional |
| Radargram Mesh | Checking this box turns the radargram mesh on/off |
| Surface DEM | Checking this button turns the Surface DEM on/off |

The radar menu has a unique sidebar.
| Radar Menu Title | Description |
| :-----------: | ----------- |
| Measurement Mode | Turns measurent mode on/off (allows user to place two marks on the same image and measure the distance between); currently non-functional |
| Sidebar "X" | Closes the menu |
| Sidebar "Home" | Opens the main menu |
| Sidebar "Refresh" | Resets the radargram to its initial location |
| Sidebar "Pin" | Saves the radar information to .txt file; currently non-functional |
| Sidebar ">" | Teleports to the location of the radar mesh centroid in the scene |

<br />

### Voice Commands

The voice commands can be used at any time and do not need to be toggled. Simply say the word clearly and VISER will process the command.

| Voice Command | Description |
| :-----------: | ----------- |
| "Menu" | Open/close the menu |
| "Model" | Turn on/off the DEM models |
| "Image" | Turn on/off the image for the selected radar line |
| "Line" | Turn on/off the CSV picks for the selected radar line |
| "Go" | Teleport to the image location for the selected radar line |
| "Mark" | Add a point to the image for the selected radar line |
| "Reset" | Reset the radar lines for the whole scene |
| "Measure" | Turn on/off measurement mode |
| "Box" | Turn on/off the bounding box for the entire scene |
