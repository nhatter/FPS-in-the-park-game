using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Mapity. Singleton Interface for accessing map data.
/// </summary>
public class Mapity : MonoBehaviour
{	
	/// <summary>
	/// The singleton.
	/// </summary>
	public static Mapity Singleton;
	
	#region Settings
	
	/// <summary>
	/// The auto load. Will we load in the Start() method or wait for a manual call to Load()
	/// </summary>
	public bool autoLoad = true;
	
	/// <summary>
	/// The has loaded. Has Mapity loaded and parsed the data.
	/// </summary>
	private bool hasLoaded = false;
	
	/// <summary>
	/// The download map data. Will we download data or use the local *.mapity data
	/// </summary>
	public bool downloadMapData = true;

	/// <summary>
	/// The open street map API URL. We use this to download the map data.
	/// See http://wiki.openstreetmap.org/wiki/Main_Page for more info.
	/// http://www.overpass-api.de/api/xapi? ( Recommended for release ).
	/// http://api.openstreetmap.org/api/0.6/ ( Used for editing data, a good fall back for dev ).
	/// </summary>
	public string openStreetMapApiUrl = "http://www.overpass-api.de/api/xapi?";
	
	/// <summary>
	/// The query height data. Will we query height data from GeoNames
	/// </summary>
	public bool queryHeightData = false;
	
	/// <summary>
	/// The geonames API URL. This is used to get the srtm3 data for a specific longitude & latitude
	/// See http://www.geonames.org/export/web-services.html
	/// </summary>
	public string geonamesApiUrl = "http://api.geonames.org/srtm3?";
	
	/// <summary>
	/// The geonames username. You need a login to use GeoNames, it's free.
	/// See http://www.geonames.org/login for account creation
	/// </summary>
	public string geonamesUsername = "demo";
	
	/// <summary>
	/// The use device location. Will we try query device location to get map data.
	/// </summary>
	public bool useDeviceLocation = false;
	
	/// <summary>
	/// The Geographic Location. Longitude & Lattitude ( x & y ) of the currently loaded/downloaded map data.
	/// </summary>
	public Vector2 location = new Vector2( 0.0f, 0.0f );
	
	/// <summary>
	/// The name of the local mapity file we're loading from the MapData folder.
	/// </summary>
	public string localMapFileName = "";
	
	/// <summary>
	/// The save downloaded map data. Should we save the downloaded map data.
	/// </summary>
	public bool saveDownloadedMapData = false;
	
	/// <summary>
	/// The map file name save prefix. Use this string to prefix the downloaded mapity data for saving.
	/// </summary>
	public string mapFileNameSavePrefix = "MapData_";
	
	/// <summary>
	/// The enable debug logging. This is slow. It will output all mapity's debug logs. Useful for debugging.
	/// </summary>
	public bool enableLogging = false;
	
	/// <summary>
	/// The gizmo draw nodes. Draws a Gizmo at every map node - Caution!( Don't exceed Unity's max supported verts for gizmos )
	/// </summary>
	public bool gizmoDrawNodes = false;
	
	/// <summary>
	/// The gizmo draw ways. Should we draw the ways.
	/// </summary>
	public bool gizmoDrawWays = true;
	/// <summary>
	/// The gizmo draw high ways. Should we draw the HighWays.
	/// </summary>
	public bool gizmoDrawHighWays = true;
	/// <summary>
	/// The gizmo draw high ways labels. Should we draw the HighWays labels containg additional information.
	/// </summary>
	public bool gizmoDrawHighWaysLabels = false;
	/// <summary>
	/// The gizmo draw water ways. Should we draw the WaterWays.
	/// </summary>
	public bool gizmoDrawWaterWays = false;
	/// <summary>
	/// The gizmo draw buildings. Should we draw the Buildings.
	/// </summary>
	public bool gizmoDrawBuildings = false;
	/// <summary>
	/// The gizmo draw relations. Should we draw the Relations.
	/// </summary>
	public bool gizmoDrawRelations = false;
	
	/// <summary>
	/// Map zoom. Caution!( Higher Zoom Levels contain a lot of data ) 0,1,2 recommended as this is a manageble amount of data
	/// </summary>
	public enum MapZoom
	{
		ZoomLevel_0 = 1, // Approx 217m x 217m
		ZoomLevel_1,
		ZoomLevel_2,
		ZoomLevel_3,
		ZoomLevel_4,
		ZoomLevel_5,
		ZoomLevel_6,
		ZoomLevel_7,
		ZoomLevel_8,
		ZoomLevel_9,
		ZoomLevel_10
	}
	
	/// <summary>
	/// The map zoom level.
	/// </summary>
	public MapZoom mapZoom = MapZoom.ZoomLevel_0;	
	
	#endregion
	
	#region Constants
	
	/// <summary>
	/// The degree in meters. Used for conversion: 1 geographical degree == 111,319.9 meters approx.
	/// </summary>
	public static float degreeInMeters = 111319.9f;
	
	/// <summary>
	/// The base zoom. e.g. ZoomLevel_0 = degreeImMeters * baseZoom;
	/// </summary>
	private static float baseZoom = 0.0009765625f;
	
	#endregion
	
	#region MapPosition
	
	/// <summary>
	/// Map position. A class to store map positional data.
	/// </summary>
	public class MapPosition
	{
		/// <summary>
		/// The geographic position (Longitude, Lattiude).
		/// </summary>
		public Vector3 geographic;
		
		/// <summary>
		/// The world position in Unity coordinates.
		/// </summary>	
		public Vector3 world;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.MapPosition"/> class.
		/// </summary>
		public MapPosition()
		{			
			geographic = new Vector3(0.0f,0.0f,0.0f);
			world = new Vector3(0.0f,0.0f,0.0f);
		}
	}
	
	#endregion
	
	#region MapBounds
	
	/// <summary>
	/// Map bounds. A class to store the bounds( edges ) information.
	/// Also contains related helper functions.
	/// </summary>
	public class MapBounds
	{
		/// <summary>
		/// The minimum.
		/// </summary>
		public MapPosition min;
		
		/// <summary>
		/// The max.
		/// </summary>
		public MapPosition max;
		
		/// <summary>
		/// The center.
		/// </summary>
		public MapPosition center;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.MapBounds"/> class.
		/// </summary>
		public MapBounds()
		{
			min = new MapPosition();
			max = new MapPosition();
			center = new MapPosition();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Mapity.MapBounds"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="Mapity.MapBounds"/>.
		/// </returns>
		public override string ToString() 
		{
			return 	"World:"+min.world.x+","+min.world.z+","+max.world.x+","+max.world.z+"\n" +
					"Geographic:"+min.geographic.x+","+min.geographic.z+","+max.geographic.x+","+max.geographic.z+"\n";
		}
		
		/// <summary>
		/// Geographic to world coordinate. Converts a geographic coordinate to a world coordinate.
		/// </summary>
		/// <returns>
		/// The world coordinate.
		/// </returns>
		/// <param name='geographic'>
		/// Geographic position.
		/// </param>
		public Vector3 GeographicToWorldCoordinate( Vector3 geographic )
		{
			Vector3 world = new Vector3( 0.0f,0.0f,0.0f );
			
			geographic.x -= center.geographic.x;
			geographic.z -= center.geographic.z;			
			
			world.x = degreeInMeters * geographic.x;
			world.z = degreeInMeters * geographic.z;
			
			world.y = geographic.y;
			
			return world;
		}
	}
	
	/// <summary>
	/// The map bounds.
	/// </summary>
	public MapBounds mapBounds = new MapBounds();
	
	#endregion
	
	static Mesh CreateMesh(Vector3[] poly)

    {
		
		Vector2[] poly2D = new Vector2[poly.Length];
		for(int i=0;i<poly.Length;i++) {
			poly2D[i] = new Vector2(poly[i].x, poly[i].y);
		}
        Triangulator triangulator = new Triangulator(poly2D);

        int[] tris = triangulator.Triangulate();
		

        Mesh m = new Mesh();

        Vector3[] vertices = new Vector3[poly.Length*2];

        

        for(int i=0;i<poly.Length;i++)

        {

            vertices[i].x = poly[i].x;

            vertices[i].y = poly[i].y;

            vertices[i].z = -poly[i].z; // front vertex

            vertices[i+poly.Length].x = poly[i].x;

            vertices[i+poly.Length].y = poly[i].y;

            vertices[i+poly.Length].z = -poly[i].z+10;  // back vertex     

        }

        int[] triangles = new int[tris.Length*2+poly.Length*6];

        int count_tris = 0;

        for(int i=0;i<tris.Length;i+=3)

        {

            triangles[i] = tris[i+2];

            triangles[i+1] = tris[i+1];

            triangles[i+2] = tris[i];

        } // front vertices

        count_tris+=tris.Length;

        for(int i=0;i<tris.Length;i+=3)

        {

            triangles[count_tris+i] = tris[i]+poly.Length;

            triangles[count_tris+i+1] = tris[i+1]+poly.Length;

            triangles[count_tris+i+2] = tris[i+2]+poly.Length;

        } // back vertices

        count_tris+=tris.Length;

        for(int i=0;i<poly.Length;i++)

        {

          // triangles around the perimeter of the object

            int n = (i+1)%poly.Length;

			triangles[count_tris] = i;
            triangles[count_tris+1] = i + poly.Length;
            triangles[count_tris+2] = n + poly.Length;
            triangles[count_tris+3] = i;
            triangles[count_tris+4] = n + poly.Length;
            triangles[count_tris+5] = n;

            count_tris += 6;

        }

        m.vertices = vertices;

        m.triangles = triangles;

        m.RecalculateNormals();

        m.RecalculateBounds();

        m.Optimize();

        return m;

    }
	
	#region Tags
	
	/// <summary>
	/// Tags. Contains a Hashtable of all the OSM Tags
	/// </summary>
	public class Tags
	{
		/// <summary>
		/// The tags.
		/// </summary>
		Hashtable tags;	
		
		/// <summary>
		/// Adds the tag.
		/// </summary>
		/// <param name='k'>
		/// K. Key
		/// </param>
		/// <param name='v'>
		/// V. Value
		/// </param>
		public void AddTag (string k, string v) 
		{
			tags.Add(k,v);
		}
	
		/// <summary>
		/// Gets a specific tag.
		/// </summary>
		/// <returns>
		/// The tag.
		/// </returns>
		/// <param name='k'>
		/// K. Key
		/// </param>
		public string GetTag(string k) 
		{
			return tags[k] as string;	
		}
		
		/// <summary>
		/// Gets the tags Hashtable.
		/// </summary>
		/// <returns>
		/// The tags.
		/// </returns>
		public Hashtable GetTags()
		{ 
			return tags; 
		} 
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.Tags"/> class.
		/// </summary>
		public Tags()
		{
			tags = new Hashtable();
		}
	}
	
	#endregion
	
	#region MapNodes
	
	/// <summary>
	/// Map node. A Map Node is the basic map element. All other elements are contructed from Nodes.
	/// A Node defines a single geospatial point using a latitude and longitude.
	/// Nodes can be used to define standalone point features for example, a town or village.
	/// Nodes are also used to define the path of a Way.
	/// </summary>
	public class MapNode
	{
		/// <summary>
		/// The position. The Nodes position.
		/// </summary>
		public MapPosition position;
		
		/// <summary>
		/// The identifier. A unique ID used for HashTable look ups.
		/// </summary>
		public uint id;
		
		/// <summary>
		/// The tags.
		/// </summary>
		public Tags tags;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.MapNode"/> class.
		/// </summary>
		public MapNode()
		{
			position = new MapPosition();
			
			id = 0u;
			
			tags = new Tags();
		}		
	}
	
	/// <summary>
	/// The map nodes Hashtable.
	/// </summary>
	public Hashtable mapNodes = new Hashtable();
	
	#endregion
	
	#region MapWays
	
	/// <summary>
	/// Map way. A way is an ordered list of between 2 and 2,000 Nodes. 
	/// Ways are used to represent linear features, such as rivers or roads.
	/// Ways can also represent solid areas, such as buildings or forests. 
	/// In this case, the first and last node will be the same - a "closed way".
	/// Note that closed ways are occasionally linear loops, such as highway roundabouts, rather than 'filled' areas. 
	/// </summary>
	public class MapWay
	{
		/// <summary>
		/// The way map nodes ArrayList. The Nodes that make up this Way.
		/// </summary>
		public ArrayList wayMapNodes;
		
		/// <summary>
		/// The identifier. A unique ID used for HashTable look ups.
		/// </summary>
		public uint id;
		
		/// <summary>
		/// The tags.
		/// </summary>
		public Tags tags;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.MapWay"/> class.
		/// </summary>
		public MapWay()
		{
			wayMapNodes = new ArrayList();
			
			id = 0u;
			
			tags = new Tags();
		}
	}
	
	/// <summary>
	/// The map ways Hashtable.
	/// </summary>
	public Hashtable mapWays = new Hashtable();
	
	#endregion
	
	#region Highways
	
	/// <summary>
	/// Highway classification. An enum used to identify different road types.
	/// </summary>
	public enum HighwayClassification
	{
		Road, // Road of unknown classification ( Default )
		Motorway,
		MotorwayLink,
		Trunk,
		TrunkLink,
		Primary,
		PrimaryLink,
		Secondary,
		SecondaryLink,
		Tertiary,
		TertiaryLink,
		Pedestrian,
		Residential
	}
	
	/// <summary>
	/// Highway. The Highway class is a convient wrapper to a Way tagged as a Highway.
	/// </summary>
	public class Highway: MapWay
	{
		/// <summary>
		/// The classification.
		/// </summary>
		public HighwayClassification classification;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.Highway"/> class.
		/// </summary>
		public Highway()
		{
			// Default classification to road
			classification = HighwayClassification.Road;
		}
	}
	
	/// <summary>
	/// The highways Hashtable.
	/// </summary>
	public Hashtable highways = new Hashtable();
	
	#endregion
	
	#region Waterways
	
	/// <summary>
	/// Waterway classification. An enum used to identify different water way types.
	/// </summary>
	public enum WaterwayClassification
	{
		River, // Default
		Stream,
		Canal,
		Lake
	}
	
	/// <summary>
	/// Waterway. The Waterway class is a convient wrapper to a Way tagged as a Waterway.
	/// </summary>
	public class Waterway
	{
		/// <summary>
		/// The identifier. A unique ID used for HashTable look ups.
		/// </summary>
		public uint id;
		
		/// <summary>
		/// The classification.
		/// </summary>
		public WaterwayClassification classification;
		
		/// <summary>
		/// The waterway ways. List of MapWays defining a waterway
		/// </summary>
		public ArrayList waterwayWays;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.Waterway"/> class.
		/// </summary>
		public Waterway()
		{
			id = 0u;
			
			// Default classification to river
			classification = WaterwayClassification.River;
			
			waterwayWays = new ArrayList();
		}
	}
	
	/// <summary>
	/// The waterways Hashtable.
	/// </summary>
	public Hashtable waterways = new Hashtable();
	
	#endregion
	
	#region Relations
	
	/// <summary>
	/// Relation type. An enum used to identify different Relation types.
	/// </summary>
	public enum RelationType
	{
		Multipolygon, // Default
		Route,
		RouteMaster,
		Restriction,
		Boundary,
		Street,
		AssociatedStreet,
		PublicTransport,
		DestinationSign,
		Waterway,
		Enforcement
	}
	
	/// <summary>
	/// Map relation. A Relation is an all-purpose data structure that documents a relationship between two or more other objects.
	/// Simple examples include:
	/// A Route relation assembles the ways that form a cycle route, bus route or long-distance highway.
	/// A Multipolygon describes an area (the 'outer way') with holes (the 'inner ways').
	/// </summary>
	public class MapRelation
	{
		/// <summary>
		/// The relation nodes. List of MapNodes defining a relation
		/// </summary>
		public ArrayList relationNodes;
		
		/// <summary>
		/// The relation ways. List of MapWays defining a relation
		/// </summary>
		public ArrayList relationWays;
		
		/// <summary>
		/// The relation relations. List of MapRelations defining a relation
		/// </summary>
		public ArrayList relationRelations;
		
		/// <summary>
		/// The identifier. A unique ID used for HashTable look ups.
		/// </summary>
		public uint id;
		
		/// <summary>
		/// The tags.
		/// </summary>
		public Tags tags;
		
		/// <summary>
		/// The type.
		/// </summary>
		public RelationType type;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.MapRelation"/> class.
		/// </summary>
		public MapRelation()
		{
			relationNodes = new ArrayList();
			relationWays = new ArrayList();
			relationRelations = new ArrayList();
			
			id = 0u;
			
			tags = new Tags();
			
			type = RelationType.Multipolygon;
		}
	}
	
	/// <summary>
	/// The map relations Hashtable.
	/// </summary>
	public Hashtable mapRelations = new Hashtable();
	
	#endregion
	
	#region Buildings
	
	/// <summary>
	/// Building type. An enum used to identify different Building types.
	/// </summary>
	public enum BuildingType
	{
		Building, // Default
	}
	
	/// <summary>
	/// Map building. The Building class is a convient wrapper to a Way or Relation tagged as a Building.
	/// </summary>
	public class MapBuilding
	{		
		/// <summary>
		/// The identifier. A unique ID used for HashTable look ups.
		/// </summary>
		public uint id;
		
		/// <summary>
		/// The type.
		/// </summary>
		public BuildingType type;
		
		/// <summary>
		/// The building ways. List of MapWays defining a building
		/// </summary>
		public ArrayList buildingWays;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Mapity.MapBuilding"/> class.
		/// </summary>
		public MapBuilding()
		{			
			id = 0u;
			
			type = BuildingType.Building;
			
			buildingWays = new ArrayList();
		}
	}
	
	/// <summary>
	/// The map buildings Hashtable.
	/// </summary>
	public Hashtable mapBuildings = new Hashtable();
	
	#endregion
	
	#region Functions
	
	/// <summary>
	/// OnEnable this instance.
	/// </summary>
	void OnEnable()
	{
		if( Singleton == null )
		{
			Singleton = this;
		}
	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		if( autoLoad )
		{
			Load();
		}
	}
	
	/// <summary>
	/// Load. Starts the Loading Coroutine.
	/// </summary>
	public void Load()
	{
		StartCoroutine(LoadingCoroutine());
	}
	
	/// <summary>
	/// Determines whether this instance has loaded.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance has loaded; otherwise, <c>false</c>.
	/// </returns>
	public bool HasLoaded()
	{ 
		return hasLoaded;
	}
	
	/// <summary>
	/// The loading coroutine.
	/// </summary>
	/// <returns>
	/// The coroutine.
	/// </returns>
	IEnumerator LoadingCoroutine()
	{
		// Reset Mapity load flag
		hasLoaded = false;
		
		// Local successful load flag
		bool loadSuccessful = true;
		
		// Create a new XML document
    	XmlDocument xmlDoc = new XmlDocument();
		
		// Are we using the online version
		if( downloadMapData )
		{
			if( useDeviceLocation )
			{
				// First, check if user has location service enabled
				if (Input.location.isEnabledByUser)
				{
					// Start service before querying location
					Input.location.Start ();
					
					// Wait until service initializes
					int maxWait = 20;
					
					while ( Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) 
					{
						yield return new WaitForSeconds (1);
						maxWait--;
					}
					
					// Service didn't initialize in 20 seconds
					if (maxWait < 1) 
					{
						MapityLog("Timed out");
					}
					else // Not a timeout
					{
						// Connection has failed
						if (Input.location.status == LocationServiceStatus.Failed) 
						{
							MapityLog("Unable to determine device location");
						}
						// Access granted and location value could be retrieved
						else 
						{
							MapityLog("Location: " + Input.location.lastData.latitude + " " +
							       Input.location.lastData.longitude + " " +
							       Input.location.lastData.altitude + " " +
							       Input.location.lastData.horizontalAccuracy + " " +
							       Input.location.lastData.timestamp);
							
							location.x = Input.location.lastData.longitude;
							location.y = Input.location.lastData.latitude;
						}
						
						// Stop service since there is no need to query location updates continuously
						Input.location.Stop ();
					}
				}
			}
			// Calculate a bounds half - The base zoom level * the current zoom level
			float bounds = baseZoom * (int)mapZoom;
			
			float longitudeMin = location.x - bounds; // Minus a half bound
			float longitudeMax = location.x + bounds; // Plus a half bound
			float latitudeMin = location.y - bounds; // Minus a half bound
			float latitudeMax = location.y + bounds; // Plus a half bound
			
			//Load XML data from a URL
		    string url = openStreetMapApiUrl + "map?bbox=" 
						+ longitudeMin.ToString() + ","
						+ latitudeMin.ToString() + ","
						+ longitudeMax.ToString() + ","
						+ latitudeMax.ToString();
			
		    WWW www = new WWW(url);
		 
		    //Load the data and yield (wait) till it's ready before we continue executing the rest of this method.
		    yield return www;
			
			// If we have don't an error
		    if (www.error == null)
		    {
				//Sucessfully loaded the XML
				MapityLog("Loaded following data: " + www.text);
				
				//Create a new XML document out of the loaded data
				xmlDoc.LoadXml(www.text);
				
				// Should we save what we downloaded
				if( saveDownloadedMapData )
				{
		      		MapityLog("Saving downloaded map data...");
					
					// Save Data
					SaveMap( www.text );
				}
			}
			else // Otherwise something went wrong
			{
				// Mark this as unsuccessful
				loadSuccessful = false;
				
				//Failed to download data
				MapityLog("Failed to download map XML data: " + www.error);
			}
		}
		else // Otherwise use the local version
		{
#if !UNITY_WEBPLAYER
			// Load local XML data
			string xmlText = System.IO.File.ReadAllText(Application.dataPath + "/Mapity/MapData/" + localMapFileName.ToString() + ".mapity");
		    
			//Create a new XML document out of the loaded data
			xmlDoc.LoadXml(xmlText);
#else
			Debug.Log("Local map loading not supported for Webplayer builds");
				
			// Mark this as unsuccessful
			loadSuccessful = false;
#endif
		}
    
		// If we successfully loaded an xml file
		if( loadSuccessful )
		{			
			// Get the bounds of the map data
			CalculateBounds(xmlDoc);
			
      		//Point to the nodes and process them
      		//ProcessMap(xmlDoc); 
			yield return StartCoroutine( ProcessMap(xmlDoc) );
			
			foreach(MapBuilding mapBuilding in mapBuildings.Values ) {
				GameObject newBuilding = new GameObject();
				MeshRenderer meshRenderer = newBuilding.AddComponent<MeshRenderer>();
				MeshFilter newMeshFilter = newBuilding.AddComponent<MeshFilter>();
				//newBuilding.transform.parent = buildings.transform;
				
				List<Vector3> vertices = new List<Vector3>();
				foreach(MapWay mapWay in mapBuilding.buildingWays) {
					foreach(MapNode node in mapWay.wayMapNodes) {
						vertices.Add(new Vector3(node.position.world.x,node.position.world.z, node.position.world.y));
					}
				}
				
				// convert polygon to triangles
				
				Mesh mesh = CreateMesh(vertices.ToArray());
				if(mesh != null) {
					newMeshFilter.mesh = mesh;
					MeshCollider collider = newBuilding.AddComponent<MeshCollider>();
					collider.sharedMesh = mesh;
				}
				
				newBuilding.transform.Rotate(90,0,0);
				Vector3 buildingPos = newBuilding.transform.position;
				newBuilding.transform.position = new Vector3(buildingPos.x, buildingPos.y+10, buildingPos.z);
			}
			
		}
	}
	
	/// <summary>
	/// Saves the map.
	/// </summary>
	/// <param name='mapData'>
	/// Map data.
	/// </param>
	public void SaveMap(string mapData)
	{
#if !UNITY_WEBPLAYER
		// Save data
		System.IO.File.WriteAllText( Application.dataPath + "/Mapity/MapData/" + mapFileNameSavePrefix.ToString() + System.DateTime.Now.ToString("HHmmddMMMMyyyy") + ".mapity", mapData );
#else
		Debug.Log("Map Saving not supported for Webplayer builds");
#endif
	}
	
	/// <summary>
	/// Calculates the map bounds.
	/// </summary>
	/// <param name='xmlDoc'>
	/// Xml document.
	/// </param>
	private void CalculateBounds(XmlDocument xmlDoc)
	{
		XmlNode node = xmlDoc.SelectSingleNode("osm/bounds");
		
		if( node != null ) // Editing api returns a bounds element
		{
			// Query map data to calculate bounds
			mapBounds.min.geographic.x = Convert.ToSingle(node.Attributes.GetNamedItem("minlon").Value);
			mapBounds.min.geographic.z = Convert.ToSingle(node.Attributes.GetNamedItem("minlat").Value);
			mapBounds.max.geographic.x = Convert.ToSingle(node.Attributes.GetNamedItem("maxlon").Value);
			mapBounds.max.geographic.z = Convert.ToSingle(node.Attributes.GetNamedItem("maxlat").Value);
			
			// Calculate center
			mapBounds.center.geographic.x = mapBounds.min.geographic.x + ( (mapBounds.max.geographic.x - mapBounds.min.geographic.x) / 2 );
			mapBounds.center.geographic.z = mapBounds.min.geographic.z + ( (mapBounds.max.geographic.z - mapBounds.min.geographic.z) / 2 );
		}
		else // Otherwise work it out based on the zoom level
		{
			float bounds = baseZoom * (int)mapZoom;
			
			mapBounds.min.geographic.x = location.x - bounds;
			mapBounds.min.geographic.z = location.y - bounds;
			mapBounds.max.geographic.x = location.x + bounds;
			mapBounds.max.geographic.z = location.y + bounds;
			
			// Calculate center
			mapBounds.center.geographic.x = mapBounds.min.geographic.x + ( (mapBounds.max.geographic.x - mapBounds.min.geographic.x) / 2 );
			mapBounds.center.geographic.z = mapBounds.min.geographic.z + ( (mapBounds.max.geographic.z - mapBounds.min.geographic.z) / 2 );
		}
		MapityLog( mapBounds.ToString() );
	}
	 
	/// <summary>
	/// Processes the map.
	/// </summary>
	/// <param name='xmlDoc'>
	/// Xml document.
	/// </param>
	private IEnumerator ProcessMap(XmlDocument xmlDoc)
	{		
		XmlNodeList xmlNodes;
		
		// Select the nodes
		xmlNodes = xmlDoc.SelectNodes("osm/node");
				
		MapityLog("Processing Nodes");		
		
		// XML node for querying attributes so we can null check it
		XmlNode childNode = null;		
		
		// Strings used for height data queries
		String lats = "lats=";
		String lngs = "lngs=";
		
		// Array of temporary mapNodes that will have their height data queried
		ArrayList tempMapNodeList = new ArrayList();
		
    	foreach (XmlNode node in xmlNodes)
    	{			
			MapNode mapNode = new MapNode();
			
			// Get visible attribute
			childNode = node.Attributes.GetNamedItem("visible");
			
			// Only add visible nodes
			if( ( childNode == null ) || ( childNode.Value == "true" ) )
			{				
				// Get Node ID				
	      		mapNode.id = Convert.ToUInt32(node.Attributes.GetNamedItem("id").Value);
				MapityLog("Node ID:" + mapNode.id.ToString() );
				
				// Get Geographic coordinates
	      		mapNode.position.geographic.x = Convert.ToSingle(node.Attributes.GetNamedItem("lon").Value);
	      		mapNode.position.geographic.z = Convert.ToSingle(node.Attributes.GetNamedItem("lat").Value);
		  
				// Convert Geographic cordinated to Unity world coordinates
				mapNode.position.world = mapBounds.GeographicToWorldCoordinate( mapNode.position.geographic );
				
				// XML Nodes defining the Map Node tags
				XmlNodeList xmlChildNodes = node.SelectNodes("tag");
				
				foreach (XmlNode tagNode in xmlChildNodes)
    			{
					string nodeKey = tagNode.Attributes.GetNamedItem("k").Value;
					string nodeValue = tagNode.Attributes.GetNamedItem("v").Value;
					
					mapNode.tags.AddTag( nodeKey, nodeValue );
				}
				
				// Add mapNode to Hashtable of mapNodes
				mapNodes.Add( mapNode.id, mapNode );
				MapityLog("Added MapNode ID:" + mapNode.id );	
				
				// If we are querying height data
				if( queryHeightData )
				{
					// Add the node to the query list
					tempMapNodeList.Add( mapNode );
					
					// Construct the query string
					lngs += mapNode.position.geographic.x.ToString();
					lats += mapNode.position.geographic.z.ToString();
					
					// Query every 20 points( free account limit ) OR at the end of the list
					if( ( tempMapNodeList.Count % 20 == 0 ) || ( tempMapNodeList.Count == xmlNodes.Count-1 ) )
					{
						// Construct the URL
						string url = geonamesApiUrl
							+ lats
							+ "&"
							+ lngs
							+ "&username="
							+ geonamesUsername.ToString();
				
					    WWW www = new WWW(url);
					 
					    //Load the data and yield (wait) till it's ready before we continue executing the rest of this method.
					    yield return www;
						
						// If we have don't an error
					    if (www.error == null)
					    {
							//Sucessfully loaded the XML
							MapityLog("Height Query succeeded");
							
							// Convert the string to an int array
							int[] ia = www.text.Split(new string[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries ).Select(n => Convert.ToInt32(n)).ToArray();
							
							int count = 0;	
							
							// Set the height on every map node in our query list						
							foreach(MapNode tempnode in tempMapNodeList )
							{								
								tempnode.position.geographic.y = ia[count];
								tempnode.position.world.y = ia[count];
								count++;
							}
						}
						else // Otherwise something went wrong
						{						
							//Failed to download data
							MapityLog("Height Query failed");
							
							int count = 0;
							
							// Set the height on every map node in our query list to 0
							foreach(MapNode tempnode in tempMapNodeList )
							{								
								tempnode.position.geographic.y = 0;
								tempnode.position.world.y = 0;
								count++;
							}
						}
						
						// Clear the list ready for the next set of map nodes
						tempMapNodeList.Clear();
						
						// Reset the query string
						lats = "lats=";
						lngs = "lngs=";
						
						// Skip adding the commas on a new set of nodes
						continue;
					}
					
					// Construct query string, locations are comma delimited
					lats += ",";
					lngs += ",";
				}
	    	}			
		}
		
		// Select ways
		xmlNodes = xmlDoc.SelectNodes("osm/way");
		
		MapityLog("Processing Ways");		
		
    	foreach (XmlNode node in xmlNodes)
    	{			
			MapWay mapWay = new MapWay();
			
			// Get visible attribute
			childNode = node.Attributes.GetNamedItem("visible");
			
			// Only add visible nodes
			if( ( childNode == null ) || ( childNode.Value == "true" ) )
			{
				// Get Way ID				
	      		mapWay.id = Convert.ToUInt32(node.Attributes.GetNamedItem("id").Value);
				MapityLog("Way ID:" + mapWay.id.ToString() );
				
				// Nodes defining the way
				XmlNodeList xmlChildNodes = node.SelectNodes("nd");
				
				foreach (XmlNode refNode in xmlChildNodes)
    			{
					uint nodeKey = Convert.ToUInt32(refNode.Attributes.GetNamedItem("ref").Value);
					
					mapWay.wayMapNodes.Add( mapNodes[nodeKey] );
					
					MapityLog ("Current Way ID: " + mapWay.id + " Adding node ID: " + nodeKey);
				}
				
				// XML Nodes defining the Map Way tags
				xmlChildNodes = node.SelectNodes("tag");
				
				foreach (XmlNode tagNode in xmlChildNodes)
    			{
					string nodeKey = tagNode.Attributes.GetNamedItem("k").Value;
					string nodeValue = tagNode.Attributes.GetNamedItem("v").Value;
					
					mapWay.tags.AddTag( nodeKey, nodeValue );
					
					// Various Add functions takes an array list of ways because they 
					// can also be a relation and contain multiple ways, so create a temp one
					ArrayList ways = new ArrayList();
					
					// Add the map way
					ways.Add( mapWay );
					
					if( nodeKey == "highway" )
					{	
						//Add the highway
						AddHighway( mapWay, tagNode );
					}
					else if( nodeKey == "waterway" )
					{
						// Add the waterway
						AddWaterway( ways, tagNode, mapWay.id );						
					}
					else if( nodeKey == "natural" )
					{					
						if( nodeValue == "water" )
						{
							// Add the waterway
							AddWaterway( ways, tagNode, mapWay.id );		
						}
					}
					else if( nodeKey == "building" )
					{							
						// Add the building
						AddBuilding( ways, tagNode, mapWay.id );						
					}
				}				
				
				// Add mapWay to Hashtable of mapWays
				mapWays.Add( mapWay.id, mapWay );
				MapityLog("Added MapWay ID:" + mapWay.id );
			}
		}
		
		// Select relations
		xmlNodes = xmlDoc.SelectNodes("osm/relation");
		
		MapityLog("Processing Relations");		
		
    	foreach (XmlNode node in xmlNodes)
    	{			
			MapRelation mapRelation = new MapRelation();
			
			// Get visible attribute
			childNode = node.Attributes.GetNamedItem("visible");
			
			// Only add visible nodes
			if( ( childNode == null ) || ( childNode.Value == "true" ) )
			{
				// Get Relation ID				
	      		mapRelation.id = Convert.ToUInt32(node.Attributes.GetNamedItem("id").Value);
				MapityLog("Relation ID:" + mapRelation.id.ToString() );
				
				// Members defining the relation
				XmlNodeList xmlChildNodes = node.SelectNodes("member");
				
				foreach (XmlNode refNode in xmlChildNodes)
    			{
					string memberType = refNode.Attributes.GetNamedItem("type").Value;
					
					if( memberType == "node" )
					{
						uint nodeKey = Convert.ToUInt32(refNode.Attributes.GetNamedItem("ref").Value);
						
						mapRelation.relationNodes.Add( mapNodes[nodeKey] );
						
						MapityLog ( "Adding node ID: " + nodeKey + " to relation ID:" + mapRelation.id );
					}
					else if( memberType == "way" )
					{
						uint wayKey = Convert.ToUInt32(refNode.Attributes.GetNamedItem("ref").Value);
						
						mapRelation.relationWays.Add( mapWays[wayKey] );
						
						MapityLog ( "Adding way ID: " + wayKey + " to relation ID:" + mapRelation.id );
					}
					else if( memberType == "relation" )
					{
						uint relationKey = Convert.ToUInt32(refNode.Attributes.GetNamedItem("ref").Value);
						
						mapRelation.relationRelations.Add( mapRelations[relationKey] );
					
						MapityLog ( "Adding relation ID: " + relationKey + " to relation ID:" + mapRelation.id );
					}					
				}
				
				// XML Nodes defining the Map Relation tags
				xmlChildNodes = node.SelectNodes("tag");
				
				foreach (XmlNode tagNode in xmlChildNodes)
    			{
					string nodeKey = tagNode.Attributes.GetNamedItem("k").Value;
					string nodeValue = tagNode.Attributes.GetNamedItem("v").Value;
					
					mapRelation.tags.AddTag( nodeKey, nodeValue );
					
					if( nodeKey == "type" )
					{						
						switch (nodeValue)
						{
							case "multipolygon":
							{
								mapRelation.type = RelationType.Multipolygon;
								break;
							}
							case "route":
							{
								mapRelation.type = RelationType.Route;
								break;
							}
							case "restriction":
							{
								mapRelation.type = RelationType.Restriction;
								break;
							}
							case "street":
							{
								mapRelation.type = RelationType.Street;
								break;
							}
							case "associatedStreet":
							{
								mapRelation.type = RelationType.AssociatedStreet;
								break;
							}
							case "public_transport":
							{
								mapRelation.type = RelationType.PublicTransport;
								break;
							}
							case "destination_sign":
							{
								mapRelation.type = RelationType.DestinationSign;
								break;
							}
							case "waterway":
							{
								mapRelation.type = RelationType.Waterway;
								break;
							}
							case "enforcement":
							{
								mapRelation.type = RelationType.Enforcement;
								break;
							}
						}
						MapityLog ("Relation type: " + mapRelation.type.ToString() );
					}
					else if( nodeKey == "building" )
					{						
						AddBuilding( mapRelation.relationWays, tagNode, mapRelation.id );					
					}
					else if( nodeKey == "natural" )
					{						
						if( nodeValue == "water" )
						{
							AddWaterway( mapRelation.relationWays, tagNode, mapRelation.id );
						}
					}
				}				
				
				// Add mapRelation to Hashtable of mapRelations
				mapRelations.Add( mapRelation.id, mapRelation );
				MapityLog("Added MapRelation ID:" + mapRelation.id );
			}
		}
		
		MapityLog("Finished Processing");
		
		// We've successfully loaded
		hasLoaded = true;
		
	}// ProcessMap
	
	/// <summary>
	/// Adds the highway.
	/// </summary>
	/// <param name='mapWay'>
	/// Map way.
	/// </param>
	/// <param name='tagNode'>
	/// Tag node.
	/// </param>
	private void AddHighway( MapWay mapWay, XmlNode tagNode )
	{
		Highway highway = new Highway();
						
		highway.id = mapWay.id;
		
		highway.wayMapNodes = mapWay.wayMapNodes;
		
		string nodeValue = tagNode.Attributes.GetNamedItem("v").Value;
		
		switch (nodeValue)
		{
			case "motorway":
			{
				highway.classification = HighwayClassification.Motorway;
				break;
			}
			case "motorway_link":
			{
				highway.classification = HighwayClassification.MotorwayLink;
				break;
			}
			case "trunk":
			{
				highway.classification = HighwayClassification.Trunk;
				break;
			}
			case "trunk_link":
			{
				highway.classification = HighwayClassification.TrunkLink;
				break;
			}
			case "primary":
			{
				highway.classification = HighwayClassification.Primary;
				break;
			}
			case "primary_link":
			{
				highway.classification = HighwayClassification.PrimaryLink;
				break;
			}
			case "secondary":
			{
				highway.classification = HighwayClassification.Secondary;
				break;
			}
			case "secondary_link":
			{
				highway.classification = HighwayClassification.SecondaryLink;
				break;
			}
			case "tertiary":
			{
				highway.classification = HighwayClassification.Tertiary;
				break;
			}
			case "tertiary_link":
			{
				highway.classification = HighwayClassification.TertiaryLink;
				break;
			}
			case "living_street":
			case "pedestrian":
			{
				highway.classification = HighwayClassification.Pedestrian;
				break;
			}
			case "residential":
			{
				highway.classification = HighwayClassification.Residential;
				break;
			}							
		}
		MapityLog ("Highway classification: " + highway.classification.ToString() );
								
		// Add highway to Hashtable of highways
		highways.Add( highway.id, highway );
		MapityLog("Added Highway ID:" + highway.id );
	}
	
	/// <summary>
	/// Adds the waterway.
	/// </summary>
	/// <param name='ways'>
	/// Ways. List of MapWays.
	/// </param>
	/// <param name='tagNode'>
	/// Tag node. Tag Information.
	/// </param>
	/// <param name='id'>
	/// Identifier.
	/// </param>
	private void AddWaterway( ArrayList ways, XmlNode tagNode, uint id )
	{
		Waterway waterway = new Waterway();
						
		waterway.id = id;
		
		waterway.waterwayWays = ways;
		
		string nodeValue = tagNode.Attributes.GetNamedItem("v").Value;
		
		switch (nodeValue)
		{
			case "river":
			case "riverbank":
			{
				waterway.classification = WaterwayClassification.River;
				break;
			}
			case "stream":
			{
				waterway.classification = WaterwayClassification.Stream;
				break;
			}
			case "canal":
			{
				waterway.classification = WaterwayClassification.Canal;
				break;
			}	
			case "water":
			{
				waterway.classification = WaterwayClassification.Lake;
				break;
			}						
		}
		MapityLog ("Waterway classification: " + waterway.classification.ToString() );
								
		// Add waterway to Hashtable of waterways
		waterways.Add( waterway.id, waterway );
		MapityLog("Added Waterway ID:" + waterway.id );
	}
	
	/// <summary>
	/// Adds the building.
	/// </summary>
	/// <param name='ways'>
	/// Ways. List of MapWays.
	/// </param>
	/// <param name='tagNode'>
	/// Tag node. Tag Information.
	/// </param>
	/// <param name='id'>
	/// Identifier.
	/// </param>
	GameObject buildings = new GameObject();
	private void AddBuilding( ArrayList ways, XmlNode tagNode, uint id )
	{
		MapBuilding mapBuilding = new MapBuilding();
						
		// Use way ID for building ID since this way is defining a building
		mapBuilding.id = id;
		
		string nodeValue = tagNode.Attributes.GetNamedItem("v").Value;
		
		switch (nodeValue)
		{
			case "yes":
			{
				mapBuilding.type = BuildingType.Building;
				break;
			}						
		}
		MapityLog ("Building type: " + mapBuilding.type.ToString() );
		
		mapBuilding.buildingWays = ways;
		
		// Add mapBuilding to Hashtable of mapBuildings
		mapBuildings.Add( mapBuilding.id, mapBuilding );
		MapityLog("Added MapBuilding ID:" + mapBuilding.id );
	}
	
	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	public void OnDrawGizmos () 
	{
#if UNITY_EDITOR
		Handles.Label(transform.position, transform.name);
#endif
		
		Gizmos.DrawIcon(transform.position, "Map.png", true);
		
		if( gizmoDrawNodes )
		{
			foreach(MapNode mapNode in mapNodes.Values )
			{			
				Gizmos.DrawIcon(mapNode.position.world, "MapNode.png", true);
				//Handles.Label(mapNode.position.world, ""+mapNode.id);
			}
		}
	}
	
	/// <summary>
	/// Raises the draw gizmos selected event.
	/// </summary>
	public void OnDrawGizmosSelected () 
	{		
        Gizmos.color = Color.white;
		
		// Ways
		if( gizmoDrawWays )
		{
			foreach(MapWay mapWay in mapWays.Values )
			{			
	        	Gizmos.color = Color.white;
				
				for (int i = 0; i < mapWay.wayMapNodes.Count-1; i++)
				{
					MapNode fromNode = (MapNode)mapWay.wayMapNodes[i];
					MapNode toNode = (MapNode)mapWay.wayMapNodes[i+1];
					
					Gizmos.DrawLine( fromNode.position.world, toNode.position.world );
				}
			}
		}
		
		// Highways
		if( gizmoDrawHighWays || gizmoDrawHighWaysLabels )
		{
			foreach(Highway highway in highways.Values )
			{			
	        	Gizmos.color = Color.white;
				
				switch (highway.classification)
				{
					case HighwayClassification.Motorway:
					{
						Gizmos.color = Color.blue;
						break;
					}
					case HighwayClassification.MotorwayLink:
					{
						Gizmos.color = Color.blue;
						break;
					}
					case HighwayClassification.Trunk:
					{
						Gizmos.color = Color.green;
						break;
					}
					case HighwayClassification.TrunkLink:
					{
						Gizmos.color = Color.green;
						break;
					}
					case HighwayClassification.Primary:
					{
						Gizmos.color = Color.red;
						break;
					}
					case HighwayClassification.PrimaryLink:
					{
						Gizmos.color = Color.red;
						break;
					}
					case HighwayClassification.Secondary:
					{
						// Orange
						Gizmos.color = new Color(1.0f, 0.6f, 0.0f);
						break;
					}
					case HighwayClassification.SecondaryLink:
					{
						// Orange
						Gizmos.color = new Color(1.0f, 0.6f, 0.0f);
						break;
					}
					case HighwayClassification.Tertiary:
					{
						Gizmos.color = Color.yellow;
						break;
					}
					case HighwayClassification.TertiaryLink:
					{
						Gizmos.color = Color.yellow;
						break;
					}
					case HighwayClassification.Pedestrian:
					{
						Gizmos.color = Color.cyan;
						break;
					}
					case HighwayClassification.Residential:
					{
						Gizmos.color = Color.gray;
						break;
					}
				}
#if UNITY_EDITOR
				if( gizmoDrawHighWaysLabels )
				{
					MapNode way = (MapNode)highway.wayMapNodes[(int)highway.wayMapNodes.Count/2];
					Handles.Label(way.position.world, "Highway #" + highway.id.ToString() + ",\n" + highway.classification.ToString() );
				}
#endif
				
				if( gizmoDrawHighWays )
				{
					for (int i = 0; i < highway.wayMapNodes.Count-1; i++)
					{
						MapNode fromNode = (MapNode)highway.wayMapNodes[i];
						MapNode toNode = (MapNode)highway.wayMapNodes[i+1];
						
						Gizmos.DrawLine( fromNode.position.world, toNode.position.world );
					}	
				}
			}
		}
		
		// Waterways
		if( gizmoDrawWaterWays )
		{
			foreach(Waterway waterway in waterways.Values )
			{			
	        	Gizmos.color = Color.cyan;
			
				switch (waterway.classification)
				{
					case WaterwayClassification.River:
					{
						Gizmos.color = Color.cyan;
						break;
					}
					case WaterwayClassification.Stream:
					{
						Gizmos.color = Color.cyan;
						break;
					}
					case WaterwayClassification.Canal:
					{
						Gizmos.color = Color.cyan;
						break;
					}
				}
				for (int i = 0; i < waterway.waterwayWays.Count; i++)
				{
					MapWay waterwayWay = (MapWay)waterway.waterwayWays[i];
					
					if( waterwayWay != null )
					{
						for (int j = 0; j < waterwayWay.wayMapNodes.Count-1; j++)
						{
							MapNode fromNode = (MapNode)waterwayWay.wayMapNodes[j];
							MapNode toNode = (MapNode)waterwayWay.wayMapNodes[j+1];
							
							Gizmos.DrawLine( fromNode.position.world, toNode.position.world );
						}
					}
				}
			}
		}
		
		// Buildings
		if( gizmoDrawBuildings )
		{
			foreach(MapBuilding mapBuilding in mapBuildings.Values )
			{			
	        	Gizmos.color = Color.green;			
				
				switch (mapBuilding.type)
				{
					case BuildingType.Building:
					{
						Gizmos.color = Color.magenta;
						break;
					}
				}
				
				for (int i = 0; i < mapBuilding.buildingWays.Count; i++)
				{
					MapWay buildingWay = (MapWay)mapBuilding.buildingWays[i];
					
					if( buildingWay != null )
					{
						for (int j = 0; j < buildingWay.wayMapNodes.Count-1; j++)
						{
							MapNode fromNode = (MapNode)buildingWay.wayMapNodes[j];
							MapNode toNode = (MapNode)buildingWay.wayMapNodes[j+1];
							
							Gizmos.DrawLine( fromNode.position.world, toNode.position.world );
						}
					}
				}
			}
		}
		
		// Relations
		if( gizmoDrawRelations )
		{
			foreach(MapRelation mapRelation in mapRelations.Values )
			{			
	        	Gizmos.color = Color.yellow;
				
				for (int i = 0; i < mapRelation.relationWays.Count; i++)
				{
					MapWay relationWay = (MapWay)mapRelation.relationWays[i];
					
					if( relationWay != null )
					{
						for (int j = 0; j < relationWay.wayMapNodes.Count-1; j++)
						{
							MapNode fromNode = (MapNode)relationWay.wayMapNodes[j];
							MapNode toNode = (MapNode)relationWay.wayMapNodes[j+1];
							
							Gizmos.DrawLine( fromNode.position.world, toNode.position.world );
						}
					}
				}
			}
		}
    }
	
	/// <summary>
	/// Custom Debug Log we can switch off - Logging is very slow.
	/// </summary>
	/// <param name='message'>
	/// Message.
	/// </param>
	public void MapityLog( string message )
	{
		if( enableLogging )
		{
			Debug.Log( message );
		}
	}
	
	#endregion
}
