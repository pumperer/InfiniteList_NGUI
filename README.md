Description
=================
====
A component that runs on top of NGUI's UIScrollView & UITable classes for Unity3D (i.e. It requires NGUI &amp; Unity3D) can be used with dynamic data

Instead of instantiating a Prefab for each row in the list we are instantiating a fixed pool of objects that will be 
resused according to the scroll direction.



Features
============
====

* Infinite scrolling with clipping panel
* **Only fixed height cells are supported for now**
* Basic Scroll indicator
* Quick jump (selecting a start point)
* Simple sections implementation **(section height need to be equal to the item height)**
* Click item callbacks
* **Tested on Unity v4.2.2 & Unity v4.3 and NGUI v3.0.9c**


The Demo
============
===
This demo package requires both Unity3D <http://unity3d.com> and NGUI <http://www.tasharen.com/?page_id=140> to be installed

To run the demo:

1. Create a new Unity Project
2. Import NGUI **including its examples** as the demo uses the atlas's from the examples
3. Import InfiniteListDemo folder or simply double click on InfiniteListDemoPackage
4. Run the scene (InfiniteListDemo)

The demo package is released under the MIT License:
<http://opensource.org/licenses/MIT>

Main Classes & Methods
===========
===
##InfiniteListPopulator 
The main controller script that can be attached to a gameobject (e.g. the panel)

Some of the main methods included:

====
    	public void InitTableView(List<string> inDataList, int inNumberOfSections, List<int> inSectionsIndices)
Initializes the list (also can be called to refresh the list with new data)
####parameters:
* inDataList: the List of strings we want to display (you can change it to any type you want… just make sure to change the member variables types for dataList & OriginalData)
* inNumberOfSections: number of sections (int)
* inSectionIndices: List of integers. The start index of each section (not as fancy as indexpath in iOS but did the job for me)

===

   	  public void SetCursor(int inCursor)

marks where to start reading the data list (the value is 0 if not set)
####parameters:
* inCursor: int 

===
	public int GetRealIndexForItem(int itemDataIndex)

**Due to the current implementation of sections:**

GetRealIndexForItem used to get the data index excluding sections seperators

===	


	public delegate void InfiniteItemIsPressed(int itemDataIndex, bool isDown);
	public event InfiniteItemIsPressed InfiniteItemIsPressedEvent;
	
	public delegate void InfiniteItemIsClicked(int itemDataIndex);
	public event InfiniteItemIsClicked InfiniteItemIsClickedEvent;

Events that can be listened to if required

====

##InfiniteItemBehaviour and InfiniteSectionBehaviour
Scripts attached to the row item prefab & section prefab **(Note: the item prefab need to be tagged as "listItem" and the section prefab as "listSection")** 

Both checks for the visiblity of the item when the list start scrolling and notifiy the InfiniteListPopulator when the items becomes invisible