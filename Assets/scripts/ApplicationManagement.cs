using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public enum CreationState
{
	BASE,
	OBJECT_CREATION,
	CUSTOMIZATION,
	VISUALIZATION
};

public enum CameraProjection
{
	PERSPECTIVE,
	ORTHOGRAPHIC
};

public class ApplicationManagement : MonoBehaviour {
	
	CreationState m_currentCreationState;
	Dropdown m_dropdownCreationState;

	GameObject m_cameraPerspObject;
	Camera m_cameraPersp;
	GameObject m_cameraOrthoObject;
	Camera m_cameraOrtho;
	Camera m_currentCamera;
	GameObject m_currentCameraObject;

	Vector3 mouseOrigin;
	bool m_isRotating;
	bool m_isTranslating;
	float m_xyTranslationSpeed;
	float m_zTranslationSpeed;
	float m_rotatingSpeed;

	CameraProjection m_currentCameraProjection;

	GameObject m_creationPanel;
	bool m_canSelectObject;
	GameObject m_selectedObject;
	Material m_selectedMaterial;
	Material m_selectedOldMaterial;
	Material m_defaultWallMaterial;

	GameObject m_buildingContainer;
	Dictionary<String, LinkedList<GameObject>> m_building;
	Text m_levelDisplayText;

	public void Start()
	{
		m_currentCreationState = CreationState.VISUALIZATION;
		m_dropdownCreationState = GameObject.Find ("dropdown_creation_states").GetComponent<Dropdown>();

		m_currentCameraProjection = CameraProjection.PERSPECTIVE;
		m_cameraPerspObject = GameObject.Find ("camera_persp");
		m_cameraPersp = m_cameraPerspObject.GetComponent<Camera> ();
		m_cameraOrthoObject = GameObject.Find ("camera_ortho");
		m_cameraOrtho = m_cameraOrthoObject.GetComponent<Camera> ();
		m_cameraOrthoObject.SetActive (false);
		m_currentCamera = m_cameraPersp;
		m_currentCameraObject = m_cameraPerspObject;

		m_isTranslating = false;
		m_isRotating = false;
		m_xyTranslationSpeed = 1;
		m_zTranslationSpeed = 30;
		m_rotatingSpeed = 5;

		m_creationPanel = GameObject.Find ("panel_building_creation");
		m_canSelectObject = false;
		Shader.EnableKeyword ("_EMISSION");
		m_selectedMaterial = Resources.Load("stripes_mat", typeof(Material)) as Material;
		m_defaultWallMaterial = Resources.Load ("Default-Material", typeof(Material)) as Material;

		m_levelDisplayText = GameObject.Find ("label_level_display").GetComponent<Text>();


		m_buildingContainer = GameObject.Find ("building");
		m_building = new Dictionary<string, LinkedList<GameObject>> ();
		/*LinkedList<GameObject> list = new LinkedList<GameObject> ();
		GetAllChildrenWithTag (list, m_buildingContainer, "Construction");
		m_building.Add ("*", list);

		m_levelDisplayText.text = "*";*/
	}

	public void Update()
	{		
		float xAxisValue = Input.GetAxis("Horizontal");
		float zAxisValue = Input.GetAxis("Vertical");
		float mouseWheelValue = Input.GetAxis ("Mouse ScrollWheel");

		if (Input.GetMouseButtonDown(0) && m_canSelectObject)
		{
			/* Left click */
			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(m_currentCamera.ScreenPointToRay(Input.mousePosition), out hitInfo);
			if (hit) 
			{
				if (hitInfo.transform.gameObject.tag == "Construction")
				{
					GameObject o = hitInfo.transform.gameObject;
					if(m_selectedObject != null)
					{
						m_selectedObject.GetComponent<Renderer>().material = m_selectedOldMaterial;
						if(o.Equals(m_selectedObject))
						{
							m_selectedObject = null;
						}
						else
						{
							m_selectedObject = o;
							m_selectedOldMaterial = m_selectedObject.GetComponent<Renderer>().material;
							m_selectedObject.GetComponent<Renderer>().material = m_selectedMaterial;
						}
					}
					else{
						m_selectedObject = o;
						m_selectedOldMaterial = m_selectedObject.GetComponent<Renderer>().material;	
						m_selectedObject.GetComponent<Renderer>().material = m_selectedMaterial;
					}
				}
			}
		}
		
		if (Input.GetMouseButtonDown(1))
		{
			/* Right click */
			m_isRotating = true;
			mouseOrigin = Input.mousePosition;
		}
		
		if (Input.GetMouseButtonDown(2))
		{
			/* Middle click */
			m_isTranslating = true;
			mouseOrigin = Input.mousePosition;
		}
		
		if (!Input.GetMouseButton (1))
			m_isRotating = false;
		if (!Input.GetMouseButton (2))
			m_isTranslating = false;

		/* Translation with keys */
		m_currentCameraObject.transform.Translate(new Vector3 (xAxisValue, zAxisValue, 0.0f));

		/* Translation with mouse */
		TranslateIfPossible(m_isTranslating, mouseOrigin);

		if (m_currentCameraProjection.Equals (CameraProjection.ORTHOGRAPHIC)) 
		{
			Debug.Log("ortho");
			/* Orthographic y management */
			m_currentCamera.orthographicSize -= m_zTranslationSpeed * mouseWheelValue / 5;
			RotateCameraOrthoIfPossible(m_isRotating, mouseOrigin);
		} else
		{
			/* Perspective y management */
			m_cameraPersp.transform.Translate(new Vector3(0, 0, m_zTranslationSpeed * mouseWheelValue));
			RotateCameraPerspIfPossible(m_isRotating, mouseOrigin);	
		}

	}

	public void TranslateCamera(Vector3 _mouseOrigin)
	{
		Vector3 pos = m_currentCamera.ScreenToViewportPoint(- (Input.mousePosition - _mouseOrigin));
		Vector3 move = new Vector3(pos.x * m_xyTranslationSpeed, pos.y * m_xyTranslationSpeed, 0);
		
		m_currentCamera.transform.Translate(move, Space.Self);	
	}

	public void TranslateIfPossible(bool _isTranslating, Vector3 _mouseOrigin)
	{
		if (_isTranslating)
			TranslateCamera (_mouseOrigin);
	}

	public void RotateCameraOrtho(Vector3 _mouseOrigin)
	{
		Vector3 pos = m_currentCamera.ScreenToViewportPoint((Input.mousePosition - _mouseOrigin));
		
		m_currentCamera.transform.RotateAround(m_currentCamera.transform.position, Vector3.up, -pos.y * m_rotatingSpeed);
		m_currentCamera.transform.RotateAround(m_currentCamera.transform.position, Vector3.up, pos.x * m_rotatingSpeed);
	}
	
	public void RotateCameraOrthoIfPossible(bool _isRotating, Vector3 _mouseOrigin)
	{
		if (_isRotating)
			RotateCameraOrtho (_mouseOrigin);
	}

	public void RotateCameraPersp(Vector3 _mouseOrigin)
	{
		Vector3 pos = m_currentCamera.GetComponent<Camera>().ScreenToViewportPoint((Input.mousePosition - _mouseOrigin));
		
		m_currentCamera.transform.RotateAround(m_currentCamera.transform.position, m_currentCamera.transform.right, -pos.y * m_rotatingSpeed);
		m_currentCamera.transform.RotateAround(m_currentCamera.transform.position, Vector3.up, pos.x * m_rotatingSpeed);
	}
	
	public void RotateCameraPerspIfPossible(bool _isRotating, Vector3 _mouseOrigin)
	{
		if (_isRotating)
			RotateCameraPersp (_mouseOrigin);
	}

	public void OnCreateRoom(GameObject _object)
	{
		GameObject container = GameObject.Find("container_building_creation");
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = Input.mousePosition;
		//container.AddComponent(cube);
	}

	public void OnChangeCreationState()
	{
		/* chooses the opposite camera projection and change it afterward. */
		switch (m_dropdownCreationState.value) 
		{
			case 0:
				onChangeCameraProjection ();
				m_currentCreationState = CreationState.BASE;
				m_currentCameraProjection = CameraProjection.PERSPECTIVE;
				break;
			case 1 :
				m_currentCreationState = CreationState.OBJECT_CREATION;
				m_currentCameraProjection = CameraProjection.ORTHOGRAPHIC;
				break;
			case 2 :
				m_currentCreationState = CreationState.CUSTOMIZATION;
				m_currentCameraProjection = CameraProjection.ORTHOGRAPHIC;
				break;
			case 3 :
				m_currentCreationState = CreationState.VISUALIZATION;
				m_currentCameraProjection = CameraProjection.ORTHOGRAPHIC;
				break;
			default:
				break;
		}
		onChangeCameraProjection ();
	}

	public void onChangeCameraProjection()
	{
		if (!m_currentCreationState.Equals (CreationState.BASE)) {

			Text text = GameObject.Find ("text_camera_projection").GetComponent<Text> ();
			if (m_currentCameraProjection.Equals (CameraProjection.ORTHOGRAPHIC)) {
				text.text = "persp";
				m_currentCameraProjection = CameraProjection.PERSPECTIVE;
				m_cameraOrthoObject.SetActive (false);
				m_cameraPerspObject.SetActive (true);
				m_currentCamera = m_cameraPersp;
				m_currentCameraObject = m_cameraPerspObject;
			} else {
				text.text = "ortho";
				m_currentCameraProjection = CameraProjection.ORTHOGRAPHIC;
				m_cameraPerspObject.SetActive (false);
				m_cameraOrthoObject.SetActive (true);
				m_currentCamera = m_cameraOrtho;
				m_currentCameraObject = m_cameraOrthoObject;
			}
		}
	}


	public void onChangeLevelUp()
	{

	}
	
	public void GetAllChildren(LinkedList<GameObject> _list, GameObject _object)
	{
		_list.AddLast (_object);
		foreach (Transform t in _object.transform)
			GetAllChildren(_list, t.gameObject);
	}
	
	public void GetAllChildrenWithTag(LinkedList<GameObject> _list, GameObject _object, String _tag)
	{
		if(_object.tag.Equals(_tag))
			_list.AddLast (_object);
		foreach (Transform t in _object.transform)
			GetAllChildrenWithTag(_list, t.gameObject, _tag);
	}

	public void SetCanSelectObject(bool _bool)
	{
		if (m_currentCreationState.Equals (CreationState.BASE) || m_currentCreationState.Equals (CreationState.VISUALIZATION))
			m_canSelectObject = false;
		else
			m_canSelectObject = _bool;
	}

	public void OnQuitSoftware() {
		Application.Quit();
	}
}











