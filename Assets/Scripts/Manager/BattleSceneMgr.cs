﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

public class BattleSceneMgr : Singleton<BattleSceneMgr> {

	public int m_iDay = 0;
	public int m_iMeat = 0;
	public int m_iReward = 0;
	public MOUSE_STATE m_mouseState = MOUSE_STATE.NORMAL;
	public TURN_STATE m_turnState = TURN_STATE.END;
	public bool m_bBigSize = false;

	// Use this for initialization
	void Start () {
	}

	void EnemyGenerate()
	{
//		LevelGenerator.getInstance.Encount (AREA_STATE.FARM, 2);
	}

	void UpdateOrthographicCameraSize (float size) {
		Camera.main.orthographicSize = size;
	}

	public void EnemyEliminatedCheck()
	{
		StartCoroutine (EnemyEliminatedCheckCoroutine());
	}

	IEnumerator EnemyEliminatedCheckCoroutine()
	{
		yield return new WaitForSeconds(1f);
		
		Transform EnemyParent = GameObject.Find ("Enemies").transform;
		
		if (EnemyParent.childCount == 0) { // All Enemies Dead. Turn to night
			StartCoroutine (NightTurn ());
		}
	}

	public IEnumerator DayTurn()
	{
		m_iDay += 1;

//		LevelGenerator.getInstance.Encount (m_iDay, m_iDay);

		GameObject.Find ("MorgueToggle").GetComponent<UIPanel> ().alpha = 0;
		BattleSceneMgr.getInstance.m_turnState = TURN_STATE.DAY;

		yield return new WaitForSeconds(0.25f);

		StartCoroutine(GameObject.Find ("WantedReward").GetComponent<WantedReward> ().WantedPop ());
		StartCoroutine(GameObject.Find ("DayNotification").GetComponent<WantedReward> ().WantedPop ());

		//Clean Morgue
		Transform morgueTrans = GameObject.Find ("Morgue").transform;
		for (int i = 0; i < morgueTrans.childCount; ++i) {
			if(morgueTrans.GetChild(i).gameObject.name != "Poop")
			{
				ObjectFactory.getInstance.Create_Poop();
				yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
			}
		}
		morgueTrans.gameObject.BroadcastMessage("DestroyThis", SendMessageOptions.DontRequireReceiver);
		GameObject.Find("Morgue").GetComponent<Morgue>().ClearMorgue ();

		//Clean Buff Icons
		Transform BuffTrans = GameObject.Find("Buffs").transform;
		for (int i = 0; i < BuffTrans.childCount; ++i) {
			Destroy (BuffTrans.GetChild (i).gameObject);
		}
	}

	public IEnumerator NightTurn()
	{
		if (BattleSceneMgr.getInstance.m_turnState == TURN_STATE.NIGHT)
			yield break;

		if (GridMgr.getInstance.GetWidthOrHeightOfMonster () > 5) {
			m_bBigSize = true;
			GameObject.Find ("Morgue").transform.localPosition = new Vector3 (1.75f, 0.155f, 10f);
		} else {
			m_bBigSize = false;
			GameObject.Find ("Morgue").transform.localPosition = new Vector3 (1.5f, 0, 10f);
		}

		if (m_bBigSize)
			iTween.ValueTo (gameObject, iTween.Hash ("from", Camera.main.orthographicSize, "to", 0.72f, "Time", 1f, "onupdate", "TweenCamOrtho", "easetype", "easeInOutBack"));
		else
			iTween.ValueTo (gameObject, iTween.Hash ("from", Camera.main.orthographicSize, "to", 0.56f, "Time", 1f, "onupdate", "TweenCamOrtho", "easetype", "easeInOutBack"));
		
		ToggleMorgue (true);
		GameObject.Find ("MorgueToggle").GetComponent<UIPanel> ().alpha = 1;
		BattleSceneMgr.getInstance.m_turnState = TURN_STATE.NIGHT;
		yield return null;
	}

	void TweenCamOrtho(float newVal)
	{
		Camera.main.orthographicSize = newVal;
	}

//	IEnumerator CheckAssembleIsDone()
//	{
//		bool bAssembleIsDone = false;
//		Transform EnemyTrans = GameObject.Find ("Enemies").transform;
//
//		do {
//			if(EnemyTrans.childCount.Equals(0))
//				bAssembleIsDone = true;
//
//			yield return null;
//		} while(!bAssembleIsDone);
//
//		yield return new WaitForSeconds (1.5f);
//
//		EnemyGenerate ();
//		GameObject.Find("Core").BroadcastMessage("StopAssemble");
//		StartCoroutine (UserMove (true));
//	}

	public void StopAssemble()
	{
		StartCoroutine (StopAssemble_Coroutine ());
	}

	IEnumerator StopAssemble_Coroutine()
	{
		iTween.MoveTo (GameObject.Find ("Morgue"), iTween.Hash ("y" , -1.36f, "islocal", false, "time", 0.35f, "easetype", "easeOutBack"));
		yield return new WaitForSeconds(0.4f);

		iTween.ValueTo(Camera.main.gameObject, iTween.Hash ("from", Camera.main.orthographicSize, "to", 1.57f, "time", 1f, "easetype", "easeInOutBack", "onupdate", "UpdateOrthographicCameraSize",
		                                                    "onupdatetarget", gameObject));
		GameObject.Find("AssembleCurtains").BroadcastMessage("CurtainOff");
		yield return new WaitForSeconds(1f);

		GameObject.Find ("StopAssembleButton").GetComponent<UIPanel> ().alpha = 0;
		
		EnemyGenerate ();
		GameObject.Find("Player").BroadcastMessage("StopAssemble");

//		StartCoroutine (UserMove (true));
	}

	public void StartAssembleAfter(GameObject obj)
	{
		StartCoroutine (StartWhosAssemble (obj));
	}

	IEnumerator StartWhosAssemble(GameObject obj)
	{
		yield return new WaitForSeconds(0.1f);

		if(obj.GetComponent<Part> ().AssembleRoutine == null)
			obj.GetComponent<Part> ().AssembleRoutine = StartCoroutine (obj.GetComponent<Part> ().Assemble ());
	}

	public void PartDestroied()
	{
		StartCoroutine (PartDestroied_Coroutine());
	}

	IEnumerator PartDestroied_Coroutine()
	{
		yield return null;
		yield return null;

//		GameObject.Find("Core").BroadcastMessage("AmI_InCoreSide");
		GameObject.Find("Player").BroadcastMessage("DestroyPart_WhenPathBreaked");
	}

	public List<int> m_iOnFieldMeatCount;
	public void OnField(GameObject target)
	{
		StartCoroutine (target.GetComponent<Part>().OnField());
	}

	public bool MorgueFalse = false;
	public void ToggleMorgue(bool bOn)
	{
		Transform morgueTrans = GameObject.Find("Morgue").transform;
		GameObject grids = GameObject.Find("Grids").gameObject;

		if (bOn) {
			GameObject.Find ("Core").transform.position = new Vector3(0,0);
			GameObject.Find ("Core").GetComponent<Part>().m_iGridIdx = GridMgr.getInstance.GetGridIdx(new Vector2(0,0));

			GameObject.Find ("Morgue").BroadcastMessage ("Assemble", null, SendMessageOptions.DontRequireReceiver);
			GameObject.Find ("Player").BroadcastMessage ("Assemble", null, SendMessageOptions.DontRequireReceiver);

			StartCoroutine(GameObject.Find("glass").GetComponent<Glass>().ToggleColor(true));
	
			for(int i = 0 ; i < grids.transform.childCount; ++i)
			{
				grids.transform.GetChild(i).gameObject.SetActive(true);
				StartCoroutine(grids.transform.GetChild(i).GetComponent<DebugLine>().AlphToggle(true));
			}
			StartCoroutine(CamOffset_XChg(true));

			if(!m_bBigSize)
				iTween.MoveTo (morgueTrans.gameObject, iTween.Hash ("x",  0.52f, "y", 0f, "time", 0.25f, "easetype", "easeInSine", "islocal", true));
			else
				iTween.MoveTo (morgueTrans.gameObject, iTween.Hash ("x",  0.765f, "y", 0.155f, "time", 0.25f, "easetype", "easeInSine", "islocal", true));
		} else {
//			Transform WorldTrans = GameObject.Find ("World").transform;
//			for (int i = 0; i < WorldTrans.childCount; ++i) {
//				WorldTrans.GetChild (i).gameObject.SetActive (true);
//			}
//			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Battle");
//
//			StartCoroutine(GameObject.Find("glass").GetComponent<Glass>().ToggleColor(false));
//			GameObject.Find ("Player").BroadcastMessage("StopAssemble", SendMessageOptions.DontRequireReceiver);
//
//			for(int i = 0 ; i < grids.transform.childCount; ++i)
//			{
//				grids.transform.GetChild(i).gameObject.SetActive(true);
//				StartCoroutine(grids.transform.GetChild(i).GetComponent<DebugLine>().AlphToggle(false));
//			}
//			StartCoroutine(CamOffset_XChg(false));
//
//			if(!m_bBigSize)
//				iTween.MoveTo (morgueTrans.gameObject, iTween.Hash ("x", 1.5f, "y", 0f, "time", 0.25f, "easetype", "easeInSine","islocal", true));
//			else
//				iTween.MoveTo (morgueTrans.gameObject, iTween.Hash ("x", 1.75f, "y", 0.155f, "time", 0.25f, "easetype", "easeInSine","islocal", true));
//
//			GameObject.Find("PartBorder").GetComponent<SpriteRenderer>().enabled = false;
//
//			StartCoroutine(DayTurn());
		}
	}

	IEnumerator CamOffset_XChg(bool bMorgueOn)
	{
		if(bMorgueOn)
		{
			if (!m_bBigSize) {
				do {
					ProCamera2D.Instance.OffsetX += 0.04f;
					yield return null;
				} while(ProCamera2D.Instance.OffsetX < 0.5f);

				ProCamera2D.Instance.OffsetX = 0.5f;
			} else {
				do {
					ProCamera2D.Instance.OffsetX += 0.04f;
					yield return null;
				} while(ProCamera2D.Instance.OffsetX < 0.57f);

				ProCamera2D.Instance.OffsetX = 0.57f;
			}
		}else{
			do{
				ProCamera2D.Instance.OffsetX -= 0.04f;
				yield return null;
			}while(ProCamera2D.Instance.OffsetX > 0f);

			ProCamera2D.Instance.OffsetX = 0f;
		}
	}
}
