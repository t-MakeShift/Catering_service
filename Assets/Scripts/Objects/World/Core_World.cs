﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

public class Core_World : MonoBehaviour {
	
	public int m_iDestinationIdx;
	public List<int> m_listMoveIdx;

	bool m_bWasMovingBeforeChgedScene = false;

	public int m_iNeedHunger; // 여행에 필요한 허기 

	// Use this for initialization
	void Start()
	{
		m_listMoveIdx = new List<int> ();
		StartCoroutine (Idle ());
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			GameObject.Find("WorldMapManager").GetComponent<WorldMapManager>().Wait ();
			StartCoroutine(EatMyPart ());
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			GameObject Dest = GameObject.Find("Destination").gameObject;
			Dest.GetComponent<SpriteRenderer>().enabled = false;

			iTween.MoveTo(GameObject.Find("WorldOverview").transform.GetChild(0).gameObject, iTween.Hash("x", 150f, "time", 0.5f,"isLocal", true,  "easetype", "easeInSine"));

			Transform pathTrans = GameObject.Find ("Path").transform;
			for (int i = 0; i < pathTrans.childCount; ++i) {
				Destroy (pathTrans.GetChild (i).gameObject);
			}
		}
	}

	public IEnumerator EatMyPart()
	{
		Transform playerTrans = GameObject.Find ("Player").transform;
		List<GameObject> branchPartList = new List<GameObject> ();

		for (int i = 1; i < playerTrans.childCount; ++i) {
			if (!playerTrans.GetChild (i).GetComponent<Part> ().m_bHasChildPart) {
				branchPartList.Add (playerTrans.GetChild (i).gameObject);
			}
		}

		GameObject targetPart = branchPartList [Random.Range (0, branchPartList.Count)];
		Destroy(targetPart.GetComponent<Part>());
		targetPart.transform.position = UICamera.mainCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 10));
		targetPart.GetComponent<SpriteRenderer> ().sortingLayerName = "FrontObject";
		targetPart.transform.parent = UICamera.mainCamera.transform;
		targetPart.layer = 5;
		targetPart.SetActive (true);
		iTween.MoveTo (targetPart, iTween.Hash("x",-205f, "y", 175f, "easetype", "easeInCubic", "time", 0.65f, "isLocal", true));

		yield return new WaitForSeconds (0.65f);

		iTween.ColorTo (targetPart, iTween.Hash("a", 0f, "time", 0.4f));

		GameObject.Find ("Hunger").GetComponent<HungerUI> ().IncreaseHunger (100);
	}

	IEnumerator Idle()
	{
		WorldMapManager world = GameObject.Find("WorldMapManager").GetComponent<WorldMapManager>();
		GridMgr grid = GridMgr.getInstance;

		float fMouseTimer = 0f;
		bool bMouseTimerOn = false;
		Vector3 vecMouseClickedPos = Vector3.zero;
		Vector3 mousePosition = Vector3.zero;
		bool bOverviewOn = false;

		do{
			mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			if(bMouseTimerOn)
				fMouseTimer += Time.deltaTime;

			if(Input.GetMouseButtonDown(0) && UICamera.selectedObject == GameObject.Find("UI Root")){
				fMouseTimer = 0f;
				bMouseTimerOn = true;
				vecMouseClickedPos = mousePosition;
				if(bOverviewOn && UICamera.hoveredObject != GameObject.Find("WorldOverview").gameObject)
				{
					bOverviewOn = false;
					iTween.MoveTo(GameObject.Find("WorldOverview").transform.GetChild(0).gameObject, iTween.Hash("x", 152f, "time", 0.5f,"isLocal", true,  "easetype", "easeInSine"));
				}
			}else if(Input.GetMouseButtonUp(0))
			{
				bMouseTimerOn = false;

				if(FoW.FogOfWar.GetFogOfWarTeam(0) != null && FoW.FogOfWar.GetFogOfWarTeam(0).GetFogValue(grid.GetPosOfIdx(grid.GetGridIdx(vecMouseClickedPos))) > 250 && !(GameMgr.getInstance.m_bDeveloperMode && GameObject.Find("DeveloperTools").GetComponent<DeveloperTool>().m_bFogDisabled)){
					
						Transform pathTrans = GameObject.Find ("Path").transform;
						for (int i = 0; i < pathTrans.childCount; ++i) {
							Destroy (pathTrans.GetChild (i).gameObject);
						}

						GameObject Dest = GameObject.Find("Destination").gameObject;
						Dest.GetComponent<SpriteRenderer>().enabled = false;
				}else if(Vector3.Distance(vecMouseClickedPos, mousePosition) < 0.025f){
					m_listMoveIdx = AStar.getInstance.AStarStart_World(grid.GetGridIdx(gameObject.transform.position), grid.m_iGridIdx, true);
					m_iDestinationIdx = grid.m_iGridIdx;
					DrawPath();

					if(m_listMoveIdx.Count != 0) // Select WorldIcon
					{
						m_iNeedHunger = m_listMoveIdx.Count * 30;
						GameObject Dest = GameObject.Find("Destination").gameObject;
						Dest.GetComponent<SpriteRenderer>().enabled = true;
						Dest.transform.position = grid.GetPosOfIdx(grid.GetGridIdx(vecMouseClickedPos));

						GameObject.Find("WorldOverview").transform.GetChild(0).GetComponent<WorldOverView>().SelectWorldIcon(GameObject.Find("Geo").transform.GetChild(grid.m_iGridIdx).GetComponent<WorldGeo>());

						bOverviewOn = true;
						iTween.MoveTo(GameObject.Find("WorldOverview").transform.GetChild(0).gameObject, iTween.Hash("x", -150f, "time", 0.5f,"isLocal", true,  "easetype", "easeInSine"));

					}else{
						GameObject Dest = GameObject.Find("Destination").gameObject;
						Dest.GetComponent<SpriteRenderer>().enabled = false;
					}
				}
			}

			yield return null;

		}while(world.m_worldTurnState.Equals(WORLDTURN_STATE.IDLE));

		StartCoroutine (Move());
	}

	public void HungerCheck()
	{
		if (m_iNeedHunger > GameMgr.getInstance.m_iHunger) {
			//TODO: HUNGER CHECKER
			ObjectFactory.getInstance.Create_MessageBox_TwoButton(Localization.Get("Mbox_HungerChecker"), "HungerCheckerEnforcement", "DestroyMessageBox");
		} else
			MoveOrder ();
	}

	public void MoveOrder()
	{
		GameObject.Find("WorldMapManager").GetComponent<WorldMapManager>().m_worldTurnState = WORLDTURN_STATE.MOVE;

		GameObject Dest = GameObject.Find("Destination").gameObject;
		Dest.GetComponent<SpriteRenderer>().enabled = false;

		iTween.MoveTo(GameObject.Find("WorldOverview").transform.GetChild(0).gameObject, iTween.Hash("x", 152f, "time", 0.5f,"isLocal", true,  "easetype", "easeInSine"));

		Transform pathTrans = GameObject.Find ("Path").transform;
		for (int i = 0; i < pathTrans.childCount; ++i) {
			Destroy (pathTrans.GetChild (i).gameObject);
		}
	}

	IEnumerator Move(bool bWaitLittleMoment = false)
	{
		WorldMapManager world = GameObject.Find("WorldMapManager").GetComponent<WorldMapManager>();
		GridMgr grid = GridMgr.getInstance;

		if (m_listMoveIdx.Count > 0) {
			ProCamera2D.Instance.AdjustCameraTargetInfluence (ProCamera2D.Instance.CameraTargets [0], 1f, 1f);
			ProCamera2D.Instance.AdjustCameraTargetInfluence (ProCamera2D.Instance.CameraTargets [1], 0f, 0f);
		}

		if (bWaitLittleMoment)
			yield return new WaitForSeconds (0.5f);

		for(int i = 0; i < m_listMoveIdx.Count; ++i)
		{
			TimeMgr.getInstance.Play ();

			Vector3 destPos = grid.GetPosOfIdx(m_listMoveIdx[i]);
			iTween.MoveTo(gameObject, iTween.Hash("x", destPos.x, "y", destPos.y, "time", 1f, "easetype", "easeInSine"));

			GameMgr.getInstance.m_iHunger -= 30;

			if (GameMgr.getInstance.m_iHunger <= 0) {
				GameMgr.getInstance.m_iHunger = 100;
				//TODO: Eat Part
			}

			yield return new WaitForSeconds(1f);


			if(i != m_listMoveIdx.Count - 1)
				m_bWasMovingBeforeChgedScene = true;
			else
				m_bWasMovingBeforeChgedScene = false;

			if(CheckLocationBreak ())
				yield break;
		}

		ProCamera2D.Instance.AdjustCameraTargetInfluence (ProCamera2D.Instance.CameraTargets [0], 0f, 0f);
		ProCamera2D.Instance.AdjustCameraTargetInfluence (ProCamera2D.Instance.CameraTargets [1], 1f, 1f);
		GameObject.Find ("PC2DPanTarget").transform.position = gameObject.transform.position;

		world.m_worldTurnState = WORLDTURN_STATE.IDLE;
		StartCoroutine (Idle());
	}

	public bool CheckLocationBreak()
	{
		int iCoreIdx = GridMgr.getInstance.GetGridIdx (gameObject.transform.position);
		GameObject target = GameObject.Find ("Geo").transform.GetChild (iCoreIdx).GetComponent<WorldGeo> ().m_worldIcon;
		WorldIcon objIcon = target.GetComponent<WorldIcon> ();


		GameMgr gMgr = GameMgr.getInstance;
		gMgr.m_ilistCurEnemyList.Clear ();
		gMgr.m_ilistCurHeroList.Clear ();

		gMgr.m_ilistCurEnemyList = objIcon.m_list_enemyType;
		Transform PartyTrans = GameObject.Find ("Party").transform;

		string strDebug = "Enemy Encount : " + objIcon.m_list_enemyType.Count + " enemies in Local, ";

		WorldMapManager world = GameObject.Find ("WorldMapManager").GetComponent<WorldMapManager> ();
		for (int i = 0; i < PartyTrans.childCount; ++i) {
			Party party = PartyTrans.GetChild (i).GetComponent<Party> ();
			if(party.m_iGridIdx == objIcon.m_iGridIdx)
			{
				world.m_encountPartyList.Add (PartyTrans.GetChild (i).gameObject);

				for (int j = 0; j < party.m_list_enemyType.Count; ++j) {

					gMgr.m_ilistCurEnemyList.Add (party.m_list_enemyType [j]);

					if (party.m_list_enemyType [j] == (int)ENEMY_TYPE.HERO)
						gMgr.m_ilistCurHeroList.Add (party.m_iHero);
				}

				strDebug += party.m_list_enemyType.Count + " enemies in " + party.m_strPartyName + ", ";
			}
		}


		if (gMgr.m_ilistCurEnemyList.Count != 0 ) {

			GameObject.Find ("WorldMapManager").GetComponent<WorldMapManager> ().EncountEnemy ();
			Debug.Log (strDebug);
			return true;
		}

		return false;
	}

	void CameBackFromBattleScene()
	{
		if (m_bWasMovingBeforeChgedScene) {
			m_listMoveIdx = AStar.getInstance.AStarStart_World(GridMgr.getInstance.GetGridIdx(gameObject.transform.position), m_iDestinationIdx, true);
			StartCoroutine (Move (true));
		} else {
			ProCamera2D.Instance.AdjustCameraTargetInfluence (ProCamera2D.Instance.CameraTargets [0], 0f, 0f);
			ProCamera2D.Instance.AdjustCameraTargetInfluence (ProCamera2D.Instance.CameraTargets [1], 1f, 1f);
			GameObject.Find ("PC2DPanTarget").transform.position = gameObject.transform.position;

			GameObject.Find ("WorldMapManager").GetComponent<WorldMapManager> ().m_worldTurnState = WORLDTURN_STATE.IDLE;
			StartCoroutine (Idle ());
		}

		StartCoroutine (AdjustCamOffset ());
	}

	IEnumerator AdjustCamOffset()
	{
		yield return new WaitForSeconds (0.1f);

		ProCamera2D.Instance.OffsetX = 0f;
		ProCamera2D.Instance.OffsetY = 0f;
	}

	void DrawPath()
	{
		GridMgr grid = GridMgr.getInstance;

		Transform pathTrans = GameObject.Find ("Path").transform;
		for (int i = 0; i < pathTrans.childCount; ++i) {
			Destroy (pathTrans.GetChild (i).gameObject);
		}

		for (int i = 0; i < m_listMoveIdx.Count; ++i) {
			GameObject myLine = new GameObject ();
			myLine.transform.position = grid.GetPosOfIdx(m_listMoveIdx[i]);
			myLine.AddComponent<LineRenderer> ();
			LineRenderer lr = myLine.GetComponent<LineRenderer> ();
			lr.material = new Material (Shader.Find ("Sprites/Default"));
			lr.SetColors (new Color (119 / 255f, 43 / 255f, 130 / 255f), new Color (119 / 255f, 43 / 255f, 130 / 255f));
			lr.SetWidth (0.05f, 0.05f);
			if (i == 0) {
				lr.SetPosition (0, GameObject.Find("Core").transform.position);
				lr.SetPosition (1, grid.GetPosOfIdx (m_listMoveIdx [i]));
			} else {
				lr.SetPosition (0, grid.GetPosOfIdx (m_listMoveIdx [i - 1]));
				lr.SetPosition (1, grid.GetPosOfIdx (m_listMoveIdx [i]));
			}
			lr.sortingLayerName = "Default";
			lr.sortingOrder = -1;
			lr.transform.parent = GameObject.Find ("Path").transform;
		}
	}
}
