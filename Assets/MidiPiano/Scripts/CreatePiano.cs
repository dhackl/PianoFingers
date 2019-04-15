using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePiano : MonoBehaviour
{

    public const int KEY_COUNT = 88;

    public GameObject keyboardObject;

    public Texture2D[] AllCubeTextures;
    public Texture2D TextureAtlas;
    public Rect[] AtlasUvs;

    void Start()
    {
        GenerateKeyboard();
    }

    private void GenerateKeyboard()
    {
        GameObject baseNoteObject = GameObject.Find("base_key");
        GameObject keyWhite = GameObject.Find("key_white");
        GameObject keyBlack = GameObject.Find("key_black");

        float spacing = 0.041f;
        float octave = 7 * spacing;
        float blackKeyOffsetY = -0.03f;
        float blackKeyOffsetX = -0.08f;

        int keyPos = 0;
        for (int i = 0; i < KEY_COUNT; i++)
        {
            if (!IsBlackKey(i))
            {
                Instantiate(keyWhite,
                    baseNoteObject.transform.position
                        + new Vector3(0, 0, 3 * octave)
                        - new Vector3(0, 0, keyPos * spacing), baseNoteObject.transform.rotation, keyboardObject.transform);
                keyPos++;
            }
            else
            {
                Instantiate(keyBlack,
                    baseNoteObject.transform.position
                    + new Vector3(0, 0, 3 * octave)
                    - new Vector3(blackKeyOffsetX, blackKeyOffsetY, (keyPos - 1) * spacing + (spacing / 2)), baseNoteObject.transform.rotation, keyboardObject.transform);
            }
        }

        baseNoteObject.transform.parent = null;
        Destroy(baseNoteObject);
    }

    private bool IsBlackKey(int key)
    {
        int octaveKey = key % 12;
        return (octaveKey == 1 || octaveKey == 3 || octaveKey == 6 || octaveKey == 8 || octaveKey == 10);
    }

}
