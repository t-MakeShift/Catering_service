﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Freindly : FSM {


	// Use this for initialization
	void OnEnable () {
		SetState (AI_STATE.IDLE);
	}

	protected override IEnumerator State_Idle()
	{
		Transform EnemyTrans = GameObject.Find("Enemies").transform;
		bool bAttackAble = GetComponent<Part> ().m_bAttackAvailable;

		float fRange = 0f;
		if (bAttackAble) {
			StartCoroutine (AnimatorPosSet (GetComponent<Part> ().m_weaponType));

			Part attackPart = GetComponent<Part> ();
			fRange = (float)attackPart.m_dicStat ["Range"];
			for (int i = 0; i < attackPart.m_lstPartBuffed.Count; ++i) {
				fRange += attackPart.m_lstPartBuffed [i].m_dicStatBuff ["Range"];
			}
			fRange /= 100f;
		}

		do{
			yield return null;

			if(bAttackAble){
				for(int i =0 ; i < EnemyTrans.childCount; ++i)
				{
					if(Vector3.Distance(EnemyTrans.GetChild(i).transform.position, transform.position) < fRange && EnemyTrans.GetChild(i).gameObject.activeInHierarchy && EnemyTrans.GetChild(i).gameObject.GetComponent<Unit>().m_fCurHealth > 0)
					{
						m_target = EnemyTrans.GetChild(i).gameObject;
						m_AiState = AI_STATE.ATTACK;
					}
				}
			}
			
		}while(m_AiState == AI_STATE.IDLE);
		
		SetState (m_AiState);
	}

	public void Weapon_Attack(Part attackPart)
	{
		float fDamage = attackPart.m_dicStat ["Attack"];

		StartCoroutine (Attack (m_target, fDamage, true));
	}
	
	protected override IEnumerator State_Attack()
	{
		Unit targetUnit = m_target.GetComponent<Unit> ();
		Part attackPart = GetComponent<Part> ();


		float fDmg = attackPart.m_dicStat["Attack"];
		for(int i = 0 ; i < attackPart.m_lstPartBuffed.Count; ++i)
		{
			fDmg += attackPart.m_lstPartBuffed[i].m_dicStatBuff["Attack"];
		}

		float fAttackSpeed = attackPart.m_dicStat["AttackSpeed"];
		for(int i = 0 ; i < attackPart.m_lstPartBuffed.Count; ++i)
		{
			fAttackSpeed += attackPart.m_lstPartBuffed[i].m_dicStatBuff["AttackSpeed"];
		}

		fAttackSpeed /= 10f;

		Animator anim = transform.GetChild(0).GetChild(0).GetComponent<Animator> ();
		anim.SetFloat ("AttackSpeed", fAttackSpeed);

		float fRange = (float)attackPart.m_dicStat ["Range"];
		for(int i = 0 ; i < attackPart.m_lstPartBuffed.Count; ++i)
		{
			fRange += attackPart.m_lstPartBuffed[i].m_dicStatBuff["Range"];
		}
		fRange /= 100f;

		while(m_AiState == AI_STATE.ATTACK){

			anim.SetBool ("Hit", false);
			anim.SetBool ("Ready_Weapon", true);
			anim.SetBool ("Cancel_Attack", false);

			float fTimer = 0f;

			do{
				fTimer += Time.deltaTime * fAttackSpeed * 0.1f;
				if(m_AiState != AI_STATE.ATTACK || targetUnit.m_fCurHealth <= 0f || m_target == null || !m_target.activeInHierarchy || Vector3.Distance(m_target.transform.position, transform.position) > fRange)
				{
					m_AiState = AI_STATE.IDLE;
					m_target = null;
					anim.SetBool ("Cancel_Attack", true);
					break;
				}
				yield return null;

			}while(fTimer < 1f);

			if (anim.GetBool ("Cancel_Attack")) {
				anim.SetBool ("Hit", false);
				anim.SetBool ("Ready_Weapon", false);
				break;
			}

			anim.SetBool ("Hit", true);
			yield return null;

//		do{
//			yield return null;
//
//			if(m_target == null || !m_target.activeInHierarchy)
//			{
//				m_AiState = AI_STATE.IDLE;
//				m_target = null;
//				break;
//			}
//			
//			iTween.RotateTo(gameObject, iTween.Hash("z",-100f,"time", fAttackSpeed * 0.2f));
//			yield return new WaitForSeconds((fAttackSpeed * 0.2f) + (fAttackSpeed * 0.1f));
//			iTween.RotateTo(gameObject, iTween.Hash("z",0f,"time", fAttackSpeed * 0.02f));
//
//			SoundMgr.getInstance.PlaySfx ("weapon_twohand");
//
//			yield return new WaitForSeconds(fAttackSpeed * 0.02f);
//			StartCoroutine(Attack(m_target, fDmg, true));
//			yield return new WaitForSeconds(fAttackSpeed * 0.1f); //Delay
//
//			if(targetUnit.m_fCurHealth <= 0)
//			{
//				m_AiState = AI_STATE.IDLE;
//				m_target = null;
//				break;
//			}
//
//			if(m_target == null || !m_target.activeInHierarchy)
//			{
//				m_AiState = AI_STATE.IDLE;
//				m_target = null;
//				break;
//			}
//
//			if(Vector3.Distance(m_target.transform.position, transform.position) > 0.5f)
//			{
//				m_AiState = AI_STATE.IDLE;
//				m_target = null;
//			}
//			
//		}while(m_AiState == AI_STATE.ATTACK);
		};

		anim.SetBool ("Ready_Weapon", false);
		
		SetState (m_AiState);
	}

	//FSM_MainScene_Core 에도 있음
	IEnumerator AnimatorPosSet(WEAPON_TYPE weaponType)
	{
		yield return null;

//		transform.GetChild (0).localPosition = -transform.GetChild (0).GetChild (0).localPosition;

		switch (weaponType) {
		case WEAPON_TYPE.ONE_HAND:
			transform.GetChild (0).localPosition = new Vector3 (0.128f, -0.085f);
			break;

		case WEAPON_TYPE.TWO_HAND:
			transform.GetChild (0).localPosition = new Vector3 (0.128f, -0.072f);
			break;

		case WEAPON_TYPE.POLE:
			transform.GetChild (0).localPosition = new Vector3 (0.128f, -0.072f);
			break;

		case WEAPON_TYPE.BOW:
			transform.GetChild (0).localPosition = new Vector3 (0.128f, -0.088f);
			break;

		case WEAPON_TYPE.CROSSBOW:
			transform.GetChild (0).localPosition = -transform.GetChild (0).GetChild (0).localPosition;
			break;

		}
	}
}
