using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActions : MonoBehaviour {

    public static UIActions Instance { get;set;}

    void Start() {
        Instance = this;
    }

	// Use this for initialization
	public void ZoomIn () {
        Game.Instance.ZoomIn();
	}

    // Update is called once per frame
    public void ZoomOut () {
        Game.Instance.ZoomOut();
    }

    public void ToggleRotation()
    {
        Game.Instance.ToggleRotation();
    }

    public void ToggleFact()
    {
        if(Game.Instance.TextControls.activeSelf)
        {
            Game.Instance.HideFact();
        }
        else
        {
            Game.Instance.ShowFact();
        }
    }

    public string GetFactString()
    {
        return Game.Instance.GetCurrentFact();
    }
}
