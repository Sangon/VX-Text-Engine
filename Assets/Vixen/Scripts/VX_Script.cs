using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VX_Script", menuName = "Vixen/Script", order = 1)]
public class VX_Script : ScriptableObject {

    public List<VX_Event> EventHandles = new List<VX_Event>();

    public string[] Lines;
    public string[] Events;

    public string[] ParsedEvenNametList() {

        List<string> names = new List<string>();

        for (int i = 0; i < Events.Length; i++) {

            if (Events[i] != "") {

                names.Add(Events[i]);

            }

        }

        return names.ToArray();
    }
}
