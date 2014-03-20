/* The MIT License (MIT)
	
	Copyright (c) 2014 Orange Labs UK
		
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class InfiniteListPopulator : MonoBehaviour {
	// events to listen to if needed...
	public delegate void InfiniteItemIsPressed(int itemDataIndex, bool isDown);
	public event InfiniteItemIsPressed InfiniteItemIsPressedEvent;

	public delegate void InfiniteItemIsClicked(int itemDataIndex);
	public event InfiniteItemIsClicked InfiniteItemIsClickedEvent;

	//Prefabs
	const string listItemTag = "listItem";
	const string listSectionTag = "listSection";
	public Transform itemPrefab;
	public Transform sectionPrefab;
	// NGUI Controllers
	public UITable table;
	public UIScrollView draggablePanel;
	public Transform scrollIndicator;
	private int scrollCursor = 0;
	// pool
	private int poolSize = 6;
	private List<Transform> itemsPool = new List<Transform>();
	private int extraBuffer = 10;
	private float cellHeight = 94f;// at the moment we support fixed height... insert here or measure it
	//our data
	private List<string> originalData = new List<string>();
	private List<string> dataList = new List<string>();
	private int startIndex = 0; // where to start
	private Hashtable dataTracker = new Hashtable();// hashtable to keep track of what is being displayed
	//sections
	private int numberOfSections = 0;
	private List<int> sectionsIndices = new List<int>();
	#region Start & Update for MonoBehaviour
	void Start () {
		// check prefabs
		if(itemPrefab == null)
			Debug.LogError("InfiniteListPopulator:: itemPrefab is not assigned");
		else if(!itemPrefab.tag.Equals(listItemTag))
			Debug.LogError("InfiniteListPopulator:: itemPrefab tag should be "+listItemTag);
		if(sectionPrefab == null)
			Debug.LogError("InfiniteListPopulator:: sectionPrefab is not assigned");
		else if(!sectionPrefab.tag.Equals(listSectionTag))
			Debug.LogError("InfiniteListPopulator:: sectionPrefab tag should be "+listSectionTag);
	
		// init pool
		RefreshPool();

		// just for the demo
		List<string> dataList = new List<string>();
		for(int i= 0; i<500 ;i++)
		{
			string number = "row"+i;
			dataList.Add(number);
		}	
		
		// some sections
		List<int> indices = new List<int>();
		indices.Add(0);
		indices.Add(10);
		InitTableView(dataList,2,indices);
	}
	// Update is called once per frame
	void Update () {
		
		// scroll indicator stuff... TODO: Add switch to turn it ON/OFF
		if(Mathf.Abs(draggablePanel.currentMomentum.y)>0 && scrollIndicator.GetComponent<TweenAlpha>().from > 0)
		{
			scrollIndicator.GetComponent<TweenAlpha>().from = 0;
			scrollIndicator.GetComponent<TweenAlpha>().to = 0.25f;
			scrollIndicator.GetComponent<TweenAlpha>().duration = 0.5f;
			scrollIndicator.GetComponent<TweenAlpha>().enabled = true;
			scrollIndicator.GetComponent<TweenAlpha>().delay = 0f;

			TweenAlpha.Begin<TweenAlpha>(scrollIndicator.gameObject,0f);
		}
		if(Mathf.Abs(draggablePanel.currentMomentum.y)== 0 && scrollIndicator.GetComponent<TweenAlpha>().to >0)
		{
			scrollIndicator.GetComponent<TweenAlpha>().from = 0.25f;
			scrollIndicator.GetComponent<TweenAlpha>().to = 0;
			scrollIndicator.GetComponent<TweenAlpha>().duration = 0.5f;
			scrollIndicator.GetComponent<TweenAlpha>().enabled = true;
			scrollIndicator.GetComponent<TweenAlpha>().delay = 1f;

			TweenAlpha.Begin<TweenAlpha>(scrollIndicator.gameObject,0.25f);

		}
		float posY = 0;
		posY = -scrollCursor/(float)dataList.Count * draggablePanel.panel.baseClipRegion.w;
		scrollIndicator.localPosition = new Vector3(scrollIndicator.localPosition.x,posY,scrollIndicator.localPosition.z);
	}
	#endregion
	#region Test Examples
	public void NoSections()
	{
		SetStartIndex(0);
		InitTableView(originalData,0,new List<int>());
	}
	public void JumpTo30()
	{
		SetStartIndex(30);
		InitTableView(originalData,sectionsIndices.Count,sectionsIndices);
	}
	public void ThreeSections()
	{
		List<int> indices = new List<int>();
		indices.Add(0);
		indices.Add(5);
		indices.Add(10);
		SetStartIndex(0);
		InitTableView(originalData,indices.Count,indices);
	}
	#endregion

	#region Infinite List Management & scrolling
	public void SetStartIndex(int inStartIndex)
	{
		startIndex = GetJumpIndexForItem(inStartIndex);
	}
	public void InitTableView(List<string> inDataList, int inNumberOfSections, List<int> inSectionsIndices)
	{
		RefreshPool();
		scrollCursor = startIndex;
		dataTracker.Clear();
		originalData = new List<string>(inDataList);
		dataList = new List<string>(inDataList);
		numberOfSections = inNumberOfSections;
		sectionsIndices = inSectionsIndices;
		// do we have a section? then inject 'special' data inside the date list
		if(numberOfSections > 0)
		{
			
			for(int i = 0; i< numberOfSections; i++)
			{
				sectionsIndices[i] +=i;
				if(sectionsIndices[i]<dataList.Count)
					dataList.Insert(sectionsIndices[i],null);// testing with null data
			}
		}

		int j = 0;
		for(int i=startIndex; i<dataList.Count; i++)
		{
			Transform item = GetItemFromPool(j);
			if(item!=null)
			{
				// is it a section index?
				if(sectionsIndices.Contains(i) && item.tag.Equals(listItemTag))
				{
					// change item to section
					InitSection(item,i,j);
//					Object.Destroy(item.gameObject);
//					item = Instantiate(sectionPrefab) as Transform;
//					item.GetComponent<InfiniteSectionBehavior>().itemNumber = j;
//					item.GetComponent<InfiniteSectionBehavior>().label.text = GetTitleForSection(sectionsIndices.IndexOf(i));
//
//					item.name = "item"+i;
//					item.parent = table.transform;
//					item.GetComponent<InfiniteSectionBehavior>().itemDataIndex = i;
//					item.GetComponent<InfiniteSectionBehavior>().listPopulator = this;
//					item.GetComponent<InfiniteSectionBehavior>().panel = draggablePanel.panel;
//
//					itemsPool[j] = item;
//					dataTracker.Add(itemsPool[j].GetComponent<InfiniteSectionBehavior>().itemDataIndex,itemsPool[j].GetComponent<InfiniteSectionBehavior>().itemNumber);

				}
				else
				{
					InitListItemWithIndex(item,i,j);
//					item.GetComponent<InfiniteItemBehavior>().itemDataIndex = i;
//					item.GetComponent<InfiniteItemBehavior>().listPopulator = this;
//					item.GetComponent<InfiniteItemBehavior>().panel = draggablePanel.panel;
//					item.name = "item"+i;
//					item.GetComponent<InfiniteItemBehavior>().label.text = dataList[i];
//					dataTracker.Add(itemsPool[j].GetComponent<InfiniteItemBehavior>().itemDataIndex,itemsPool[j].GetComponent<InfiniteItemBehavior>().itemNumber);

				}
				Debug.Log(item.name+"::"+item.tag);
				
				j++;

			}
			else // end of pool
			{
				
				break;
			}
		}

		// at the moment we are repositioning the list after a delay... repositioning immediatly messes up the table when refreshing... no clue why...
		Invoke("RepositionList",0.1f);
	}
	
	void RepositionList()
	{

		table.Reposition();
		// make sure we have a correct poistion sequence
		draggablePanel.SetDragAmount(0,0, false);

		for (int i = 0;i<itemsPool.Count;i++)
		{
			Transform item = itemsPool[i];
			item.localPosition = new Vector3(item.localPosition.x,-((cellHeight/2) + i * cellHeight),item.localPosition.z);
		}
	}
	void InitSection(Transform item, int dataIndex, int poolIndex)
	{
		Object.Destroy(item.gameObject);
		item = Instantiate(sectionPrefab) as Transform;
		item.GetComponent<InfiniteSectionBehavior>().itemNumber = poolIndex;
		item.GetComponent<InfiniteSectionBehavior>().label.text = GetTitleForSection(sectionsIndices.IndexOf(dataIndex));
		
		item.name = "item"+dataIndex;
		item.parent = table.transform;
		item.GetComponent<InfiniteSectionBehavior>().itemDataIndex = dataIndex;
		item.GetComponent<InfiniteSectionBehavior>().listPopulator = this;
		item.GetComponent<InfiniteSectionBehavior>().panel = draggablePanel.panel;
		
		itemsPool[poolIndex] = item;
		dataTracker.Add(itemsPool[poolIndex].GetComponent<InfiniteSectionBehavior>().itemDataIndex,itemsPool[poolIndex].GetComponent<InfiniteSectionBehavior>().itemNumber);

	}
	void ChangeSectionToItem(Transform item, int newIndex,int oldIndex)
	{
		int j = 0;
		Vector3 lastPosition = Vector3.zero;
		j = item.GetComponent<InfiniteSectionBehavior>().itemNumber;
		lastPosition = item.localPosition;
		Object.Destroy(item.gameObject);
		item = Instantiate(itemPrefab) as Transform;
		item.parent = table.transform;
		item.localPosition = lastPosition;
		item.GetComponent<InfiniteItemBehavior>().itemNumber = j;
		item.GetComponent<InfiniteItemBehavior>().listPopulator = this;
		item.GetComponent<InfiniteItemBehavior>().panel = draggablePanel.panel;
		itemsPool[j] = item;

		PopulateListItemWithIndex(item,newIndex,oldIndex);
	}
	void InitListItemWithIndex(Transform item, int dataIndex, int poolIndex)
	{
		item.GetComponent<InfiniteItemBehavior>().itemDataIndex = dataIndex;
		item.GetComponent<InfiniteItemBehavior>().listPopulator = this;
		item.GetComponent<InfiniteItemBehavior>().panel = draggablePanel.panel;
		item.name = "item"+dataIndex;
		item.GetComponent<InfiniteItemBehavior>().label.text = dataList[dataIndex];
		dataTracker.Add(itemsPool[poolIndex].GetComponent<InfiniteItemBehavior>().itemDataIndex,itemsPool[poolIndex].GetComponent<InfiniteItemBehavior>().itemNumber);

	}
	void PopulateListItemWithIndex(Transform item, int newIndex,int oldIndex)
	{
		if(newIndex <oldIndex)
			item.localPosition += new Vector3(0,(poolSize)*cellHeight,0);
		else
			item.localPosition -= new Vector3(0,(poolSize)*cellHeight,0);

		item.GetComponent<InfiniteItemBehavior>().label.text = dataList[newIndex];
		item.GetComponent<InfiniteItemBehavior>().itemDataIndex=newIndex;
		item.name = "item"+(newIndex);
		dataTracker.Add(newIndex,(int)(dataTracker[oldIndex]));
		dataTracker.Remove(oldIndex);
	}
	void ChangeItemToSection(Transform item, int newIndex,int oldIndex)
	{
		int j = 0;
		Vector3 lastPosition = Vector3.zero;
		if(item.tag.Equals(listItemTag))
			j = item.GetComponent<InfiniteItemBehavior>().itemNumber;
		if(item.tag.Equals(listSectionTag))
			j = item.GetComponent<InfiniteSectionBehavior>().itemNumber;
		
		lastPosition = item.localPosition;
		Object.Destroy(item.gameObject);
		item = Instantiate(sectionPrefab) as Transform;
		item.parent = table.transform;

		item.localPosition = lastPosition;
		if(newIndex <oldIndex)
			item.localPosition += new Vector3(0,(poolSize)*cellHeight,0);
		else
			item.localPosition -= new Vector3(0,(poolSize)*cellHeight,0);
		
		item.name = "item"+(newIndex);
		item.GetComponent<InfiniteSectionBehavior>().itemNumber = j;
		item.GetComponent<InfiniteSectionBehavior>().label.text = GetTitleForSection(sectionsIndices.IndexOf(newIndex));
		item.GetComponent<InfiniteSectionBehavior>().itemDataIndex = newIndex;
		item.GetComponent<InfiniteSectionBehavior>().listPopulator = this;
		item.GetComponent<InfiniteSectionBehavior>().panel = draggablePanel.panel;

		itemsPool[j] = item;
		dataTracker.Add(newIndex,(int)(dataTracker[oldIndex]));
		dataTracker.Remove(oldIndex);
	}


	private bool isUpdatingList = false;
	
	public IEnumerator ItemIsInvisible(int itemNumber)
	{
		if(isUpdatingList) yield return null;
		isUpdatingList = true;
		if(dataList.Count > poolSize)// we need to do something "smart"... 
		{
			Transform item = itemsPool[itemNumber];
			int itemDataIndex = 0;
			if(item.tag.Equals(listItemTag))
				itemDataIndex = item.GetComponent<InfiniteItemBehavior>().itemDataIndex;
			if(item.tag.Equals(listSectionTag))
				itemDataIndex = item.GetComponent<InfiniteSectionBehavior>().itemDataIndex;

			int indexToCheck=0;
			InfiniteItemBehavior infItem = null;
			InfiniteSectionBehavior infSection = null;
			if(dataTracker.ContainsKey(itemDataIndex+1))
			{
				infItem = itemsPool[(int)(dataTracker[itemDataIndex+1])].GetComponent<InfiniteItemBehavior>();
				infSection = itemsPool[(int)(dataTracker[itemDataIndex+1])].GetComponent<InfiniteSectionBehavior>();

				if((infItem != null && infItem.verifyVisibility()) || (infSection != null && infSection.verifyVisibility()))
				{
					// dragging upwards (scrolling down)
					indexToCheck = itemDataIndex -(extraBuffer/2);
					if(dataTracker.ContainsKey(indexToCheck))
					{
						//do we have an extra item(s) as well?
						for(int i = indexToCheck; i>=0; i--)
						{
							if(dataTracker.ContainsKey(i))
							{
								infItem = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteItemBehavior>();
								infSection = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteSectionBehavior>();
								if((infItem != null && !infItem.verifyVisibility()) || (infSection != null && !infSection.verifyVisibility()))
								{
									item = itemsPool[(int)(dataTracker[i])];
									if((i)+poolSize < dataList.Count && i>-1)
									{
										// is it a section index?
										if(sectionsIndices.Contains(i+poolSize))
										{
											// change item to section
											ChangeItemToSection(item,i+poolSize,i);
										}
										else if(item.tag.Equals(listSectionTag))
										{
											// change section to item
											ChangeSectionToItem(item,i+poolSize,i);
										}
										else
										{
											PopulateListItemWithIndex(item,i+poolSize,i);
										}
									}
								}
							}
							else
							{
								scrollCursor = itemDataIndex-1;
								break;
							}
						}
					}
				}
			}
			if(dataTracker.ContainsKey(itemDataIndex-1))
			{
				infItem = itemsPool[(int)(dataTracker[itemDataIndex-1])].GetComponent<InfiniteItemBehavior>();
				infSection = itemsPool[(int)(dataTracker[itemDataIndex-1])].GetComponent<InfiniteSectionBehavior>();

				if((infItem != null && infItem.verifyVisibility()) || (infSection != null && infSection.verifyVisibility()))
				{
					//dragging downwards check the item below
					indexToCheck = itemDataIndex +(extraBuffer/2);
					
					if(dataTracker.ContainsKey(indexToCheck))
					{
						// if we have an extra item
						for(int i = indexToCheck; i<dataList.Count; i++)
						{
							if(dataTracker.ContainsKey(i))
							{
								infItem = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteItemBehavior>();
								infSection = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteSectionBehavior>();
								if((infItem != null && !infItem.verifyVisibility()) || (infSection != null && !infSection.verifyVisibility()))
								{
									item = itemsPool[(int)(dataTracker[i])];
									if((i)-poolSize > -1 && (i) < dataList.Count)
									{
										// is it a section index?
										if(sectionsIndices.Contains(i-poolSize))
										{
											// change item to section
											ChangeItemToSection(item,i-poolSize,i);
										}
										else if(item.tag.Equals(listSectionTag))
										{
											// change section to item
											ChangeSectionToItem(item,i-poolSize,i);
										}
										else
										{
											PopulateListItemWithIndex(item,i-poolSize,i);
										}
									}
								}
							}
							else
							{
								scrollCursor = itemDataIndex+1;
								break;
							}
						}
					}
				}
			}
		}
		isUpdatingList = false;
	}
	#endregion
	
	#region Pool & sections Management
	string GetTitleForSection(int i)
	{
		return "Section "+i;
	}
	Transform GetItemFromPool(int i)
	{
		if(i >= 0 && i< poolSize)
		{
			itemsPool[i].gameObject.SetActive(true);
			return itemsPool[i];
		}
		else
			return null;
	}

	void RefreshPool()
	{
		poolSize = (int)(draggablePanel.panel.baseClipRegion.w/cellHeight) + extraBuffer;
		Debug.Log("REFRESH POOL SIZE:::"+poolSize);
		// destroy current items
		for(int i=0; i< itemsPool.Count; i++) 
		{
			Object.Destroy(itemsPool[i].gameObject);
		}
		itemsPool.Clear();
		for(int i=0; i< poolSize; i++) // the pool will use itemPrefab as a default
		{
			Transform item = Instantiate(itemPrefab) as Transform;
			item.gameObject.SetActive(false);
			item.GetComponent<InfiniteItemBehavior>().itemNumber = i;
			item.name = "item"+i;
			item.parent = table.transform;
			itemsPool.Add(item);
		}
		
	}
	#endregion
	#region items callbacks and helpers
	int GetJumpIndexForItem(int itemDataIndex)
	{
		// find real data index
		if(numberOfSections>0 && itemDataIndex > sectionsIndices[0])
		{
		
			for(int index = numberOfSections-1; index >=0; index--)
			{
				if(itemDataIndex > sectionsIndices[index])
				{
					itemDataIndex = itemDataIndex + (index+1);
					break;
				}
				
			}
		}
		return itemDataIndex;
	}

	public int GetRealIndexForItem(int itemDataIndex)
	{
		// find real data index
		if(numberOfSections>0 && itemDataIndex > sectionsIndices[0])
		{
		
			for(int index = numberOfSections-1; index >=0; index--)
			{
				if(itemDataIndex > sectionsIndices[index])
				{
					itemDataIndex = itemDataIndex - (index+1);
					break;
				}
				
			}
		}
		return itemDataIndex;
	}
	public void itemIsPressed(int itemDataIndex,bool isDown)
	{
		itemDataIndex = GetRealIndexForItem(itemDataIndex);
		Debug.Log("Pressed down item "+ itemDataIndex +" "+isDown);

		if(InfiniteItemIsPressedEvent!=null)
			InfiniteItemIsPressedEvent(itemDataIndex,isDown);
	}
	public void itemClicked(int itemDataIndex)
	{
		itemDataIndex = GetRealIndexForItem(itemDataIndex);
		Debug.Log("Clicked item "+ itemDataIndex);

		if(InfiniteItemIsClickedEvent!=null)
			InfiniteItemIsClickedEvent(itemDataIndex);
	}
	#endregion

}
