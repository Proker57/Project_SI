using System.Collections.Generic;
using BOYAREngine.Parser;
using UnityEngine;
using UnityEngine.UI;

public class TestParser : MonoBehaviour
{
    [SerializeField] private List<Round> _rounds;
    [SerializeField] private Image image;

    private void Start()
    {
        var p = new SiqParser();
        //p.Parser(@"F:\!Source\DELETE\SG_SI_Final_33.siq");
        //p.Parser(@"F:\!Source\DELETE\TestPackage.siq");
        //p.Parser(@"F:\!Source\DELETE\Solyanka_na_eruditsiyu.siq");
        p.Parser(@"F:\!Source\DELETE\Voprosy_SIGame.siq");
        //p.Parser(@"F:\!Source\DELETE\Ugaday_melodiyu.siq");
        


        _rounds = p.Rounds;
        //image.sprite = Sprite.Create(_rounds[0].Themes[1].Questions[0].Image, new Rect(0,0, _rounds[0].Themes[1].Questions[0].Image.width, _rounds[0].Themes[1].Questions[0].Image.height), new Vector2(0.5f, 0.5f));

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
