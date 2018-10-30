using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(VX_Event))]
public class NamedArrayDrawer : UnityEventDrawer {

public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {


        try {

			//Just intercept the OnGUI call and modify the label to suit our needs.
			//Is easily modifiable for other things as well.
			base.OnGUI(rect, property, new GUIContent(property.FindPropertyRelative("Name").stringValue));

        } catch {

			//If for some god forsaken reason the event has been left unnamed, use the default label unity gives it.
			//Should probably throw a warning here to notify of the missing label.
            base.OnGUI(rect, property.FindPropertyRelative("Name"), label);
        }
    }

}
