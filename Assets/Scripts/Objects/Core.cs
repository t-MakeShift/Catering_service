﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : Part {

	public int m_MoveCount = 1;

	// Use this for initialization
	void Start () {
		StartCoroutine (UserControl());

		m_bAttackAvailable = true;
		m_bFriendly = true;
		m_StickAvailableSeat = new List<int> ();
	}

	void OnDestroy()
	{
		StopAllCoroutines ();
	}

	TURN_STATE m_beforeState;
	int m_iCoreIdx;
	public List<int> m_StickAvailableSeat;

	IEnumerator UserControl()
	{
		GameMgr gMgr = GameMgr.getInstance;

		while (true) {

			if (gMgr.m_turnState.Equals (TURN_STATE.USER_MOVE)) {

				//Init
				if (m_beforeState != gMgr.m_turnState) {
					//최초 위치기억 
					m_iCoreIdx = GridMgr.getInstance.GetGridIdx (transform.position);
					m_iGridIdx = m_iCoreIdx;

					m_MoveCount = 2;
					GameObject.Find ("CanMove").GetComponent<UILabel> ().text = "MOVE : " + m_MoveCount;
				}

				if (Input.GetKeyDown (KeyCode.W)) {
					if (GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position + new Vector3 (0, GridMgr.getInstance.m_fYsize))) <= m_MoveCount) {
						transform.Translate (new Vector3 (0, GridMgr.getInstance.m_fYsize));
						GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position), true);
						m_iGridIdx = GridMgr.getInstance.GetGridIdx (transform.position);
					}
				}else if (Input.GetKeyDown (KeyCode.S)) {
					if (GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position - new Vector3 (0, GridMgr.getInstance.m_fYsize))) <= m_MoveCount) {
						transform.Translate (new Vector3 (0, -GridMgr.getInstance.m_fYsize));
						GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position), true);
						m_iGridIdx = GridMgr.getInstance.GetGridIdx (transform.position);
					}
				}else if (Input.GetKeyDown (KeyCode.A)) {
					if (GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position - new Vector3 (GridMgr.getInstance.m_fXsize, 0))) <= m_MoveCount) {
						transform.Translate (new Vector3 (-GridMgr.getInstance.m_fXsize, 0));
						GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position), true);
						m_iGridIdx = GridMgr.getInstance.GetGridIdx (transform.position);
					}
				}else if (Input.GetKeyDown (KeyCode.D)) {
					if (GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position + new Vector3 (GridMgr.getInstance.m_fXsize, 0))) <= m_MoveCount) {
						transform.Translate (new Vector3 (GridMgr.getInstance.m_fXsize, 0));
						GetShortestDistance (m_iCoreIdx, GridMgr.getInstance.GetGridIdx (transform.position), true);
						m_iGridIdx = GridMgr.getInstance.GetGridIdx (transform.position);
					}
				}
			}

			m_beforeState = gMgr.m_turnState;

			yield return null;
		};
	}

	int GetShortestDistance(int iStart, int iEnd, bool bWithLabel = false)
	{
		AStar.getInstance.AStarStart (iStart, iEnd);

		int iDistance = AStar.getInstance.m_BestList.Count;

		if (iStart == iEnd)
			iDistance = 0;

		if(bWithLabel)
			GameObject.Find ("CanMove").GetComponent<UILabel> ().text = "MOVE : " + (m_MoveCount - iDistance);

		return iDistance;
	}

	public void CalculateStickableSeat(bool bDragPart)
	{
		m_StickAvailableSeat.Clear ();
		List<int> TakenSeat = new List<int> ();
		
		int iPartGrid = 0;
		bool bEdgePart = false;
		GridMgr grid = GridMgr.getInstance;
		TakenSeat.Add (grid.GetGridIdx(transform.position));
		
		for (int i = 0; i < transform.childCount + 1; ++i) {
			bEdgePart = false;

			if(i == transform.childCount){
				iPartGrid = grid.GetGridIdx(transform.position);
			}else{
				iPartGrid = grid.GetGridIdx(transform.GetChild(i).position);
				bEdgePart = transform.GetChild(i).GetComponent<Part>().m_bEdgePart;
			}
			TakenSeat.Add(iPartGrid);

			///check A* CoreSide
			
			int iStart = iPartGrid;
			int iEnd = GridMgr.getInstance.GetGridIdx (transform.position);
			
			if (AStar.getInstance.AStarStart_CoreFind (iStart, iEnd) && !bEdgePart) { //코어 쪽 붙은애들에만 붙을수있음

				if(!m_StickAvailableSeat.Contains(iPartGrid-1))
					m_StickAvailableSeat.Add(iPartGrid-1);
				if(!m_StickAvailableSeat.Contains(iPartGrid+1))
					m_StickAvailableSeat.Add(iPartGrid+1);
				if(!m_StickAvailableSeat.Contains(iPartGrid-GridMgr.getInstance.m_iXcount))
					m_StickAvailableSeat.Add(iPartGrid-GridMgr.getInstance.m_iXcount);
				if(!m_StickAvailableSeat.Contains(iPartGrid+GridMgr.getInstance.m_iXcount))
					m_StickAvailableSeat.Add(iPartGrid+GridMgr.getInstance.m_iXcount);
			}

			for(int j = 0; j< TakenSeat.Count; ++j)
			{
				m_StickAvailableSeat.Remove(TakenSeat[j]);
			}

			
		}

		if (!bDragPart) {
			Transform StickableDot = GameObject.Find ("StickableDots").transform;

			for (int i = 0; i < StickableDot.childCount; ++i) {
				Destroy (StickableDot.GetChild (i).gameObject);
			}
		} else {
			for(int i = 0; i < m_StickAvailableSeat.Count; ++i)
			{
				ObjectFactory.getInstance.Create_StickableDot(m_StickAvailableSeat[i]);
			}
		}
	}
}
