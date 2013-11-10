Welcome to Map-ity!

Map-ity Version 1.04 © RewindGameStudio 2013

Support:
http://www.rewindgamestudio.com/

Documentation:
http://www.rewindgamestudio.com/documentation/mapity_v104/index.html

Please see the link above for detailed documentation on Map-ity.

Map-ity allows you to model your environment on any real world location by providing access to open street map data.
Use it to get a lists of roads, waterways and buildings to use in your scene!
Use it to get the classification: Motorway, Primary or Secondary road. Canal or River.

Features:

Download the map data for your current location on mobile devices, or use a previously saved file.
Get lists of: Roads, Waterways, Buildings, Relations( e.g. Bus Routes ), Ways( Any map defined path ), Nodes( Any map feature )
Render a Gizmo showing the map in Editor.

Getting Started:

1. Move the Gizmo folder to the project root.
2. Add the Mapity prefab to your scene
3. Select it and set any options
4. While running select the Mapity object to see the map rendered by Unity Gizmos

Code examples will be added soon.

The code has lots of comments and should be easy to understand.

OpenStreetMap Data:
http://wiki.openstreetmap.org/wiki/Main_Page

This contains a lot of information about the data that is parsed by map-ity.

GeoNames Data:

Map-ity can now query height data for any longitude & latitude. This uses GeoNames SRTM webservice which requires a free account.
http://www.geonames.org/login Accounts can be set up here or we can do it for you.
A limit of free accounts is a max number of coordinates per query of 20. This is reflected in the code.

Note: Performing a web request for every 20 nodes can be slow. Best suited to small data sets.

A general overview - 
All map data is represented by a base element called a node. This can be a single structure
or part of a road.

Ways are comprised of nodes and represent Highways, Waterways etc.

Relations can contain Nodes, Ways and other Relations. Examples include BusRoutes.

ChangeList:

Version 1.04 -
Bug Fixes. HasLoaded was set too early.
Added OSM Tags to all Map Nodes, Ways and Relations.
These are accessible through a Tags class containing a HashTable of all tags.

Version 1.03 - 
Fixed webplayer compilation bug.
Added Highway Labels.
Added Height data queries from GeoNames.org.


Feature Road Map:

Access to more data will be added based on feedback from users.

Mapity's own spline and mesh modifier support for rendering roads directly from Map-ity.

Mapity's own building rendering.

The Open Street Map Data is © OpenStreetMap contributors. http://www.openstreetmap.org/copyright
