using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class VX_TextObject : MonoBehaviour {

	#region Delegates
	public delegate void VX_TextObjectEvent();
	public event VX_TextObjectEvent OnTextUpdate;

	public event VX_TextObjectEvent OnLineStart;
	public event VX_TextObjectEvent OnLineEnd;

	public event VX_TextObjectEvent OnScriptLoad;
	public event VX_TextObjectEvent OnScriptChanged;
	public event VX_TextObjectEvent OnScriptUnload;
	public event VX_TextObjectEvent OnScriptEnd;
	public event VX_TextObjectEvent OnScriptStart;
	#endregion

	#region Public variables

	public VX_Script CurrentScript;

	public List<VX_Event> EventList = new List<VX_Event>();

	public float Speed = 0.0425f;
	public bool AutomaticStart;
	public bool Wordwrap;
	public bool Autoplay;

	public float AutoplayTimer = 0f;
	public float MaxAutoplaytimer = 5f;

	public Text Text;

	public bool Instant {

		get {
			return m_Instant;
		}

		set {
			m_Instant = value;
		}
	}

	public string CurrentText {

		get {
			return m_strBuffer == null ? "" : m_strBuffer.ToString();
		}

	}
	#endregion

	#region Private variables
	private int m_CurrentLine = 0;
	private StringBuilder m_strBuffer;
	[NonSerialized]
	private bool m_Instant;
	private string[] m_Linebuffer;
	private GUIStyle m_TextStyle = new GUIStyle();
	private GUIContent m_TextContent = new GUIContent();
	private Coroutine m_TextRoutine;
	private bool m_autoPlaytimerOn;
	#endregion

	void Start() {

		if (!Text) {
			Debug.LogError("No textobject assigned to: " + name);
			return;
		}

		m_strBuffer = new StringBuilder();

		m_TextStyle.font = Text.font;
		m_TextStyle.fontSize = Text.fontSize;

		//CurrentScript.EventHandles.Add("Testevent", () => Text.color = Color.red);


		//If automatic start is on we check for running lineroutine just in case we hotswapped.
		if (AutomaticStart) {
			if (m_TextRoutine != null) {
				StopCoroutine(m_TextRoutine);
				m_TextRoutine = null;
			}
			LoadNextLine();
		}

		//Debug things
#if UNITY_EDITOR
		OnScriptEnd += () => Debug.Log("Script " + CurrentScript.name + " ended.");
		OnScriptStart += () => Debug.Log("Script " + CurrentScript.name + " started.");
		OnScriptLoad += () => Debug.Log("Script " + CurrentScript.name + " loaded.");
		OnLineEnd += () => Debug.Log("Line ended " + CurrentScript.name);
		OnLineEnd += () => m_autoPlaytimerOn = true; 
#endif

	}

	/// <summary>
	/// Loads the next line from current script.
	/// </summary>
	public void LoadNextLine() {

		//If the textroutine is not null there is still text left to display, we want to display all of it immediately and then return.
		//In the future we probably want to have a boolean here on whether or not to immediately start loading the next line.
		if (m_TextRoutine != null) {

			StopCoroutine(m_TextRoutine);
			m_TextRoutine = null;

			m_strBuffer.Remove(0, m_strBuffer.Length);

			for (int i = 0; i < m_Linebuffer.Length; i++) {
				m_strBuffer.Append(m_Linebuffer[i]);
			}

			if (OnTextUpdate != null) {
				OnTextUpdate();
			}

			return;

		}


		//Check if we're on the last line.
		if (m_CurrentLine < CurrentScript.Lines.Length) {


			//Setup everything and start the text loading coroutine.
			m_strBuffer.Remove(0, m_strBuffer.Length);
			m_CurrentLine++;

			if (OnLineStart != null) {
				OnLineStart();
			}

			//If text is instant just append everything.
			if (m_Instant) {

				Text.horizontalOverflow = HorizontalWrapMode.Wrap;
				m_strBuffer.Append(CurrentScript.Lines[m_CurrentLine - 1]);

				if (OnTextUpdate != null) {
					OnTextUpdate();
				}

			} else {


				//Currently does nothing as we validate the lines ourselves, probably want to ignore line validation if wordwrapping is off.
				if (!Wordwrap) {
					Text.horizontalOverflow = HorizontalWrapMode.Overflow;
				}

				m_Linebuffer = CurrentScript.Lines[m_CurrentLine - 1].Split(null);

				m_TextRoutine = StartCoroutine(LoadLine());

			}



		} else if (m_CurrentLine == CurrentScript.Lines.Length) {

			if (OnScriptEnd != null) {
				OnScriptEnd();
			}

		}

		//Broken event caller thing, fix.
		if (CurrentScript.Events.Length > m_CurrentLine - 1) {

			string[] Events = CurrentScript.Events[m_CurrentLine - 1].Replace(" ", "").Split(',');


			for (int i = 0; i < EventList.Count; i++) {
				if (EventList[i].Name == CurrentScript.Events[m_CurrentLine]) {
					if (EventList[i] != null) {
						EventList[i].Invoke();
					}
				}
			}


		}


	}

	/// <summary>
	/// Loads a new script.
	/// </summary>
	/// <param name="script">Script object to load</param>
	public void LoadScript(VX_Script script) {

		CurrentScript = script;
		m_CurrentLine = 0;

		if (AutomaticStart) {

			if (m_TextRoutine != null) {
				StopCoroutine(m_TextRoutine);
				m_TextRoutine = null;
			}

			LoadNextLine();
		}

		if (OnScriptLoad != null) {
			OnScriptLoad();
		}

	}


	/// <summary>
	/// If we have an textobject attached, update it!
	/// </summary>
	private void UpdateTextObject() {

		if (Text) {
			Text.text = m_strBuffer.ToString();
		}

	}

	public void Update() {

		if (m_autoPlaytimerOn) {
			HandleAutoplay();
		}

	}



	#region Private methods
	private void HandleAutoplay() {

		AutoplayTimer += Time.deltaTime;
		if (AutoplayTimer > MaxAutoplaytimer) {
			m_autoPlaytimerOn = false;
			AutoplayTimer = 0f;
			LoadNextLine();
		}

	}


	//Coroutine to load the next line incrementally.
	private IEnumerator LoadLine() {

		int WordCursor = 0;
		int CharacterCursor = 0;

		//May want to disable if not wordwrapping.
		ValidateLines();

		while (WordCursor < m_Linebuffer.Length) {

			CharacterCursor = 0;


			while (CharacterCursor < m_Linebuffer[WordCursor].Length) {

				m_strBuffer.Append(m_Linebuffer[WordCursor][CharacterCursor]);
				CharacterCursor++;

				if (OnTextUpdate != null) {
					OnTextUpdate();
				}

				yield return new WaitForSeconds(Speed);
			}

			WordCursor++;
		}

		if (OnLineEnd != null) {
			OnLineEnd();
		}

		m_TextRoutine = null;

	}

	/// <summary>
	///Checks if the current line fits within the given textcontext and prunes it if necessary.
	/// </summary>
	private void ValidateLines() {

		m_TextContent.text = m_Linebuffer[0] + " ";

		for (int i = 0; i < m_Linebuffer.Length - 1; i++) {

			m_TextContent.text += m_Linebuffer[i + 1] + " ";

			if (m_TextStyle.CalcSize(m_TextContent).x > Text.GetComponent<RectTransform>().rect.width) {

				m_Linebuffer[i] = m_Linebuffer[i] + "\n"; ;
				m_TextContent.text = m_Linebuffer[i + 1];


			} else {
				m_Linebuffer[i] = m_Linebuffer[i] + " ";
			}
		}
	}


	private void OnEnable() {
		OnTextUpdate += UpdateTextObject;
	}

	private void OnDisable() {
		OnTextUpdate -= UpdateTextObject;
	}

	#endregion

}