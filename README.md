Description
=================
A component that runs on top of NGUI's UIScrollView & UITable classes for Unity3D (i.e. It requires NGUI &amp; Unity3D) can be used with dynamic data.

Instead of instantiating a Prefab for each row in the list we are instantiating a fixed pool of objects that will be 
reused according to the scroll direction.

Best suited for Mobile (tested on both iOS & Android).

Features
============

* Infinite scrolling with clipping panel
* **Only fixed height cells are supported for now**
* Basic Scroll indicator
* Quick jump (selecting a start point)
* Simple sections implementation **(section height need to be equal to the item height)**
* Click item callbacks
* **Tested on Unity v4.2.2 & Unity v4.3 and NGUI v3.0.9f7**


The Demo
============
This demo package requires both Unity3D <http://unity3d.com> and NGUI <http://www.tasharen.com/?page_id=140> to be installed.
video: <https://www.youtube.com/watch?v=5xFVJqzp0kY>

To run the demo:

1. Create a new Unity Project (preferably mobile i.e. iOS or Android)
2. Import NGUI **including its examples** as the demo uses the atlas's from the examples
3. Import InfiniteListDemo folder or simply double click on InfiniteListDemoPackage
4. Run the scene (InfiniteListDemo)

The demo package is released under the MIT License:
<http://opensource.org/licenses/MIT>

Example of app using this component is Avatar Messenger for Android (Contacts list view) which is a free app on Google Play: <https://play.google.com/store/apps/details?id=com.orange.labs.avachat>

Main Classes & Methods in the Demo
===========
===
##InfiniteListPopulator 
The main controller script that can be attached to a gameobject (e.g. the panel)

Some of the main methods included:

====
Initializes the list (also can be called to refresh the list with new data)

	public void InitTableView(ArrayList inDataList, List<int> inSectionsIndices, int inStartIndex)


Parameters:

* inDataList: the generic list of our data (you can change it to any type you want… just make sure to change the member variables types for dataList & OriginalData)* inNumberOfSections: number of sections (int)
* inSectionIndices: List of integers. The start index of each section (not as fancy as indexpath in iOS but did the job for me)
* inStartIndex: where to start

===
Refresh the list without changing the data (list start at startIndex value)

	public void RefreshTableView()

===
Individual methods for changing the parameters if needed


	public void SetStartIndex(int inStartIndex)
	public void SetOriginalData(ArrayList inDataList)
	public void SetSectionIndices(List<int> inSectionsIndices)


===
You can include section titles values.. or if you have more detailed sections seperators you can change the implementation of PopulateListSectionWithIndex

	string GetTitleForSection(int i)
	void PopulateListSectionWithIndex(Transform item, int index)



====
You can do your implementation of what to populate your row item with (in the demo we simply set a label to string value from our datalist array). Simply change InfiniteItemBehaviour (mentioned later below) to include more items as you want.

	void PopulateListItemWithIndex(Transform item, int dataIndex)

===	
Events that can be listened to.

	public event InfiniteItemIsPressed InfiniteItemIsPressedEvent;
	public event InfiniteItemIsClicked InfiniteItemIsClickedEvent;

====

##InfiniteItemBehaviour and InfiniteSectionBehaviour
Scripts attached to the row item prefab & section prefab **(Note: the item prefab need to be tagged as "listItem" and the section prefab as "listSection")** 

Both checks for the visiblity of the item when the list start scrolling and notifiy the InfiniteListPopulator when the items becomes invisible. you can change use them as a template and include more labels, sprites or textures.
