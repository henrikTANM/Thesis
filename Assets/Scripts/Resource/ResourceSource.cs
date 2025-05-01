using UnityEngine;

public class ResourceSource
{
    public string name;
    public bool active;
    public int cycleInterval;

    public virtual void SetActive(bool active, Planet planet, string message)
    {
        this.active = active;
        Debug.Log("Sent message from " + name);
    }

    public virtual void SetActive(bool active, string message)
    {
        this.active = active;
        Debug.Log("Sent message from " + name);
    }

    public virtual void DebugLog(string message) { Debug.Log(message); }
}
