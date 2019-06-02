using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static List<Character> characters = new List<Character>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        characters.ForEach(delegate (Character character) {
            character.Update();
        });
    }

    public static void AppendCharacter(Character character)
    {
        if (character != null) {
            characters.Add(character);
        }
    }

    public static Character FindFirstCharacter(bool protagonist)
    {
        return characters.Find(x => x.protagonist == protagonist);
    }
}
