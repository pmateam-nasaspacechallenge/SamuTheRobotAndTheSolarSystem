using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public static int HOME_INDEX = 0;
    public static int STORY_INDEX = 2;

    public void Home()
    {
        LoadByIndex(HOME_INDEX);
    }

    public void Story()
    {
        LoadByIndex(STORY_INDEX);
    }
    

    public void Back()
    {
        if(Game.Instance.TextControls.activeSelf)
        {
            Game.Instance.HideFact();
        }
        else
        {
            Home();
        }
    }

    //
    // HELPERS
    //
    
    public void LoadByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        Screen.orientation = sceneIndex == STORY_INDEX ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
    }
}
