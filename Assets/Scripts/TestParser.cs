using System.Collections.Generic;
using BOYAREngine.Parser;
using UnityEngine;
using UnityEngine.UI;

public class TestParser : MonoBehaviour
{
    [SerializeField] private List<Round> _rounds;

    private void Start()
    {
        var p = new SiqParser();
        //p.NewParser(@"F:\!Source\DELETE\SG_SI_Final_33.siq");
        //p.NewParser(@"F:\!Source\DELETE\TestPackage.siq");
        //p.NewParser(@"F:\!Source\DELETE\Solyanka_na_eruditsiyu.siq");
        p.NewParser(@"F:\!Source\DELETE\Voprosy_SIGame.siq");
        

        _rounds = p.Rounds;

//        NativeGallery.Permission permission = NativeGallery.GetMixedMediaFromGallery((path) =>
//        {
//            Debug.Log("Image path: " + path);
//            if (path != null)
//            {
//                // Create Texture from selected image
//                Texture2D texture = NativeGallery.LoadImageAtPath(path, 512);
//            }
//        }, NativeGallery.MediaType.Image, "Siq package");
    }
}
