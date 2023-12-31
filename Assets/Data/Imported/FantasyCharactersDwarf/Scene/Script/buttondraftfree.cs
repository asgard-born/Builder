﻿using UnityEngine;
using UnityEngine.UI;

namespace Data.Imported.FantasyCharactersDwarf.Scene.Script
{
	public class buttondraftfree : MonoBehaviour {

		public GameObject [] obj;
		public GameObject rotationButtonText;
		public int select = 0;
		public bool rotation = false;
		// Use this for initialization
		private void Start () {
			setActiveUnit (0);
		}

		// Update is called once per frame
		private void Update () {
			if (rotation) {
				transform.Rotate(Vector3.down * Time.deltaTime * 60.0f);
			}
		}

		private void setActiveUnit(int iselect){
			select = iselect;
			for (int i = 0; i < obj.Length; i++) {
				obj [i].SetActive (false);
			}
			obj [select].SetActive (true);
		}

		public void ButtonRotation (){
			rotation = !rotation;
			if (rotation) {
				rotationButtonText.GetComponent<Text> ().text = "Rotation ON";
			} else {
				rotationButtonText.GetComponent<Text> ().text = "Rotation OFF";
			}
		}

		public void ButtonModel0 (){
			setActiveUnit (0);
		}

		public void ButtonModel1 (){
			setActiveUnit (1);
		}

		public void ButtonModel2 (){
			setActiveUnit (2);
		}

		public void ButtonShowAllAnimation (){
			obj[select].GetComponent<Animator> ().Play ("all animation");
		}

		public void ButtonIdle1 (){
			obj[select].GetComponent<Animator> ().Play ("idle1");
		}
		public void ButtonIdle2 (){
			obj[select].GetComponent<Animator> ().Play ("idle2");
		}
		public void ButtonIdle3 (){
			obj[select].GetComponent<Animator> ().Play ("idle3");
		}
		public void ButtonIdle4 (){
			obj[select].GetComponent<Animator> ().Play ("idle4");
		}

		public void ButtonAttack1 (){
			obj[select].GetComponent<Animator> ().Play ("Attack1");
		}
		public void ButtonAttack2 (){
			obj[select].GetComponent<Animator> ().Play ("Attack2");
		}
		public void ButtonAttack3 (){
			obj[select].GetComponent<Animator> ().Play ("Attack3");
		}
		public void ButtonAttack4 (){
			obj[select].GetComponent<Animator> ().Play ("Attack4");
		}
		public void ButtonAttack5 (){
			obj[select].GetComponent<Animator> ().Play ("Attack5");
		}
		public void ButtonIdle_Attack (){
			obj[select].GetComponent<Animator> ().Play ("Idle_Attack");
		}
		public void ButtonCombat_run (){
			obj[select].GetComponent<Animator> ().Play ("Combat_run");
		}
		public void ButtonRun (){
			obj[select].GetComponent<Animator> ().Play ("Run");
		}
		public void ButtonWalk (){
			obj[select].GetComponent<Animator> ().Play ("Walk");
		}
		public void ButtonDeath1 (){
			obj[select].GetComponent<Animator> ().Play ("Death1");
		}
		public void ButtonDeath2 (){
			obj[select].GetComponent<Animator> ().Play ("Death2");
		}



	}
}

