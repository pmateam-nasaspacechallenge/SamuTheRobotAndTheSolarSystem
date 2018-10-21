using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{

    // singleton
    public static Game Instance { get; set; }

    // current AR object
    public GameObject CurrentGameObject { get; set; }

    // current data object
    public NasaObject CurrentNasaObject { get; set; }

    // speed on wich the item rotates
    public float Speed;

    // Scale on wich zoom will affect the scale of an object
    public float ZoomScale;

    // Default Scale
    public float DefaultScale;

    // UI controls to be hidden when no object is set
    public GameObject UIControls;

    // Text controls
    public GameObject TextControls;

    public GameObject Assistant;

    public Text FactText;

    public Text FactTitle;

    private int factIndex = 0;

    // flag that indicates the items rotation status
    public bool ShouldRotate { get; set; }

    public void SetGameObject(GameObject _obj)
    {
        Assistant.SetActive(true);
        Debug.Log("GAME: " + _obj.name);

        CurrentGameObject = GameObject.FindGameObjectWithTag(_obj.name);
        try
        {
            CurrentNasaObject = GetNasaObject(_obj.name);
            ShouldRotate = true;
        }
        catch (Exception ex)
        {

        }

        if (!TextControls.activeSelf)
        {
            UIControls.SetActive(true);
        }
    }

    public void UnsetGameObject()
    {
        Assistant.SetActive(false);
        CurrentGameObject.transform.localScale = new Vector3(DefaultScale, DefaultScale, DefaultScale);

        ShouldRotate = false;
        CurrentGameObject = null;
        CurrentNasaObject = null;

        UIControls.SetActive(false);
    }

    private NasaObject GetNasaObject(string name)
    {
        return MAP[name];
    }

    // toggle rotation on object
    public void ToggleRotation()
    {
        ShouldRotate = !ShouldRotate;
    }

    // zoom in
    public void ZoomIn()
    {
        Debug.Log("ZOOM IN");
        if (CurrentGameObject)
        {
            CurrentGameObject.transform.localScale += new Vector3(ZoomScale, ZoomScale, ZoomScale);
        }
        else
        {
            Debug.Log("GAME: NO OBJECT");
        }
    }

    // zoom out
    public void ZoomOut()
    {
        Debug.Log("ZOOM OUT");
        if (CurrentGameObject)
        {
            var CurrentScale = CurrentGameObject.transform.localScale;

            float _Scale = CurrentScale.x - ZoomScale > 0 ? ZoomScale : 0;

            CurrentGameObject.transform.localScale -= new Vector3(_Scale, _Scale, _Scale);
        }
        else
        {
            Debug.Log("GAME: NO OBJECT");
        }
    }

    // speak
    public void Speak(string text)
    {

    }

    public void ShowFact()
    {
        if (CurrentNasaObject != null)
        {
            UIControls.SetActive(false);
            FactTitle.text = CurrentNasaObject.Name;
            FactText.text = GetFact();

            TextControls.SetActive(true);
        }
    }

    public void HideFact()
    {
        TextControls.SetActive(false);

        if (CurrentGameObject)
        {
            UIControls.SetActive(true);
        }
    }

    public string GetCurrentFact()
    {
        string output = null;

        if(CurrentNasaObject!=null)
        {
            output = CurrentNasaObject.Facts[factIndex];
        }

        return output;
    }

    // get fact
    public string GetFact()
    {
        string output = "";
        if (CurrentNasaObject != null && CurrentGameObject != null)
        {
            System.Random rand = new System.Random();

            factIndex = (factIndex + 1) % 2;

            output = CurrentNasaObject.Facts[factIndex];
        }

        return output;
    }

    // Use this for initialization
    void Start()
    {
        Instance = this;
        UIControls.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldRotate && CurrentGameObject != null)
        {
            CurrentGameObject.transform.Rotate(Vector3.up, Speed * Time.deltaTime);
        }
    }

    public static Dictionary<string, NasaObject> MAP = new Dictionary<string, NasaObject>() {
        { "hubble",
            new NasaObject()
            {
                Name="Hubble",
                Facts=new string []
                {
                    "The hubble uses about 2800 watts, while a typical kitchen kettle is rated at 2200 watts",
                    "The hubble's speed around the world is 28000 km/h, twelve times faster than the Concorde"
                }
            }
        },
        { "sun",
            new NasaObject()
            {
                Name="Sun",
                Facts=new string []
                {
                    "The sun is approximately 4.5 billion years old",
                    "The sum is a yellow dwarf"
                }
            }
        },
        { "mercury",
            new NasaObject()
            {
                Name="Mercury",
                Facts=new string []
                {
                    "The smallest planet of the solar system",
                    "Shortest orbit in the solar system"
                }
            }
        },
        { "venus",
            new NasaObject()
            {
                Name="Venus",
                Facts=new string []
                {
                    "The hottest planet of the solar system",
                    "Atmosphere mostly composed of carbon dioxide"
                }
            }
        },
        { "earth",
            new NasaObject()
            {
                Name="Earth",
                Facts=new string []
                {
                    "Earth has an enormous water system",
                    "Earth is the only known planet to contain life"
                }
            }
        },
        { "mars",
            new NasaObject()
            {
                Name="Mars",
                Facts=new string []
                {
                    "Mars might have supported life 3.7 billion years ago",
                    "Mars might have had a watery of icy surface"
                }
            }
        },
        { "jupiter",
            new NasaObject()
            {
                Name="Jupiter",
                Facts=new string []
                {
                    "Jupiter has 53 named moons",
                    "Jupiter is mostly made of Helium and Hydrogen"
                }
            }
        },
        { "saturn",
            new NasaObject()
            {
                Name="Saturn",
                Facts=new string []
                {
                    "Saturn has 4 groups of rings and 9 named moons",
                    "Saturn is mostly made of Helium and Hydrogen"
                }
            }
        },
        { "uranus",
            new NasaObject()
            {
                Name="Uranus",
                Facts=new string []
                {
                    "Uranus is about four times wider than earth",
                    "Uranus is an ice giant"
                }
            }
        },
        { "neptune",
            new NasaObject()
            {
                Name="Neptune",
                Facts=new string []
                {
                    "Neptune is about four times wider than earth",
                    "Neptune is an ice giant"
                }
            }
        },
        { "pluto",
            new NasaObject()
            {
                Name="Pluto",
                Facts=new string []
                {
                    "Pluto is a dwarf planet",
                    "Pluto orbits the sun about 3.6 billion miles away"
                }
            }
        }
    };
}
