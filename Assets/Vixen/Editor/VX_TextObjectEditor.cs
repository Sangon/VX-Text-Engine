using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(VX_TextObject))]
public class VX_TextObjectEditor : Editor {

    private bool m_Preview;
    private string m_LastScriptName = "";
    private SerializedProperty m_eventList;

    private void OnEnable() {
        m_eventList = serializedObject.FindProperty("EventList");
    }

    private bool UpdateEventList() {

        Debug.Log("Updating");
        var txobj = target as VX_TextObject;

		if (txobj.EventList == null) {
			return false;
		} else {

			txobj.EventList.Clear();

			for (int i = 0; i < txobj.CurrentScript.Events.Length; i++) {

				if (txobj.CurrentScript.Events[i] != "") {
					txobj.EventList.Add(new VX_Event(txobj.CurrentScript.Events[i]));
				}

			}

			return true;
		}


    }

    public override void OnInspectorGUI() {

        var txobj = target as VX_TextObject;

        if (txobj.CurrentScript != null && txobj.CurrentScript.name != m_LastScriptName) {

			if (UpdateEventList()) {
				m_LastScriptName = txobj.CurrentScript.name;
				Debug.Log("Updated the eventlist.");
			}

        } else if (txobj.CurrentScript == null && m_eventList.arraySize > 0) {
            Debug.Log("Cleared the eventlist. (" + m_LastScriptName + ")");
            m_LastScriptName = "";
            txobj.EventList.Clear();
        }

        //Output text field
        //TODO: Make different types of output possible here.
        txobj.Text = (Text)EditorGUILayout.ObjectField("Output", txobj.Text, typeof(Text), true);

        //Script field
        txobj.CurrentScript = (VX_Script)EditorGUILayout.ObjectField("Current script", txobj.CurrentScript, typeof(VX_Script), true);

        EditorGUILayout.Space();

        //Hide everything if there is no script assigned to the object. You can still modify these through scripts if necessary.
        if (txobj.CurrentScript) {

            txobj.AutomaticStart = EditorGUILayout.ToggleLeft("Start the script automatically", txobj.AutomaticStart);
            txobj.Instant = EditorGUILayout.ToggleLeft("Instant text", txobj.Instant);

            if (!txobj.Instant) {
                txobj.Speed = EditorGUILayout.Slider("Speed", txobj.Speed, 0, 1);
            }

            EditorGUILayout.Space();

            txobj.Wordwrap = EditorGUILayout.ToggleLeft("Wordwrap", txobj.Wordwrap);
            m_Preview = EditorGUILayout.Toggle("Preview", m_Preview);
            if (m_Preview) {

                if (txobj.CurrentText != null) {
                    EditorGUILayout.TextArea(txobj.CurrentText, GUILayout.Height(50f));
                } else {
                    EditorGUILayout.TextArea("");
                }

                Repaint();

            }



            if (GUI.changed && !Application.isPlaying) {
                EditorUtility.SetDirty(txobj);
                EditorSceneManager.MarkSceneDirty(txobj.gameObject.scene);
            }

            EditorGUILayout.Space();

            serializedObject.Update();

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("onEvent"), true);

            EditorGUILayout.PropertyField(m_eventList, new GUIContent("Events"), true);
        } else {

            EditorGUILayout.LabelField("Assign a script object to enable functionality.");

        }

        serializedObject.ApplyModifiedProperties();

    }

}
