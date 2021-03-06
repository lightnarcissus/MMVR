﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
public class LogicNodeManager : MonoBehaviour {

	public GameObject functionBoxPrefab;
	public GameObject basicLayerPrefab;
	public GameObject utilityBoxPrefab;
	public GameObject beginBoxPrefab;
	public GameObject tickBoxPrefab;

	public Transform layerParent;

	public Transform logicParent;
	public List<GameObject> functionBoxList;

	public GameObject[] utilityList;
	public Dropdown utilityDropdown;

	public Canvas logicCanvas;
	public GameObject variableContent;
	public List<GameObject> variableList;

	public Dictionary<string,GameObject> logicLayerDict;

	public GameObject selectedObjController;
	private GameObject activePlaygroundObj;
	private GameObject selectedVariable;

	public GameObject varPlayground;
	int variableIndex=0;

	public List<FunctionBox> funcSequence;

	public GameObject variableGroupPrefab;

	public Vector3 beginBoxSpawnPos;
	public Vector3 tickBoxSpawnPos;
	private GameObject beginBox;
	private GameObject tickBox;

	GameObject activeLayer;

	public Button sandboxButton;

	private int index;

	public delegate void LogicEvent();
	public static event LogicEvent OnVariableUpdate;

	private static LogicNodeManager _instance;
	public static LogicNodeManager Instance{
		get{
			return _instance;
		}
	}


	public void SetSelectedVariable(GameObject varObj,GameObject playgroundObj)
	{
		if (activePlaygroundObj != null) {
			Debug.Log ("deselecting " + activePlaygroundObj.name);
			activePlaygroundObj.SetActive (false);
		}

		if (selectedVariable != null) {
			selectedVariable.GetComponent<VariablePanel> ().Deselect ();
		}
		activePlaygroundObj = playgroundObj;
		Debug.Log ("enabling : " + activePlaygroundObj.name);
		activePlaygroundObj.SetActive (true);
		selectedVariable = varObj;
	}

	void Awake(){
		if (_instance != null) {
			Debug.Log ("Instance already exists!");
			return;
		}
		_instance = this;
		logicLayerDict = new Dictionary<string,GameObject> ();
		functionBoxList = new List<GameObject> ();
		utilityList = Resources.LoadAll<GameObject>("Logic/Utilities");
		variableList = new List<GameObject> ();
	}
	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.R))
			CreateSequenceOfFunctionBox ();
		if (Input.GetKeyDown (KeyCode.A))
			RetrieveVariables ();
	}

	public void SpawnBasicLayer(string phaseName)
	{
		GameObject basicLayerObj = Instantiate (basicLayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		basicLayerObj.transform.SetParent (layerParent, false);
		basicLayerObj.transform.localPosition = Vector3.zero;
		basicLayerObj.name = phaseName;
//		basicLayerObj.name = associatedObj.name + "_LogicLayer";
		logicLayerDict.Add (phaseName, basicLayerObj);
		//then set it as inactive unless the object is selected in the hierarchy
		basicLayerObj.SetActive (false);
	}

	void SpawnBasicBoxes()
	{
		Vector3 beginPos = Camera.main.ScreenToWorldPoint (beginBoxSpawnPos);
		beginBox = Instantiate(beginBoxPrefab,beginPos,Quaternion.identity) as GameObject;
		beginBox.transform.SetParent(logicParent,false);
		beginBox.transform.localPosition = beginPos;
		functionBoxList.Add (beginBox);

		Vector3 tickPos = Camera.main.ScreenToWorldPoint (tickBoxSpawnPos);
		tickBox = Instantiate(tickBoxPrefab,tickPos,Quaternion.identity) as GameObject;
		tickBox.transform.SetParent(logicParent,false);
		tickBox.transform.localPosition = tickPos;
		functionBoxList.Add (tickBox);
	}

	void RetrieveVariables()
	{
		for(int i=0;i<variableList.Count;i++)
		{
			Debug.Log ("variable type:" + variableList [i].GetComponent<VariablePanel> ().varType.ToString ());
		}
	}

	public void SwitchToLogicLayer(string objName)
	{
		if (activeLayer != null)
			activeLayer.SetActive (false);
		
		GameObject resultObj;
		bool result = logicLayerDict.TryGetValue (objName, out resultObj);
		if (result) {
			resultObj.SetActive (true);
			activeLayer = resultObj;
			logicParent = resultObj.transform;
		}
	}
//	{
	//	void OnGUI()
//		Event currentEvent = Event.current;
//		if (currentEvent.type == EventType.ScrollWheel) {
//			if (currentEvent.delta.y > 0f) {
//				canvasScale += 0.01f;
//
//			} else
//				canvasScale -= 0.01f;
//			canvasScale=Mathf.Clamp (canvasScale, 0.2f, 3f);
//			logicCanvas.GetComponent<CanvasScaler>().scaleFactor = canvasScale;
//		}
//	}
	List<FunctionBox> CreateSequenceOfFunctionBox()
	{
		//sort function boxes from left to right
		List<GameObject> sortedList=functionBoxList.OrderBy (o => o.transform.position.x).ToList ();
		List<FunctionBox> sortedCompList = new List<FunctionBox> ();
		for (int i = 0; i < sortedList.Count; i++) {
			sortedCompList.Add(sortedList [i].GetComponent<FunctionBox>());
		}
		return sortedCompList;
	}

	//executed by button "EXECUTE BUTTON"
	public void ExecuteLogic()
	{
		funcSequence = CreateSequenceOfFunctionBox ();
		StartCoroutine (PerformLogicSequence (funcSequence));
	}

	IEnumerator PerformLogicSequence(List<FunctionBox> funcSequence)
	{
		//executes the sequence of each function box in the sorted order
		for (int i = 0; i < funcSequence.Count; i++) {
			yield return StartCoroutine (funcSequence [i].ExecuteSequence ());
		}
		
		yield return null;
	}

	public void CreateNewVariable()
	{
		GameObject variableGroupObj = Instantiate (variableGroupPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		variableGroupObj.transform.parent = variableContent.transform;
		variableGroupObj.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (91.62f, 240f + (variableIndex * -90f), 0f);
		variableGroupObj.GetComponent<VariablePanel> ().varPlaygroundRef = varPlayground;
		variableGroupObj.GetComponent<VariablePanel> ().variableName.text = "Variable_"+variableIndex.ToString();
		variableList.Add (variableGroupObj);
		variableIndex++;

		//on updating
		if(OnVariableUpdate!=null)
			OnVariableUpdate ();
	}


	public void CreateFunctionBox()
	{
		GameObject spawnedBox = Instantiate(functionBoxPrefab,Camera.main.ScreenToWorldPoint(UtilityFunctions.GetMousePosInWorldCoords()),Quaternion.identity) as GameObject;
		spawnedBox.transform.SetParent(logicParent,false);
		spawnedBox.GetComponent<FunctionBox> ().SetupFunctionBox ("Function_" + index.ToString ());
		functionBoxList.Add (spawnedBox);
		index++;
	}

	public void CreateUtilityBox()
	{
			GameObject spawnedBox = Instantiate(utilityList[utilityDropdown.value],Camera.main.ScreenToWorldPoint(UtilityFunctions.GetMousePosInWorldCoords()),Quaternion.identity) as GameObject;
			spawnedBox.transform.SetParent(logicParent,false);
	}
}
