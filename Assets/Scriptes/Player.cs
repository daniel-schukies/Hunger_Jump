﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour 
{
	public ParticleSystem PukeSystem;
	public GameObject PlayerRotationObj;
	public float RotationSpeed = 0.1f;
	public float MaxRotationAngle = 40;

	public GameObject Camera;
	public MoveDeadzone deadzone;
	public Spawner spawner;

    public float PlayerSize = 1;
    private float amountFoodEaten = 0;
    public Text scoreUiToUpdate;

    private float timeStampStart;
    public float slowSpeed = 4;
    public float normalSpeed = 9.81f;
    public float timeToReAccelerate = 3;

    private float camStartScale;
    private Vector3 playerStartScale;

    public Animator eating;

	public float NewCameraScale;
	public float OldCameraScale;

	public Vector3 NewPlayerScale;
	public Vector3 OldPlayerScale;

	public Vector2 RotationRange = new Vector2(320, 20);

	bool lockRot = false;
	bool crash = false;

	// Use this for initialization
	void Start () {
        Camera camData = Camera.GetComponent<Camera>();

        NewPlayerScale = transform.localScale;
		OldPlayerScale = transform.localScale;
		NewCameraScale = camData.orthographicSize;
		OldCameraScale = camData.orthographicSize;

        playerStartScale = transform.localScale;
        camStartScale = camData.orthographicSize;
		Scale ();
	}
	
	// Update is called once per frame
	void Update () 
	{
	}

	void FixedUpdate()
	{
		Physics2D.gravity = new Vector2(0, Mathf.Lerp(slowSpeed,normalSpeed, Mathf.Clamp01(Time.time/(timeStampStart + timeToReAccelerate))));



		transform.localScale = Vector3.Lerp (OldPlayerScale, NewPlayerScale, Time.deltaTime);

		//Debug.Log ("Log: " + transform.localScale.ToString());
		//Debug.Log ("Old" + OldPlayerScale + "New: " + NewPlayerScale);
		Camera.GetComponent<Camera> ().orthographicSize = Mathf.Lerp(OldCameraScale, NewCameraScale, Time.deltaTime);

		Debug.Log("CameraSize: " + Camera.GetComponent<Camera>().orthographicSize + "TargetSize: " + NewCameraScale + "OriginTarget: " + OldCameraScale);

		float horizontalInput = Input.GetAxis("Horizontal");

		// Rotate Player
		if (horizontalInput > 0) 
		{
			if (!lockRot) 
			{
				PlayerRotationObj.transform.Rotate (0, 0, RotationSpeed * horizontalInput);
			}

			if( PlayerRotationObj.transform.rotation.eulerAngles.z < RotationRange.x && PlayerRotationObj.transform.rotation.eulerAngles.z > RotationRange.y)
			{
				PlayerRotationObj.transform.Rotate (0, 0, -(RotationSpeed * horizontalInput));
				//Debug.Log ("++Angle" + (RotationSpeed * horizontalInput));
				this.lockRot = false;
			}
		} 
		else 
		{
			if (!lockRot) 
			{
				PlayerRotationObj.transform.Rotate (0, 0, RotationSpeed * horizontalInput);
			}

			if( PlayerRotationObj.transform.rotation.eulerAngles.z < RotationRange.x && PlayerRotationObj.transform.rotation.eulerAngles.z > RotationRange.y)
			{
				PlayerRotationObj.transform.Rotate (0, 0, -(RotationSpeed * horizontalInput));

				this.lockRot = false;
			}
		}

		if( PlayerRotationObj.transform.rotation.eulerAngles.z < RotationRange.x && PlayerRotationObj.transform.rotation.eulerAngles.z > RotationRange.y)
		{
			this.lockRot = true;
		}

		if (crash) 
		{
			transform.Rotate( new Vector3(0,0, 30));
		}

	}

	public void StartPage() {
		print("in StartPage()");
		StartCoroutine(FinishFirst(5.0f));
	}

	IEnumerator FinishFirst(float waitTime) {
		print("in FinishFirst");
		yield return new WaitForSeconds(waitTime);
		print("leave FinishFirst");
		EndCrash();
	}

	void DoLast() {
		print("do after everything is finished");
		print("done");
	}

	void EndCrash()
	{
		crash = false;
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		StartCoroutine(FinishFirst(2.0f));


		if (col.gameObject.tag == "food" || col.gameObject.tag == "trash") {
			Food foodData = col.gameObject.GetComponent<Food> ();

			if (foodData.eatSize <= PlayerSize) {
				if (col.gameObject.tag == "trash") {
					PukeSystem.Play ();
					amountFoodEaten -= foodData.eatSize * foodData.eatSize;
				} else {
					amountFoodEaten += foodData.eatSize * foodData.eatSize;
				}

				foodData.respawn ();
				updatePlayerSize ();
				this.Scale ();
				this.ScaleCamera ();
				eating.SetTrigger ("isEating");
			}

			if (foodData.eatSize > PlayerSize) {
				timeStampStart = Time.time;
			}
		} 
		else 
		{
			crash = true;
			PukeSystem.Play ();
			amountFoodEaten -= amountFoodEaten/100 * 5;
		}

        if(col.gameObject.tag == "finish")
        {
            SceneManager.LoadScene("title");
        }
	}

    private void updatePlayerSize()
    {
        PlayerSize = Mathf.Log(amountFoodEaten,2) + 1;
        scoreUiToUpdate.text = (int)((PlayerSize - 1) * 100) + "";
        Debug.Log(PlayerSize);
    }
	public void Scale()
	{
		OldPlayerScale = transform.localScale;

		NewPlayerScale = playerStartScale * PlayerSize * 3;
		//Debug.Log ("ScalePlayer");
	}

	private void ScaleCamera()
	{
		OldCameraScale = Camera.GetComponent<Camera> ().orthographicSize;
		NewCameraScale =  PlayerSize * camStartScale;
		spawner.RecalculateSpawnPosition ();
		deadzone.RecalculatePosition ();
	}
}
