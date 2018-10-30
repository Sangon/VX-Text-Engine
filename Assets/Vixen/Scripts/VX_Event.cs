using UnityEngine.Events;
using System;

[Serializable]
public class VX_Event : UnityEvent {

    public string Name;
    public VX_Event() { }
    public VX_Event(string Name) {
        this.Name = Name;
    }

}