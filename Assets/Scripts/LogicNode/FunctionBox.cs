﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class FunctionBox : InteractableUIElement {
	public Text funcNameText;
	public List<IEnumerator> activeSequence;
	private GameObject utilityConnectedTo;
	public Transform canvasTransform;

	private bool outConnecting=false;
	private Vector3 lastDraggedPos;

	public List<UtilityBox> utilitiesConnected;
	public GameObject utilityDropdownPrefab;
	public GameObject bezierPrefab;
	GameObject activeBezierCurve;

	//PIN BUTTONS
	public Button inPin;
	public Button outPin;

	Transform originalParent;
	List<GameObject> outBezierList;

	Vector3 lastClickedPos;
	Vector3 startPos;
	Vector3 endPos;

	// Use this for initialization
	void Start () {
		canvasTransform = GameObject.FindGameObjectWithTag("Canvas").transform;
		outBezierList = new List<GameObject> ();
		activeSequence = new List<IEnumerator> ();
		utilitiesConnected = new List<UtilityBox> ();
		Debug.Log (transform.position.ToString ());

		originalParent = transform.parent;
	}
	
	// Update is called once per frame
	void Update () {
//		Debug.Log (this.GetComponent<RectTransform> ().anchoredPosition3D);
//		Debug.Log (Camera.main.ViewportToWorldPoint (this.GetComponent<RectTransform>().anchoredPosition3D));
	}

	public void SetupFunctionBox(string name)
	{
		gameObject.name = name;
		funcNameText.text = name; 
	}

	public IEnumerator ExecuteSequence()
	{
		//add to the sequence
		for (int j = 0; j < utilitiesConnected.Count; j++) {
			utilitiesConnected [j].AddCoroutineTo (this.gameObject);
		}
		//then execute the sequence
		IEnumerator[] seqArr = new IEnumerator[activeSequence.Count];
		for (int i = 0; i < activeSequence.Count; i++) {
			seqArr [i] = activeSequence [i];
		}
		yield return StartCoroutine(Sequence (seqArr));
		yield return null;
	}
	public static IEnumerator Sequence(params IEnumerator[] sequence)
	{
		//Debug.Log ("inside sequence " + " with length " + sequence.Length.ToString());
		for(int i = 0 ; i < sequence.Length; ++i)
		{
			while(sequence[i].MoveNext())
				yield return sequence[i].Current;
		}
	}


	IEnumerator PrintCoroutine(string arg)
	{
		Debug.Log (arg);
		yield return new WaitForSeconds(0.3f);
	}

	public void AddToSequence(IEnumerator coroutineToBeAdded)
	{
		activeSequence.Add (coroutineToBeAdded);
	}

	public void OutButtonPressed()
	{
		outConnecting = true;
		Debug.Log ("set outconnecting to true");
		startPos=Camera.main.ScreenToWorldPoint (GetMousePosInWorldCoords ());
	}

	bool CheckForUtilityConnection(GameObject droppedObj)
	{
		GameObject droppedPin;
		if (droppedObj != null) {
			//Debug.Log ("dropped object is : " + droppedObj.name);
			if (droppedObj.name.Contains ("InPin")) {
				droppedPin = droppedObj;
				droppedObj = droppedObj.transform.parent.gameObject;
				Debug.Log ("connected with utilitybox");
				droppedObj.GetComponent<UtilityBox> ().functionConnectedTo = this.gameObject;
//			droppedObj.GetComponent<UtilityBox> ().AddCoroutineTo (this.gameObject);

				//only add if it hasn't already been added before to this function
				bool foundMatch = false;
				for (int i = 0; i < utilitiesConnected.Count; i++) {
					if (utilitiesConnected [i] == droppedObj.GetComponent<UtilityBox> ())
						foundMatch = true;
				}

				if (!foundMatch) {
					utilitiesConnected.Add (droppedObj.GetComponent<UtilityBox> ());
					droppedPin.GetComponent<Image> ().color = Color.gray;
					outPin.GetComponent<Image> ().color = Color.gray;
					endPos = Camera.main.ScreenToWorldPoint (GetMousePosInWorldCoords ());
					return true;
				} else {
					return false;
				}
			} else
				return false;
		} else {
			return false;
		}
	
	}
		

	public override void OnPointerEnter(PointerEventData eventData)
	{
		transform.SetParent (canvasTransform, true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		transform.SetParent (originalParent, true);
	}
	public override void OnBeginDrag(PointerEventData data)
	{
		//Debug.Log("They started dragging " + this.name);
		lastClickedPos =  Camera.main.ScreenToWorldPoint (GetMousePosInWorldCoords());
		if (data.pointerPress.tag == "Button") {
			activeBezierCurve = Instantiate (bezierPrefab, lastClickedPos, Quaternion.identity) as GameObject;
			activeBezierCurve.GetComponent<Bezier_Curve> ().p0 = lastClickedPos;
		} 
	}

	public override void OnEndDrag(PointerEventData data)
	{
		//Debug.Log("Stopped dragging " + this.name);
		bool result=CheckForUtilityConnection (data.pointerEnter);
		if (result) {
			activeBezierCurve.GetComponent<Bezier_Curve> ().enabled = false;
//			activeBezierCurve.GetComponent<Bezier_Curve> ().isSet=true;
//			Debug.Log("my outpin: "+ outPin.gameObject.name);
//			activeBezierCurve.GetComponent<Bezier_Curve>().start=this.gameObject;
//			Debug.Log ("their outpin: " + utilitiesConnected [utilitiesConnected.Count - 1].gameObject.name);
//			activeBezierCurve.GetComponent<Bezier_Curve> ().end = utilitiesConnected [utilitiesConnected.Count - 1].gameObject;
			outBezierList.Add (activeBezierCurve);
			activeBezierCurve = null;
			Debug.Log ("bezier obj: " + outBezierList [outBezierList.Count - 1].gameObject.name);
			result = false;
		} else {
//			Debug.Log ("FALSE");
			Destroy (activeBezierCurve);
			activeBezierCurve = null;
		}

		transform.SetParent (originalParent, true);


//		Debug.Log ("out connecting is false");
		outConnecting = false;
	}
	public override void OnDrag(PointerEventData data)
	{
		if (data.pointerPress == this.gameObject) {
			GetComponent<RectTransform> ().anchoredPosition3D = Camera.main.ScreenToViewportPoint (GetMousePosInWorldCoords ());
			GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (GetComponent<RectTransform> ().anchoredPosition3D.x * Screen.width, GetComponent<RectTransform> ().anchoredPosition3D.y * Screen.height);
			lastDraggedPos = Camera.main.ScreenToWorldPoint (GetMousePosInWorldCoords ());
			if (activeBezierCurve != null) {
				activeBezierCurve.GetComponent<Bezier_Curve> ().p2 = lastDraggedPos;
			}

		}

	}
}
