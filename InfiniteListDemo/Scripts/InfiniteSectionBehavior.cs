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

public class InfiniteSectionBehavior : MonoBehaviour {
	public UILabel label;
	public UIPanel panel;
	public InfiniteListPopulator listPopulator;
	public int itemNumber;
	public int itemDataIndex;
	public bool isVisible = true;
	private BoxCollider thisCollider;

	// Use this for initialization
	void Start() 
	{
		thisCollider = GetComponent<BoxCollider>();
		transform.localScale = new Vector3(1,1,1);
	}
	void Update()
	{
		if(Mathf.Abs(listPopulator.draggablePanel.currentMomentum.y) >0)
		{
			CheckVisibilty();
		}
	}
	public bool verifyVisibility()
	{
		return(panel.IsVisible(label));
	}	

	void CheckVisibilty() 
	{
		bool currentVisibilty = panel.IsVisible(label);
		if(currentVisibilty != isVisible)
		{
			isVisible = currentVisibilty;
			thisCollider.enabled = isVisible;
			
			if(!isVisible)
			{
				listPopulator.StartCoroutine(listPopulator.ItemIsInvisible(itemNumber));
			}
		}
	}
}
