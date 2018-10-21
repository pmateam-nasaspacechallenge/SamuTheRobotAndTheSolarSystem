using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionScript : DefaultTrackableEventHandler
{
    // Use this for initialization
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        Game.Instance.SetGameObject(gameObject);
    }


    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();

        Game.Instance.UnsetGameObject();
    }
}
